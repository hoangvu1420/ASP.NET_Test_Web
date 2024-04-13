using System.Linq.Expressions;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Stripe;
using Stripe.Checkout;
using TestApp.DataAccess.Repositories.IRepository;
using TestApp.Models;
using TestApp.Models.ViewModels;
using TestApp.Utilities;

namespace WebApplicationTest.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize]
public class OrderController : Controller
{
    private readonly IUnitOfWork _unitOfWork;
    [BindProperty]
    public OrderVM OrderVm { get; set; }

    public OrderController(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }
    
    // GET
    public IActionResult Index()
    {

        return View();
    }
    
    // GET
    public IActionResult Details(int? id)
    {
        if (id is null or 0)
        {
            return NotFound();
        }
        OrderVm = new OrderVM
        {
            OrderHeader = _unitOfWork.OrderHeader.Get(o => o.Id == id, includeProperties: "ApplicationUser"),
            OrderDetails = _unitOfWork.OrderDetail.GetRange(o => o.OrderHeaderId == id, includeProperties: "Product")
        };
        return View(OrderVm);
    }
    
    // POST
    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = StaticDetails.RoleAdmin+","+StaticDetails.RoleEmployee)]
    public IActionResult UpdateOrderDetails()
    {
        var orderHeaderFromDb = _unitOfWork.OrderHeader.Get(o => o.Id == OrderVm.OrderHeader.Id); // this is the order header from the database
        orderHeaderFromDb.FullName = OrderVm.OrderHeader.FullName;
        orderHeaderFromDb.PhoneNumber = OrderVm.OrderHeader.PhoneNumber;
        orderHeaderFromDb.StreetAddress = OrderVm.OrderHeader.StreetAddress;
        orderHeaderFromDb.City = OrderVm.OrderHeader.City;
        orderHeaderFromDb.State = OrderVm.OrderHeader.State;
        orderHeaderFromDb.PostalCode = OrderVm.OrderHeader.PostalCode;

        if (!string.IsNullOrEmpty(OrderVm.OrderHeader.Carrier))
        {
            orderHeaderFromDb.Carrier = OrderVm.OrderHeader.Carrier;
        }
        if (!string.IsNullOrEmpty(OrderVm.OrderHeader.TrackingNumber))
        {
            orderHeaderFromDb.TrackingNumber = OrderVm.OrderHeader.TrackingNumber;
        }
        _unitOfWork.OrderHeader.Update(orderHeaderFromDb);
        _unitOfWork.Save();
        
        TempData["success"] = "Order details updated successfully";

        return RedirectToAction(nameof(Details), new {id = orderHeaderFromDb.Id});
    }
    
    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = StaticDetails.RoleAdmin + "," + StaticDetails.RoleEmployee)]
    public IActionResult StartProcessing()
    {
        _unitOfWork.OrderHeader.UpdateStatus(OrderVm.OrderHeader.Id, StaticDetails.OrderStatusInProcess);
        _unitOfWork.Save();
        
        TempData["success"] = "Order is in process";
        return RedirectToAction(nameof(Details), new {id = OrderVm.OrderHeader.Id});
    }
    
    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = StaticDetails.RoleAdmin + "," + StaticDetails.RoleEmployee)]
    public IActionResult ShipOrder()
    {
        var orderHeader = _unitOfWork.OrderHeader.Get(o => o.Id == OrderVm.OrderHeader.Id);
        orderHeader.TrackingNumber = OrderVm.OrderHeader.TrackingNumber;
        orderHeader.Carrier = OrderVm.OrderHeader.Carrier;
        orderHeader.ShippingDate = DateTime.Now;
        orderHeader.OrderStatus = StaticDetails.OrderStatusShipped;
        if (orderHeader.PaymentStatus == StaticDetails.PaymentStatusDelayedPayment)
        {
            orderHeader.PaymentDueDate = DateOnly.FromDateTime(DateTime.Now.AddDays(30));
        }
        
        _unitOfWork.OrderHeader.Update(orderHeader);
        _unitOfWork.Save();
        
        TempData["success"] = "Order is shipped";
        return RedirectToAction(nameof(Details), new {id = OrderVm.OrderHeader.Id});
    }
    
    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = StaticDetails.RoleAdmin + "," + StaticDetails.RoleEmployee)]
    public IActionResult CancelOrder()
    {
        var orderHeader = _unitOfWork.OrderHeader.Get(o => o.Id == OrderVm.OrderHeader.Id);
        
        if (orderHeader.PaymentStatus == StaticDetails.PaymentStatusApproved)
        {
            // Refund the money
            var options = new RefundCreateOptions // this is from Stripe
            {
                Reason = RefundReasons.RequestedByCustomer,
                PaymentIntent = orderHeader.PaymentIntentId
            };
            var service = new RefundService();
            Refund refund = service.Create(options);
            
            _unitOfWork.OrderHeader.UpdateStatus(OrderVm.OrderHeader.Id, StaticDetails.OrderStatusRefunded);
        }
        else
        {
            _unitOfWork.OrderHeader.UpdateStatus(OrderVm.OrderHeader.Id, StaticDetails.OrderStatusCancelled);
        }
        _unitOfWork.Save();
        
        TempData["success"] = "Order is cancelled successfully";
        return RedirectToAction(nameof(Details), new {id = OrderVm.OrderHeader.Id});
    }

	[HttpPost]
	[ValidateAntiForgeryToken]
	public IActionResult PayNow()
	{
        OrderVm.OrderHeader = _unitOfWork.OrderHeader.Get(o => o.Id == OrderVm.OrderHeader.Id, includeProperties: "ApplicationUser");
        OrderVm.OrderDetails = _unitOfWork.OrderDetail.GetRange(o => o.OrderHeaderId == OrderVm.OrderHeader.Id, includeProperties: "Product");

		var domain = "https://localhost:7115/";
		var options = new SessionCreateOptions
		{
			SuccessUrl = domain + $"Admin/Order/PaymentConfirmation?orderHeaderId={OrderVm.OrderHeader.Id}",
			CancelUrl = domain + "Admin/Oder/Details?orderHeaderId={OrderVm.OrderHeader.Id}",
			LineItems = new List<SessionLineItemOptions>(),
			Mode = "payment"
		};

        var shoppingCarts = OrderVm.OrderDetails.ToList();
		foreach (var item in shoppingCarts)
		{
			var sessionLineItem = new SessionLineItemOptions
			{
				PriceData = new SessionLineItemPriceDataOptions
				{
					UnitAmount = (long)(item.Price * 100),
					Currency = "usd",
					ProductData = new SessionLineItemPriceDataProductDataOptions
					{
						Name = item.Product.Title
					}
				},
				Quantity = item.Count
			};
			options.LineItems.Add(sessionLineItem);
		}
		var service = new SessionService();
		Session session = service.Create(options);

		_unitOfWork.OrderHeader.UpdateStripePaymentId(OrderVm.OrderHeader.Id, session.Id, session.PaymentIntentId);
		_unitOfWork.Save();
		Response.Headers.Add("Location", session.Url);
		return new StatusCodeResult(303);
	}
    
    // GET
    public IActionResult PaymentConfirmation(int orderHeaderId)
    {
        OrderHeader orderHeader = _unitOfWork.OrderHeader.Get(o => o.Id == orderHeaderId);
        if(orderHeader.PaymentStatus == StaticDetails.PaymentStatusDelayedPayment)
        {
            var service = new SessionService();
            Session session = service.Get(orderHeader.SessionId);
            if (session.PaymentStatus.ToLower() == "paid")
            {
                _unitOfWork.OrderHeader.UpdateStripePaymentId(orderHeaderId, session.Id, session.PaymentIntentId);
                _unitOfWork.OrderHeader.UpdateStatus(orderHeaderId, orderHeader.OrderStatus, StaticDetails.PaymentStatusApproved);
                _unitOfWork.Save();
            }
        }
        
        return View(orderHeaderId);
    }

	#region API CALLS

	[HttpGet]
    public IActionResult GetAll(string status)
    {
        IEnumerable<OrderHeader> orderHeaders;
        if (User.IsInRole(StaticDetails.RoleAdmin) || User.IsInRole(StaticDetails.RoleEmployee))
        {
            orderHeaders = _unitOfWork.OrderHeader.GetAll(includeProperties: "ApplicationUser").ToList();
        }
        else
        {
            var claimsIdentity = (ClaimsIdentity) User.Identity!;
            var claim = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier)!.Value;
            orderHeaders = _unitOfWork.OrderHeader.GetRange(o => o.ApplicationUserId == claim, includeProperties: "ApplicationUser").ToList();
        }

        orderHeaders = status switch
        {
            "pending" => orderHeaders.Where(o => o.PaymentStatus == StaticDetails.PaymentStatusDelayedPayment),
            "inprocess" => orderHeaders.Where(o => o.PaymentStatus == StaticDetails.OrderStatusInProcess),
            "completed" => orderHeaders.Where(o => o.PaymentStatus == StaticDetails.OrderStatusShipped),
            "approved" => orderHeaders.Where(o => o.PaymentStatus == StaticDetails.OrderStatusApproved),
            _ => orderHeaders
        };

        return Json(new {data = orderHeaders});
    }

    #endregion
    
}
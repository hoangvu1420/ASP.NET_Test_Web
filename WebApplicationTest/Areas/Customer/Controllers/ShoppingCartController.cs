using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Stripe.Checkout;
using TestApp.DataAccess.Repositories.IRepository;
using TestApp.Models;
using TestApp.Models.ViewModels;
using TestApp.Utilities;

namespace WebApplicationTest.Areas.Customer.Controllers;

[Area("Customer")]
[Authorize]
public class ShoppingCartController : Controller
{
    private readonly IUnitOfWork _unitOfWork;
    [BindProperty] // This is used to bind the ShoppingCartVM object to the view, so that we can access it in the view and use it to display the data
    public ShoppingCartVM ShoppingCartVM { get; set; }

    public ShoppingCartController(IUnitOfWork unitOfWork, IConfiguration configuration)
    {
        _unitOfWork = unitOfWork;
    }

    // GET
    public IActionResult Index()
    {
        var claimsIdentity = (ClaimsIdentity)User.Identity!;
        var userId = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier)!.Value;

        ShoppingCartVM = new ShoppingCartVM
        {
            ListCart = _unitOfWork.ShoppingCart.GetRange(c => c.ApplicationUserId == userId,
                includeProperties: "Product"),
            OrderHeader = new OrderHeader {OrderTotal = 0}
        };
        foreach (var cart in ShoppingCartVM.ListCart)
        {
            cart.Price = CalculatePriceBasedOnQty(cart);
            ShoppingCartVM.OrderHeader.OrderTotal += cart.Price * cart.Count;
        }

        return View(ShoppingCartVM);
    }
    
    // GET
    public IActionResult Checkout()
    {
        var claimsIdentity = (ClaimsIdentity)User.Identity!;
        var userId = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier)!.Value;
        
        ShoppingCartVM = new ShoppingCartVM()
        {
            ListCart = _unitOfWork.ShoppingCart.GetRange(c => c.ApplicationUserId == userId,
                includeProperties: "Product"),
            OrderHeader = new OrderHeader
            {
                OrderTotal = 0,
                ApplicationUser = _unitOfWork.ApplicationUser.Get(u => u.Id == userId)
            }
        };

        ShoppingCartVM.OrderHeader.FullName = ShoppingCartVM.OrderHeader.ApplicationUser.Name;
        ShoppingCartVM.OrderHeader.PhoneNumber = ShoppingCartVM.OrderHeader.ApplicationUser.PhoneNumber;
        ShoppingCartVM.OrderHeader.StreetAddress = ShoppingCartVM.OrderHeader.ApplicationUser.StreetAddress;
        ShoppingCartVM.OrderHeader.City = ShoppingCartVM.OrderHeader.ApplicationUser.City;
        ShoppingCartVM.OrderHeader.State = ShoppingCartVM.OrderHeader.ApplicationUser.State;
        ShoppingCartVM.OrderHeader.PostalCode = ShoppingCartVM.OrderHeader.ApplicationUser.PostalCode;
        
        foreach (var cart in ShoppingCartVM.ListCart)
        {
            cart.Price = CalculatePriceBasedOnQty(cart);
            ShoppingCartVM.OrderHeader.OrderTotal += cart.Price * cart.Count;
        }
        
        if (ShoppingCartVM.OrderHeader.OrderTotal == 0)
        {
            TempData["error_cus"] = "Your shopping cart is empty. Please add some products first";
            return RedirectToAction(nameof(Index));
        }

        return View(ShoppingCartVM);
    }
    
    // POST
    [HttpPost]
    [ValidateAntiForgeryToken]
    [ActionName("Checkout")]
    public IActionResult CheckoutPost()
    {
        
        var claimsIdentity = (ClaimsIdentity)User.Identity!;
        var userId = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier)!.Value;
        
        //      ShoppingCartVM.OrderHeader.ApplicationUser = _unitOfWork.ApplicationUser.Get(u => u.Id == userId);
        // Note: There will be a problem when we populate the ApplicationUser property of the OrderHeader object here.
        // Because later on, we will insert this OrderHeader object into the database, and the ApplicationUser property will also be inserted into the database.
        // However, the ApplicationUser property is already in the database, so we will get an error.
        // To avoid this, we should not populate any navigation properties when we are inserting an object into the database.
        ApplicationUser applicationUser = _unitOfWork.ApplicationUser.Get(u => u.Id == userId); // we need to get the ApplicationUser object from the database
        
        ShoppingCartVM.ListCart = _unitOfWork.ShoppingCart.GetRange(c => c.ApplicationUserId == userId,
            includeProperties: "Product");
        
        var shoppingCarts = ShoppingCartVM.ListCart.ToList();
        foreach (var cart in shoppingCarts)
        {
            cart.Price = CalculatePriceBasedOnQty(cart);
            ShoppingCartVM.OrderHeader.OrderTotal += cart.Price * cart.Count;
        }

        ShoppingCartVM.OrderHeader.ApplicationUserId = userId;
        ShoppingCartVM.OrderHeader.OrderDate = DateTime.Now;
        
        if (applicationUser.CompanyId.GetValueOrDefault() == 0)
        {
            // shopping for personal use, payment is required immediately
            ShoppingCartVM.OrderHeader.PaymentStatus = StaticDetails.PaymentStatusPending;
            ShoppingCartVM.OrderHeader.OrderStatus = StaticDetails.OrderStatusPending;
        }
        else
        {
            // shopping for company, payment is not required immediately
            ShoppingCartVM.OrderHeader.PaymentStatus = StaticDetails.PaymentStatusDelayedPayment;
            ShoppingCartVM.OrderHeader.OrderStatus = StaticDetails.OrderStatusApproved;
        }
        
        _unitOfWork.OrderHeader.Insert(ShoppingCartVM.OrderHeader);
        _unitOfWork.Save();

        foreach (var cart in shoppingCarts)
        {
            OrderDetail orderDetail = new OrderDetail
            {
                ProductId = cart.ProductId,
                OrderHeaderId = ShoppingCartVM.OrderHeader.Id, // this works because we just inserted the OrderHeader with the Save() method
                Price = cart.Price,
                Count = cart.Count
            };
            _unitOfWork.OrderDetail.Insert(orderDetail);
        }
        _unitOfWork.Save();

        if (applicationUser.CompanyId.GetValueOrDefault() == 0)
        {
            // shopping for personal use, we need to track the Stripe session
            var domain = "https://localhost:7115/";

            var options = new SessionCreateOptions
            {
                SuccessUrl = domain + $"Customer/ShoppingCart/OrderConfirmation?id={ShoppingCartVM.OrderHeader.Id}",
                CancelUrl = domain + "Customer/ShoppingCart/Index",
                LineItems = new List<SessionLineItemOptions>(),
                Mode = "payment"
            };
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
            
            _unitOfWork.OrderHeader.UpdateStripePaymentId(ShoppingCartVM.OrderHeader.Id, session.Id, session.PaymentIntentId);
            _unitOfWork.Save();
            Response.Headers.Add("Location", session.Url);
            return new StatusCodeResult(303);
        }
        
        return RedirectToAction(nameof(OrderConfirmation), new {id = ShoppingCartVM.OrderHeader.Id}); 
        // Since we are redirecting to an action method of the same controller, we can use nameof() to avoid hard-coding the action method name
        // Also, we dont need to specify the controller name because we are already in the same controller
    }
    
    // GET
    public IActionResult OrderConfirmation(int id)
    {
        OrderHeader orderHeader = _unitOfWork.OrderHeader.Get(o => o.Id == id, includeProperties: "ApplicationUser");
        if(orderHeader.PaymentStatus != StaticDetails.PaymentStatusDelayedPayment)
        {
            var service = new SessionService();
            Session session = service.Get(orderHeader.SessionId);
            if (session.PaymentStatus.ToLower() == "paid")
            {
                _unitOfWork.OrderHeader.UpdateStripePaymentId(id, session.Id, session.PaymentIntentId);
                _unitOfWork.OrderHeader.UpdateStatus(id, StaticDetails.OrderStatusApproved, StaticDetails.PaymentStatusApproved);
                _unitOfWork.Save();
            }
            HttpContext.Session.SetInt32(StaticDetails.SessionCart, 0);
        }
        
        List<ShoppingCart> shoppingCarts = _unitOfWork.ShoppingCart.GetRange(c => c.ApplicationUserId == orderHeader.ApplicationUserId).ToList();
        _unitOfWork.ShoppingCart.DeleteRange(shoppingCarts);
        _unitOfWork.Save();
        
        return View(id);
    }

    public IActionResult Increase(int cartId)
    {
        var cartFromDb = _unitOfWork.ShoppingCart.Get(c => c.Id == cartId, includeProperties: "Product");
        cartFromDb.Count += 1;
        cartFromDb.Price = CalculatePriceBasedOnQty(cartFromDb);
        _unitOfWork.ShoppingCart.Update(cartFromDb);
        _unitOfWork.Save();
        return RedirectToAction(nameof(Index));
    }

    public IActionResult Decrease(int cartId)
    {
		var cartFromDb = _unitOfWork.ShoppingCart.Get(c => c.Id == cartId, includeProperties: "Product", isTracking: true);
		if (cartFromDb.Count == 1)
        {
			HttpContext.Session.SetInt32(StaticDetails.SessionCart, _unitOfWork.ShoppingCart.GetAll(s => s.ApplicationUserId == cartFromDb.ApplicationUserId).Count() - 1);
			_unitOfWork.ShoppingCart.Delete(cartFromDb);
        }
        else
        {
            cartFromDb.Count -= 1;
            cartFromDb.Price = CalculatePriceBasedOnQty(cartFromDb);
            _unitOfWork.ShoppingCart.Update(cartFromDb);
        }
        _unitOfWork.Save();
        return RedirectToAction(nameof(Index));
    }

    public IActionResult Remove(int cartId)
    {
        var cartFromDb = _unitOfWork.ShoppingCart.Get(c => c.Id == cartId, includeProperties: "Product", isTracking: true);
        _unitOfWork.ShoppingCart.Delete(cartFromDb);
        HttpContext.Session.SetInt32(StaticDetails.SessionCart, _unitOfWork.ShoppingCart.GetAll(s => s.ApplicationUserId == cartFromDb.ApplicationUserId).Count() - 1);
        _unitOfWork.Save();
        return RedirectToAction(nameof(Index));
    }

    private double CalculatePriceBasedOnQty(ShoppingCart cart)
    {
        if (cart.Count <= 50)
        {
            return cart.Product.Price;
        }
        if (cart.Count <= 100)
        {
            return cart.Product.Price50;
        }
        return cart.Product.Price100;
    }
}
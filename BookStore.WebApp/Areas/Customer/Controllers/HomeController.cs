using System.Diagnostics;
using System.Security.Claims;
using BookStore.DataAccess.Repositories.IRepository;
using BookStore.Models;
using BookStore.Utilities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BookStore.WebApp.Areas.Customer.Controllers
{
    [Area("Customer")]
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        // The HomeController class is a controller class. Declare a private readonly ILogger<HomeController> field to enable logging in the controller.
        private readonly IUnitOfWork _unitOfWork;

        public HomeController(ILogger<HomeController> logger, IUnitOfWork unitOfWork)
        {
            _logger = logger;
            _unitOfWork = unitOfWork;
        }
        // The HomeController class is a controller class. Add a constructor that accepts an ILogger<HomeController> parameter.

        public IActionResult Index()
        {
			var claimsIdentity = (ClaimsIdentity)User.Identity!;
			var claim = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier);
            if (claim != null)
            {
				int count = _unitOfWork.ShoppingCart.GetAll(s => s.ApplicationUserId == claim.Value).Count();
				HttpContext.Session.SetInt32(StaticDetails.SessionCart, count);
			}
			IEnumerable<Product> products = _unitOfWork.Product.GetAll(includeProperties: "Category");
            return View(products); // View() method returns an IActionResult.
        }
        // Add an Index action method that returns an IActionResult.

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        /// GET
        public IActionResult Details(int productId)
        {
            ShoppingCart shoppingCart = new()
            {
                Product = _unitOfWork.Product.Get(p => p.Id == productId, includeProperties: "Category"),
                ProductId = productId,
                Count = 1
            };
			return View(shoppingCart);
		}
        
        /// POST
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize] // This attribute makes sure that only authenticated users can access this action method.
        public IActionResult Details(ShoppingCart shoppingCart)
        {
            var claimsIdentity = (ClaimsIdentity)User.Identity!; // This line of code gets the claims identity of the logged-in user
                                                                 // Which includes the Id of the logged-in user that we need to pass to the AddToCart method.
                                                                 // The '!' operator is used to suppress the warning that the User.Identity property may be null.
                                                                 // Typically, we tell the compiler that the User.Identity property will not null.
            var userId = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier)!.Value; // This line of code gets the Id of the logged-in user.
            shoppingCart.ApplicationUserId = userId;
            ShoppingCart shoppingCartFromDb = _unitOfWork.ShoppingCart.Get(s => s.ApplicationUserId == shoppingCart.ApplicationUserId && s.ProductId == shoppingCart.ProductId);
            if (shoppingCartFromDb != null)
            {
                shoppingCartFromDb.Count += shoppingCart.Count;
                _unitOfWork.ShoppingCart.Update(shoppingCartFromDb);
                // It is might not necessary to call the Update here.
                // Because the shoppingCartFromDb object is already being tracked by the Entity Framework Core after we retrieved it from the database.
                // So, when we call the Save method, the Entity Framework Core will automatically update the shoppingCartFromDb object in the database if it has been changed.
                // And because of this feature, we need to be careful when retrieving an object from the database since it will be tracked
                // by the Entity Framework Core and got updated automatically.
                // For that reason, the Repository class is modified to include a new parameter called isTracking.
                // And by default, the isTracking parameter is set to false. Which means that the object retrieved from the database will not be tracked by the Entity Framework Core.
                _unitOfWork.Save();
            }
            else
            {
                _unitOfWork.ShoppingCart.Insert(shoppingCart); // add the shopping cart to the database
                _unitOfWork.Save();
                int count = _unitOfWork.ShoppingCart.GetAll(s => s.ApplicationUserId == userId).Count();
                HttpContext.Session.SetInt32(StaticDetails.SessionCart, count);
                // the line updates the session with the number of items in the shopping cart
            }
            TempData["Success"] = "Product added to cart successfully";
            return RedirectToAction(nameof(Index));
        }
    }
}

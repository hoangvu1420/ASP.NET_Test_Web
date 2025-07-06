using System.Security.Claims;
using BookStore.DataAccess.Repositories.IRepository;
using Microsoft.AspNetCore.Mvc;
using TestApp.Utilities;

namespace BookStore.WebApp.ViewComponents;

/*
 View components in ASP.NET Core are similar to partial views, but they're much more powerful. They're intended to be used in a variety of scenarios, including:
- Rendering a menu
- Showing a list of records
- Displaying a login panel
- Displaying a shopping cart
- Rendering a tag cloud
- Displaying a breadcrumb trail
- Displaying a list of categories
- Displaying a list of recent comments
- Displaying a list of recent posts
...
    This file is the code behind for the ShoppingCartViewComponent view component.
    This class is derived from the ViewComponent class, which is a base class for view components in ASP.NET Core.
    The ShoppingCartViewComponent class has an InvokeAsync() method that returns a Task<IViewComponentResult>.
    The InvokeAsync() method is called when the view component is invoked.
    The InvokeAsync() method gets the number of items in the shopping cart from the database and stores it in the session.
    The InvokeAsync() method then returns the ShoppingCartViewComponent view component with the number of items in the shopping cart.
 */

public class ShoppingCartViewComponent : ViewComponent
{
    private readonly IUnitOfWork _unitOfWork;

    public ShoppingCartViewComponent(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<IViewComponentResult> InvokeAsync()
    {
        var claimsIdentity = (ClaimsIdentity)User.Identity!;
        var claim = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier);
        if (claim != null)
        {
            int count = _unitOfWork.ShoppingCart.GetAll(s => s.ApplicationUserId == claim.Value).Count();
            if (HttpContext.Session.GetInt32(StaticDetails.SessionCart) == null)
            {
                HttpContext.Session.SetInt32(StaticDetails.SessionCart, count);
            }

            return View(count);
        }

        HttpContext.Session.SetInt32(StaticDetails.SessionCart, 0);
        return View(0);
    }
}
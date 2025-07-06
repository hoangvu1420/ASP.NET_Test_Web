using BookStore.DataAccess.Repositories.IRepository;
using BookStore.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TestApp.Utilities;

namespace BookStore.WebApp.Areas.Admin.Controllers
{
    [Area("Admin")] // this attribute specifies that the CategoryController class is in the Admin area.
    [Authorize(Roles = StaticDetails.RoleAdmin)] // this attribute specifies that only users in the Admin role can access the CategoryController class
                                                  // We can add this attribute individually to each action method, but it is better to add it to the controller class.
    public class CategoryController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;

        public CategoryController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }
        // In orser to use the CategoryRepository class, we need to register it in the Program.cs file

        public IActionResult Index()
        {
            IEnumerable<Category> objCategoryList = _unitOfWork.Category.GetAll();
            return View(objCategoryList);
        }

        // GET
        public IActionResult Create()
        {
            return View();
        }

        // POST
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(Category obj)
        {
            if (obj.Name == obj.DisplayOrder.ToString())
            {
                ModelState.AddModelError(string.Empty, "Display Order should not be the same with Name");
                // The ModelState.AddModelError() method adds a model error to the ModelState object. The ModelState object contains the state of the model and model-binding validation.
                // If we want the error message to be displayed in the span tag of Name, we can change the key of AddModelError() to "Name" as the following code:
                // ModelState.AddModelError("Name", "Display Order should not be the same with Name");
                TempData["error"] = "Display Order should not be the same with Name";
            }
            if (obj.Name == "test")
            {
                ModelState.AddModelError(string.Empty, "Category Name should not be \"test\"");
                // Note that the first parameter of the AddModelError() method is an empty string. This means that the error message is not associated with any property.
                // Hence, the error is considered a model error, not a property error.
                // The error message will be displayed in the validation summary if we set asp-validation-summary="ModelOnly" in the form tag.
                TempData["error"] += "Category Name should not be \"test\"";
            }
            if (_unitOfWork.Category.GetAll().Any(c => c.Name == obj.Name))
            {
                ModelState.AddModelError(string.Empty, "Category Name already exists");
                TempData["error"] += "Category Name already exists";
            }
            if (_unitOfWork.Category.GetAll().Any(c => c.DisplayOrder == obj.DisplayOrder))
            {
                ModelState.AddModelError(string.Empty, "Display Order already exists");
                TempData["error"] += "Display Order already exists";
            }
            if (ModelState.IsValid)
            {
                // The ModelState.IsValid property checks whether the submitted form values can be used to create a new Category object (i.e. whether the submitted form values are valid).
                // this property checks values based on the validation rules that we specified in the Category model class (for example, [Required] attribute for the Name property).
                // we can create a custom validation rule by creating a custom validation attribute. This is done by creating a class that inherits from the ValidationAttribute class.
                _unitOfWork.Category.Insert(obj);
                _unitOfWork.Save();
                TempData["success"] = "Category has been created successfully";
                return RedirectToAction("Index"); // this line of code redirects the user to the Index action method of the same controller.
                                                  // if we want to redirect the user to the Index action method of a different controller, we can use the following code:
                                                  // return RedirectToAction("Index", "Home");
            }

            return View(obj); // If the submitted form values are not valid, the Create action method returns the same view (i.e. the Create view) to the user, along with the submitted form values.
        }

        // Create GET and POST action methods for Edit and Delete
        // GET
        public IActionResult Edit(int? id)
        {
            if (id is null or 0)
            {
                return NotFound();
            }

            var obj = _unitOfWork.Category.Get(i => i.Id == id);

            return View(obj);
        }

        // POST
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(Category obj)
        {
            if (obj.Name == obj.DisplayOrder.ToString())
            {
                ModelState.AddModelError(string.Empty, "Display Order should not be the same with Name");
                TempData["error"] = "Display Order should not be the same with Name";
            }
            if (obj.Name == "test")
            {
                ModelState.AddModelError(string.Empty, "Category Name should not be \"test\"");
                TempData["error"] += "Category Name should not be \"test\"";
            }
            if (ModelState.IsValid)
            {
                _unitOfWork.Category.Update(obj);
                _unitOfWork.Save();
                TempData["success"] = "Category has been updated successfully";
                return RedirectToAction(nameof(Index));
            }

            return View(obj);
        }

        // GET
        public IActionResult Delete(int? id)
        {
            if (id == null || id == 0)
            {
                return NotFound();
            }

            var obj = _unitOfWork.Category.Get(i => i.Id == id);
            if (obj == null)
            {
                return NotFound();
            }

            return View(obj);
        }

        // POST
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public IActionResult DeletePost(int? id)
        {
            var obj = _unitOfWork.Category.Get(i => i.Id == id);
            if (obj == null)
            {
                return NotFound();
            }

            _unitOfWork.Category.Delete(obj);
            _unitOfWork.Save();
            TempData["success"] = "Category has been deleted successfully";
            return RedirectToAction("Index");   // RedirectToAction() returns a 302 HTTP status code to the browser, which causes the browser to make a GET request to the specified action method.
											    // On the other hand, the View() method returns a 200 response to the browser, which causes the browser to make a GET request to the same action method.
        }
    }
}

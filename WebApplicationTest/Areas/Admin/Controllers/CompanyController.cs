using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using TestApp.DataAccess.Repositories.IRepository;
using TestApp.Models;
using TestApp.Models.ViewModels;
using TestApp.Utilities;

namespace WebApplicationTest.Areas.Admin.Controllers
{
	[Area("Admin")]
	[Authorize(Roles = StaticDetails.RoleAdmin)] // this attribute specifies that only users in the Admin role can access the CategoryController class.
	public class CompanyController : Controller
	{
		private readonly IUnitOfWork _unitOfWork;
		private readonly IWebHostEnvironment _hostEnvironment; // The IWebHostEnvironment interface is used to get the root path of the web application.
		public CompanyController(IUnitOfWork unitOfWork, IWebHostEnvironment hostEnvironment)
		{
			_unitOfWork = unitOfWork;
			_hostEnvironment = hostEnvironment; // We need to inject the IWebHostEnvironment interface into the CompanyController class.
		}

		public IActionResult Index()
		{
			IEnumerable<Company> objCompanyList = _unitOfWork.Company.GetAll();
			return View(objCompanyList);
		}

		// Combine the Create and Edit actions into one action called Upsert.
		// The Upsert action will be used for both creating and updating a product.
		// GET
		public IActionResult Upsert(int? id)
		{
			if (id == null || id == 0)
			{
				// this is for create
				return View(new Company());
			}
			// this is for edit
			Company companyObj = _unitOfWork.Company.Get(i => i.Id == id);
			if (companyObj == null)
			{
				return NotFound();
			}
			return View(companyObj);
		}
		// POST
		[HttpPost]
		[ValidateAntiForgeryToken]
		public IActionResult Upsert(Company companyObj)
		{
			if (ModelState.IsValid)
			{
				if (companyObj.Id == 0)
				{
					_unitOfWork.Company.Insert(companyObj);
					TempData["success"] = "Company created successfully";
				}
				else
				{
					_unitOfWork.Company.Update(companyObj);
					TempData["success"] = "Company updated successfully";
				}
				_unitOfWork.Save();
				return RedirectToAction(nameof(Index));
			}
			return View(companyObj);
		}

		//// GET
		//public IActionResult Create()
		//{
		//	IEnumerable<SelectListItem> objCategoryList = _unitOfWork.Category.GetAll().Select(i => new SelectListItem
		//	{
		//		Text = i.Name,
		//		Value = i.Id.ToString()
		//	});
		//	// The SelectList class is used to create a list of items that are used to populate the drop-down list.
		//	// The SelectList class has two properties: Text and Value. The Text property is the text of the item,
		//	// and the Value property is the value that is submitted to the server when the item is selected.
		//	ProductVM productVM = new ProductVM()
		//	{
		//		Product = new Product(),
		//		CategoryList = objCategoryList
		//	};
		//	return View(productVM);		// The Create view will be strongly typed to the ProductVM class.
		//								// Therefore, now we can access the Product and CategoryList properties of the ProductVM class in the Create view.
		//								// No need lengthy code in the Create view anymore. This also enhances the security of the application.
		//}
		//// POST
		//[HttpPost]
		//[ValidateAntiForgeryToken]
		//public IActionResult Create(ProductVM productVM)
		//{
		//	if (ModelState.IsValid)
		//	{
		//		_unitOfWork.Product.Insert(productVM.Product);
		//		_unitOfWork.Save();
		//		TempData["success"] = "Product created successfully";
		//		return RedirectToAction(nameof(Index));
		//	}
		//	else
		//	{
		//		productVM.CategoryList = _unitOfWork.Category.GetAll().Select(i => new SelectListItem
		//		{
		//			Text = i.Name,
		//			Value = i.Id.ToString()
		//		});
		//		return View(productVM); // We need populate the CategoryList property of the ProductVM class again incase the form is not valid.
		//								// Otherwise, the drop-down list will be empty when the Create view is rendered.
		//	}
		//}

		//// GET
		//public IActionResult Edit(int? id)
		//{
		//	if (id == null || id == 0)
		//	{
		//		return NotFound();
		//	}

		//	var obj = _unitOfWork.Product.Get(i => i.Id == id);
		//	if (obj == null)
		//	{
		//		return NotFound();
		//	}

		//	return View(obj);
		//}
		//// POST
		//[HttpPost]
		//[ValidateAntiForgeryToken]
		//public IActionResult Edit(Product obj)
		//{
		//	if (ModelState.IsValid)
		//	{
		//		_unitOfWork.Product.Update(obj);
		//		_unitOfWork.Save();
		//		TempData["success"] = "Product updated successfully";
		//		return RedirectToAction(nameof(Index));
		//	}
		//	return View(obj);
		//}

		// GET
		//public IActionResult Delete(int? id)
		//{
		//	if (id == null || id == 0)
		//	{
		//		return NotFound();
		//	}

		//	var obj = _unitOfWork.Product.Get(i => i.Id == id);
		//	if (obj == null)
		//	{
		//		return NotFound();
		//	}

		//	return View(obj);
		//}
		//// POST
		//[HttpPost]
		//[ValidateAntiForgeryToken]
		//public IActionResult DeletePost(int? id)
		//{
		//	if (id == null || id == 0)
		//	{
		//		return NotFound();
		//	}

		//	var obj = _unitOfWork.Product.Get(i => i.Id == id);
		//	if (obj == null)
		//	{
		//		return NotFound();
		//	}
		//	_unitOfWork.Product.Delete(obj);
		//	_unitOfWork.Save();
		//	TempData["success"] = "Product deleted successfully";
		//	return RedirectToAction(nameof(Index));
		//}

		#region API CALLS

		[HttpGet]
		public IActionResult GetAll()
		{
			var allObj = _unitOfWork.Company.GetAll();
			return Json(new { data = allObj });
		}

		[HttpDelete]
		public IActionResult Delete(int? id)
		{
			var objFromDb = _unitOfWork.Company.Get(i => i.Id == id);
			if (objFromDb == null)
			{
				return Json(new { success = false, message = "Error while deleting" });
			}

			_unitOfWork.Company.Delete(objFromDb);
			_unitOfWork.Save();
			TempData["success"] = "Company deleted successfully";
			return Json(new { success = true, message = "Company deleted successfully" });
		}

		#endregion

	}
}

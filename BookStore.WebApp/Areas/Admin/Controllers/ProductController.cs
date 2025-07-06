using BookStore.DataAccess.Repositories.IRepository;
using BookStore.Models;
using BookStore.Models.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using TestApp.Utilities;

namespace BookStore.WebApp.Areas.Admin.Controllers
{
	[Area("Admin")]
	[Authorize(Roles = StaticDetails.RoleAdmin)] // this attribute specifies that only users in the Admin role can access the CategoryController class.
	public class ProductController : Controller
	{
		private readonly IUnitOfWork _unitOfWork;
		private readonly IWebHostEnvironment _hostEnvironment; // The IWebHostEnvironment interface is used to get the root path of the web application.
		public ProductController(IUnitOfWork unitOfWork, IWebHostEnvironment hostEnvironment)
		{
			_unitOfWork = unitOfWork;
			_hostEnvironment = hostEnvironment; // We need to inject the IWebHostEnvironment interface into the ProductController class.
		}

		public IActionResult Index()
		{
			IEnumerable<Product> objProductList = _unitOfWork.Product.GetAll(includeProperties: "Category");
			return View(objProductList);
		}

		// Combine the Create and Edit actions into one action called Upsert.
		// The Upsert action will be used for both creating and updating a Company.
		// GET
		public IActionResult Upsert(int? id)
		{
			ProductVM productVM = new()
			{
				CategoryList = _unitOfWork.Category.GetAll().Select(u => new SelectListItem
				{
					Text = u.Name,
					Value = u.Id.ToString()
				}),
				Product = new Product()
			};
			if (id == null || id == 0)
			{
				// this is for create
				// set the image url to the default image
				productVM.Product.ImageUrl = @"\images\product\Default.png";
				return View(productVM);
			}
			// this is for edit
			productVM.Product = _unitOfWork.Product.Get(u => u.Id == id);
			return View(productVM);
		}
		// POST
		[HttpPost]
		[ValidateAntiForgeryToken]
		public IActionResult Upsert(ProductVM productVM, IFormFile? file)
		{
			if (ModelState.IsValid)
			{
				string webRootPath = _hostEnvironment.WebRootPath; // The WebRootPath property of the IWebHostEnvironment interface returns the absolute path to the web root directory.
				if (file != null)
				{
					// if user uploads an image
					string upload = Path.Combine(webRootPath, @"images\product"); // this give us the path to the images/product folder
					string fileName = Guid.NewGuid().ToString(); // The Guid class is used to generate a unique string value.
					string extension = Path.GetExtension(file.FileName);

					if (!string.IsNullOrEmpty(productVM.Product.ImageUrl))
					{
						// this is an edit and we need to remove old image
						var imagePath = Path.Combine(webRootPath, productVM.Product.ImageUrl.TrimStart('\\')); // Use TrimStart('\\') to remove the leading backslash (\) from the ImageUrl property.
						if (System.IO.File.Exists(imagePath))
						{
							System.IO.File.Delete(imagePath);
						}
					}

					using (var fileStream = new FileStream(Path.Combine(upload, fileName + extension), FileMode.Create))
					{
						file.CopyTo(fileStream);
					}
					productVM.Product.ImageUrl = @"\images\product\" + fileName + extension;
				}
				if (productVM.Product.Id == 0)
				{
					_unitOfWork.Product.Insert(productVM.Product);
					TempData["success"] = "Product created successfully";
				}
				else
				{
					_unitOfWork.Product.Update(productVM.Product);
					TempData["success"] = "Product updated successfully"; }
				_unitOfWork.Save();
				return RedirectToAction(nameof(Index));
			}
			return View(productVM);
		}

		// Old code
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
		//	CompanyVM CompanyVM = new CompanyVM()
		//	{
		//		Company = new Company(),
		//		CategoryList = objCategoryList
		//	};
		//	return View(CompanyVM);		// The Create view will be strongly typed to the CompanyVM class.
		//								// Therefore, now we can access the Company and CategoryList properties of the CompanyVM class in the Create view.
		//								// No need lengthy code in the Create view anymore. This also enhances the security of the application.
		//}
		//// POST
		//[HttpPost]
		//[ValidateAntiForgeryToken]
		//public IActionResult Create(CompanyVM CompanyVM)
		//{
		//	if (ModelState.IsValid)
		//	{
		//		_unitOfWork.Company.Insert(CompanyVM.Company);
		//		_unitOfWork.Save();
		//		TempData["success"] = "Company created successfully";
		//		return RedirectToAction(nameof(Index));
		//	}
		//	else
		//	{
		//		CompanyVM.CategoryList = _unitOfWork.Category.GetAll().Select(i => new SelectListItem
		//		{
		//			Text = i.Name,
		//			Value = i.Id.ToString()
		//		});
		//		return View(CompanyVM); // We need populate the CategoryList property of the CompanyVM class again incase the form is not valid.
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

		//	var obj = _unitOfWork.Company.Get(i => i.Id == id);
		//	if (obj == null)
		//	{
		//		return NotFound();
		//	}

		//	return View(obj);
		//}
		//// POST
		//[HttpPost]
		//[ValidateAntiForgeryToken]
		//public IActionResult Edit(Company obj)
		//{
		//	if (ModelState.IsValid)
		//	{
		//		_unitOfWork.Company.Update(obj);
		//		_unitOfWork.Save();
		//		TempData["success"] = "Company updated successfully";
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

		//	var obj = _unitOfWork.Company.Get(i => i.Id == id);
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

		//	var obj = _unitOfWork.Company.Get(i => i.Id == id);
		//	if (obj == null)
		//	{
		//		return NotFound();
		//	}
		//	_unitOfWork.Company.Delete(obj);
		//	_unitOfWork.Save();
		//	TempData["success"] = "Company deleted successfully";
		//	return RedirectToAction(nameof(Index));
		//}

		#region API CALLS

		[HttpGet]
		public IActionResult GetAll()
		{
			var allObj = _unitOfWork.Product.GetAll(includeProperties: "Category");
			return Json(new { data = allObj });
		}

		[HttpDelete]
		public IActionResult Delete(int? id)
		{
			var objFromDb = _unitOfWork.Product.Get(i => i.Id == id);
			if (objFromDb == null)
			{
				return Json(new { success = false, message = "Error while deleting" });
			}
			var imagePath = Path.Combine(_hostEnvironment.WebRootPath, objFromDb.ImageUrl.TrimStart('\\'));
			// check if the image is Default.png, if it is, we don't want to delete it
			if (System.IO.File.Exists(imagePath) && !imagePath.Contains("Default.png"))
			{
				System.IO.File.Delete(imagePath);
			}

			_unitOfWork.Product.Delete(objFromDb);
			_unitOfWork.Save();
			TempData["success"] = "Product deleted successfully";
			return Json(new { success = true, message = "Product deleted successfully" });
		}

		#endregion

	}
}

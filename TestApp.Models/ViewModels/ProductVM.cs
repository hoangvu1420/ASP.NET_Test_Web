using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TestApp.Models.ViewModels
{
	// The purpose of ViewModel in general is to combine multiple models into one model then pass it to the view.
	// With this approach, only one model is passed to the view and we can avoid the problem of passing multiple models to the view.
	
	// The ProductVM class is used to combine the Product and the CategoryList models into one model so in the view we can access both models.
	public class ProductVM
	{
		public Product Product { get; set; }

		[ValidateNever]
		public IEnumerable<SelectListItem> CategoryList { get; set; }
	}
}

using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace BookStore.Models
{
	public class Product
	{
		[Key]
		public int Id { get; set; }

		[Required]
		public string Title { get; set; }

		public string Description { get; set; }
		
		[Required]
		public string ISBN { get; set; } // International Standard Book Number

		[Required]
		public string Author { get; set; }

		[Required]
		[DisplayName("List Price")]
		[Range(1, 1000)]
		public double ListPrice { get; set; } // The price at which the publisher recommends that the book be sold

		[Required]
		[DisplayName("Price for 1-50")]
		[Range(1, 1000)]
		public double Price { get; set; } // The price at which the book is actually sold

		[Required]
		[DisplayName("Price for 51-100")]
		[Range(1, 1000)]
		public double Price50 { get; set; } // The price applied to the book when the customer buys 51-100 copies

		[Required]
		[DisplayName("Price for 101+")]
		[Range(1, 1000)]
		public double Price100 { get; set; } // The price applied to the book when the customer buys 101+ copies

		// Foreign Key to Category
		public int CategoryId { get; set; }
		[ForeignKey("CategoryId")]
		[ValidateNever]
		public Category Category { get; set; }

		[DisplayName("Image URL")]
		[ValidateNever]
		public string ImageUrl { get; set; }
	}
}

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace TestApp.Models;

public class ShoppingCart
{
    [Key]
    public int Id { get; set; }
    
    public int ProductId { get; set; }
    [ForeignKey("ProductId")]
    [ValidateNever]
    public Product Product { get; set; } // Navigation property to the Product class
    
    public int Count { get; set; }
    
    public string ApplicationUserId { get; set; }
    [ForeignKey("ApplicationUserId")]
    [ValidateNever]
    public ApplicationUser ApplicationUser { get; set; } // Navigation property to the ApplicationUser class
    
    [NotMapped]
    public double Price { get; set; } // This is the price of the product at the time of adding to the cart
}
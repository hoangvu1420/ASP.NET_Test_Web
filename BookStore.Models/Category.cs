using System.ComponentModel.DataAnnotations;

namespace BookStore.Models
{
	public class Category
    {
        [Key] // Add the [Key] attribute to the Id property. This attribute identifies the Id property as the primary key in the database.
        public int Id { get; set; }

        [Required] // Add the [Required] attribute to the Name property. This attribute specifies that the Name property is required.
        [Display(Name = "Category Name")] // Add the [Display] attribute to the Name property. This attribute specifies the display name of the Name property.
        [MaxLength(30)]
        [MinLength(3, ErrorMessage = "The minimum length of Category Name is 3")]
        public string Name { get; set; }

        [Display(Name = "Display Order")] // Add the [Display] attribute to the DisplayOrder property. This attribute specifies the display name of the DisplayOrder property.
        [Range(1, int.MaxValue, ErrorMessage = "Display Order for category must be greater than 0")] // Add the [Range] attribute to the DisplayOrder property. This attribute specifies the range of the DisplayOrder property.
        public int DisplayOrder { get; set; }

        [Display(Name = "Created Date Time")]
        public DateTime CreatedDateTime { get; set; } = DateTime.Now;
    }
}

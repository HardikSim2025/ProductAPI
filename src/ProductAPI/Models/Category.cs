using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ProductAPI.Models
{
    public class Category
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "Category name is required")]
        [StringLength(200, MinimumLength = 1, ErrorMessage = "Category name must be between 1 and 200 characters")]
        public string Name { get; set; } = string.Empty;

        [StringLength(1000, ErrorMessage = "Description cannot exceed 1000 characters")]
        public string? Description { get; set; }

        [ForeignKey("ParentCategory")]
        public int? ParentCategoryId { get; set; }

        // Navigation property for self-referencing
        [InverseProperty("ChildCategories")]
        public virtual Category? ParentCategory { get; set; }

        [InverseProperty("ParentCategory")]
        public virtual ICollection<Category> ChildCategories { get; set; } = new List<Category>();
    }
}

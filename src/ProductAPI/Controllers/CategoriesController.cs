using Microsoft.AspNetCore.Mvc;
using ProductAPI.Models;
using ProductAPI.Services;
using System.ComponentModel.DataAnnotations;

namespace ProductAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CategoriesController : ControllerBase
    {
        private readonly ICategoryService _categoryService;
        private readonly ILogger<CategoriesController> _logger;

        public CategoriesController(ICategoryService categoryService, ILogger<CategoriesController> logger)
        {
            _categoryService = categoryService;
            _logger = logger;
        }

        /// <summary>
        /// Create a new category
        /// </summary>
        /// <param name="category">Category data with Name and optional Description and ParentCategoryId</param>
        /// <returns>Created category with id</returns>
        [HttpPost]
        [ProducesResponseType(typeof(Category), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> CreateCategory([FromBody] Category category)
        {
            try
            {
                // Validate model
                var context = new ValidationContext(category);
                var results = new List<ValidationResult>();
                if (!Validator.TryValidateObject(category, context, results, true))
                {
                    var errors = results.ToDictionary(
                        r => r.MemberNames.FirstOrDefault() ?? "General",
                        r => new[] { r.ErrorMessage ?? "Validation error" }
                    );
                    return BadRequest(new { errors });
                }

                // Validate parentCategoryId if provided
                var validationErrors = new Dictionary<string, string[]>();

                if (category.ParentCategoryId.HasValue)
                {
                    // Check if parent category exists
                    var parentExists = await _categoryService.CategoryExistsAsync(category.ParentCategoryId.Value);
                    if (!parentExists)
                    {
                        validationErrors["ParentCategoryId"] = new[] { "Parent category does not exist" };
                    }

                    // Check for self-reference
                    if (category.ParentCategoryId.Value == category.Id)
                    {
                        validationErrors["ParentCategoryId"] = new[] { "A category cannot be its own parent" };
                    }
                }

                if (validationErrors.Count > 0)
                {
                    return BadRequest(new { errors = validationErrors });
                }

                var createdCategory = await _categoryService.CreateCategoryAsync(category);
                return CreatedAtAction(nameof(GetCategoryById), new { id = createdCategory.Id }, createdCategory);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating category");
                return BadRequest(new { error = "An error occurred while creating the category" });
            }
        }

        /// <summary>
        /// Get all categories
        /// </summary>
        /// <returns>List of all categories with hierarchy information</returns>
        [HttpGet]
        [ProducesResponseType(typeof(List<Category>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetCategories()
        {
            try
            {
                var categories = await _categoryService.GetAllCategoriesAsync();
                return Ok(categories);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving categories");
                return BadRequest(new { error = "An error occurred while retrieving categories" });
            }
        }

        /// <summary>
        /// Get a specific category by id
        /// </summary>
        /// <param name="id">Category id</param>
        /// <returns>Category with full details</returns>
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(Category), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetCategoryById(int id)
        {
            try
            {
                var category = await _categoryService.GetCategoryByIdAsync(id);
                if (category == null)
                {
                    return NotFound(new { error = $"Category with id {id} not found" });
                }

                return Ok(category);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving category");
                return BadRequest(new { error = "An error occurred while retrieving the category" });
            }
        }

        /// <summary>
        /// Update a category
        /// </summary>
        /// <param name="id">Category id</param>
        /// <param name="category">Updated category properties</param>
        /// <returns>Updated category</returns>
        [HttpPut("{id}")]
        [ProducesResponseType(typeof(Category), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> UpdateCategory(int id, [FromBody] Category category)
        {
            try
            {
                // Validate model
                var context = new ValidationContext(category);
                var results = new List<ValidationResult>();
                if (!Validator.TryValidateObject(category, context, results, true))
                {
                    var errors = results.ToDictionary(
                        r => r.MemberNames.FirstOrDefault() ?? "General",
                        r => new[] { r.ErrorMessage ?? "Validation error" }
                    );
                    return BadRequest(new { errors });
                }

                // Check if category exists
                var existingCategory = await _categoryService.GetCategoryByIdAsync(id);
                if (existingCategory == null)
                {
                    return NotFound(new { error = $"Category with id {id} not found" });
                }

                // Validate parentCategoryId if provided
                var validationErrors = new Dictionary<string, string[]>();

                if (category.ParentCategoryId.HasValue)
                {
                    // Check for self-reference
                    if (category.ParentCategoryId.Value == id)
                    {
                        validationErrors["ParentCategoryId"] = new[] { "A category cannot be its own parent" };
                        return BadRequest(new { errors = validationErrors });
                    }

                    // Check if parent category exists
                    var parentExists = await _categoryService.CategoryExistsAsync(category.ParentCategoryId.Value);
                    if (!parentExists)
                    {
                        validationErrors["ParentCategoryId"] = new[] { "Parent category does not exist" };
                        return BadRequest(new { errors = validationErrors });
                    }

                    // Check for circular references
                    var wouldCreateCircular = await _categoryService.WouldCreateCircularReferenceAsync(id, category.ParentCategoryId);
                    if (wouldCreateCircular)
                    {
                        validationErrors["ParentCategoryId"] = new[] { "This would create a circular reference in the category hierarchy" };
                        return BadRequest(new { errors = validationErrors });
                    }
                }

                var updatedCategory = await _categoryService.UpdateCategoryAsync(id, category);
                return Ok(updatedCategory);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating category");
                return BadRequest(new { error = "An error occurred while updating the category" });
            }
        }

        /// <summary>
        /// Delete a category
        /// </summary>
        /// <param name="id">Category id</param>
        /// <returns>Success or conflict if category has children</returns>
        [HttpDelete("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        public async Task<IActionResult> DeleteCategory(int id)
        {
            try
            {
                // Check if category exists
                var existingCategory = await _categoryService.GetCategoryByIdAsync(id);
                if (existingCategory == null)
                {
                    return NotFound(new { error = $"Category with id {id} not found" });
                }

                // Check if category has children
                var hasChildren = await _categoryService.HasChildrenAsync(id);
                if (hasChildren)
                {
                    return Conflict(new { error = "Cannot delete a category that has child categories" });
                }

                var deleted = await _categoryService.DeleteCategoryAsync(id);
                if (deleted)
                {
                    return Ok(new { message = $"Category with id {id} deleted successfully" });
                }

                return NotFound(new { error = $"Category with id {id} not found" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting category");
                return BadRequest(new { error = "An error occurred while deleting the category" });
            }
        }
    }
}

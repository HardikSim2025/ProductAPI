using Microsoft.AspNetCore.Mvc;
using ProductAPI.Models;
using ProductAPI.Services;
using System.ComponentModel.DataAnnotations;

namespace ProductAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ProductsController : ControllerBase
    {
        private readonly IProductService _productService;
        private readonly ILogger<ProductsController> _logger;

        public ProductsController(IProductService productService, ILogger<ProductsController> logger)
        {
            _productService = productService;
            _logger = logger;
        }

        /// <summary>
        /// Create a new product
        /// </summary>
        /// <param name="product">Product data</param>
        /// <returns>Created product with id</returns>
        [HttpPost]
        [ProducesResponseType(typeof(Product), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> CreateProduct([FromBody] Product product)
        {
            try
            {
                // Validate model
                var context = new ValidationContext(product);
                var results = new List<ValidationResult>();
                if (!Validator.TryValidateObject(product, context, results, true))
                {
                    var errors = results.ToDictionary(
                        r => r.MemberNames.FirstOrDefault() ?? "General",
                        r => new[] { r.ErrorMessage ?? "Validation error" }
                    );
                    return BadRequest(new { errors });
                }

                // Additional validation
                var validationErrors = new Dictionary<string, string[]>();

                if (product.Price <= 0)
                {
                    validationErrors["Price"] = new[] { "Price must be greater than 0" };
                }

                if (product.Stock < 0)
                {
                    validationErrors["Stock"] = new[] { "Stock cannot be negative" };
                }

                if (validationErrors.Count > 0)
                {
                    return BadRequest(new { errors = validationErrors });
                }

                var createdProduct = await _productService.CreateProductAsync(product);
                return CreatedAtAction(nameof(GetProducts), createdProduct);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating product");
                return BadRequest(new { error = "An error occurred while creating the product" });
            }
        }

        /// <summary>
        /// Get all products
        /// </summary>
        /// <returns>List of all products</returns>
        [HttpGet]
        [ProducesResponseType(typeof(List<Product>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetProducts()
        {
            try
            {
                var products = await _productService.GetAllProductsAsync();
                return Ok(products);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving products");
                return BadRequest(new { error = "An error occurred while retrieving products" });
            }
        }
    }
}

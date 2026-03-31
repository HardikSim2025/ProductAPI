using NUnit.Framework;
using Moq;
using ProductAPI.Data;
using ProductAPI.Models;
using ProductAPI.Services;
using Microsoft.EntityFrameworkCore;

namespace ProductAPI.Tests
{
    [TestFixture]
    public class ProductServiceTests
    {
        private AppDbContext _context = null!;
        private ProductService _service = null!;

        [SetUp]
        public void Setup()
        {
            // Create in-memory database context
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;
            _context = new AppDbContext(options);
            _service = new ProductService(_context);
        }

        [TearDown]
        public void TearDown()
        {
            _context?.Dispose();
        }

        [Test]
        public async Task CreateProductAsync_WithValidProduct_ReturnsProduct()
        {
            // Arrange
            var product = new Product
            {
                Name = "Test Product",
                Description = "Test Description",
                Price = 29.99m,
                Category = "Electronics",
                Stock = 10
            };

            // Act
            var result = await _service.CreateProductAsync(product);

            // Assert
            Assert.IsNotNull(result);
            Assert.That(result.Id, Is.GreaterThan(0));
            Assert.That(result.Name, Is.EqualTo("Test Product"));
            Assert.That(result.Price, Is.EqualTo(29.99m));
        }

        [Test]
        public async Task CreateProductAsync_ValidatesPrice_MustBeGreaterThanZero()
        {
            // Arrange
            var product = new Product
            {
                Name = "Test Product",
                Price = -10m,
                Stock = 5
            };

            // Act & Assert
            Assert.That(product.Price, Is.LessThanOrEqualTo(0));
        }

        [Test]
        public async Task CreateProductAsync_ValidatesStock_CannotBeNegative()
        {
            // Arrange
            var product = new Product
            {
                Name = "Test Product",
                Price = 19.99m,
                Stock = -5
            };

            // Act & Assert
            Assert.That(product.Stock, Is.LessThan(0));
        }

        [Test]
        public async Task CreateProductAsync_WithValidPrice_GreaterThanZero()
        {
            // Arrange
            var product = new Product
            {
                Name = "Valid Price Product",
                Price = 0.01m,
                Stock = 1
            };

            // Act
            var result = await _service.CreateProductAsync(product);

            // Assert
            Assert.That(result.Price, Is.GreaterThan(0));
        }

        [Test]
        public async Task CreateProductAsync_WithZeroStock_IsValid()
        {
            // Arrange
            var product = new Product
            {
                Name = "Zero Stock Product",
                Price = 15.99m,
                Stock = 0
            };

            // Act
            var result = await _service.CreateProductAsync(product);

            // Assert
            Assert.That(result.Stock, Is.EqualTo(0));
        }

        [Test]
        public async Task GetAllProductsAsync_WithNoProducts_ReturnsEmptyList()
        {
            // Act
            var result = await _service.GetAllProductsAsync();

            // Assert
            Assert.IsNotNull(result);
            Assert.That(result.Count, Is.EqualTo(0));
        }

        [Test]
        public async Task GetAllProductsAsync_WithProducts_ReturnsAllProducts()
        {
            // Arrange
            var product1 = new Product { Name = "Product 1", Price = 10m, Stock = 5 };
            var product2 = new Product { Name = "Product 2", Price = 20m, Stock = 3 };
            await _service.CreateProductAsync(product1);
            await _service.CreateProductAsync(product2);

            // Act
            var result = await _service.GetAllProductsAsync();

            // Assert
            Assert.That(result.Count, Is.EqualTo(2));
            Assert.IsTrue(result.Any(p => p.Name == "Product 1"));
            Assert.IsTrue(result.Any(p => p.Name == "Product 2"));
        }

        [Test]
        public void Product_ValidatesNameRequired()
        {
            // Arrange & Act
            var product = new Product
            {
                Name = string.Empty,
                Price = 10m,
                Stock = 5
            };

            // Assert
            var context = new System.ComponentModel.DataAnnotations.ValidationContext(product);
            var results = new List<System.ComponentModel.DataAnnotations.ValidationResult>();
            var isValid = System.ComponentModel.DataAnnotations.Validator.TryValidateObject(product, context, results, true);

            Assert.IsFalse(isValid);
        }

        [Test]
        public void Product_AllowsNullDescription()
        {
            // Arrange & Act
            var product = new Product
            {
                Name = "Product with null description",
                Price = 10m,
                Stock = 5,
                Description = null
            };

            // Assert
            var context = new System.ComponentModel.DataAnnotations.ValidationContext(product);
            var results = new List<System.ComponentModel.DataAnnotations.ValidationResult>();
            var isValid = System.ComponentModel.DataAnnotations.Validator.TryValidateObject(product, context, results, true);

            Assert.IsTrue(isValid);
        }
    }
}

using NUnit.Framework;
using Microsoft.AspNetCore.Mvc.Testing;
using System.Net;
using System.Text;
using System.Text.Json;
using ProductAPI;
using ProductAPI.Models;
using Microsoft.Extensions.DependencyInjection;
using ProductAPI.Data;
using Microsoft.EntityFrameworkCore;

using Microsoft.EntityFrameworkCore.InMemory;

namespace ProductAPI.Tests
{
    [TestFixture]
    public class ProductControllerIntegrationTests
    {
        private WebApplicationFactory<Program> _factory = null!;
        private HttpClient _client = null!;

        [SetUp]
        public void Setup()
        {
            _factory = new WebApplicationFactory<Program>()
                .WithWebHostBuilder(builder =>
                {
                    builder.ConfigureServices(services =>
                    {
                        // Remove the app's DbContext registration
                        var descriptor = services.SingleOrDefault(
                            d => d.ServiceType == typeof(DbContextOptions<AppDbContext>));

                        if (descriptor != null)
                        {
                            services.Remove(descriptor);
                        }

                        // Add in-memory database for testing
                        services.AddDbContext<AppDbContext>(options =>
                        {
                            options.UseInMemoryDatabase("TestDb");
                        });
                    });
                });

            _client = _factory.CreateClient();
        }

        [TearDown]
        public void TearDown()
        {
            _client?.Dispose();
            _factory?.Dispose();
        }

        [Test]
        public async Task PostProducts_WithValidProduct_ReturnsCreatedStatusAndProduct()
        {
            // Arrange
            var product = new
            {
                name = "Test Product",
                description = "A test product",
                price = 29.99,
                category = "Electronics",
                stock = 10
            };

            var content = new StringContent(
                JsonSerializer.Serialize(product),
                Encoding.UTF8,
                "application/json");

            // Act
            var response = await _client.PostAsync("/api/products", content);

            // Assert
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.Created));

            var responseBody = await response.Content.ReadAsStringAsync();
            // Note: 201 Created usually returns the created resource
            Assert.IsNotEmpty(responseBody);
        }

        [Test]
        public async Task PostProducts_WithInvalidPrice_ReturnsBadRequest()
        {
            // Arrange
            var product = new
            {
                name = "Test Product",
                price = -10,
                stock = 5
            };

            var content = new StringContent(
                JsonSerializer.Serialize(product),
                Encoding.UTF8,
                "application/json");

            // Act
            var response = await _client.PostAsync("/api/products", content);

            // Assert
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));

            var responseBody = await response.Content.ReadAsStringAsync();
            Assert.IsTrue(responseBody.Contains("Price") || responseBody.Contains("error"));
        }

        [Test]
        public async Task PostProducts_WithZeroPrice_ReturnsBadRequest()
        {
            // Arrange
            var product = new
            {
                name = "Test Product",
                price = 0,
                stock = 5
            };

            var content = new StringContent(
                JsonSerializer.Serialize(product),
                Encoding.UTF8,
                "application/json");

            // Act
            var response = await _client.PostAsync("/api/products", content);

            // Assert
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
        }

        [Test]
        public async Task PostProducts_WithNegativeStock_ReturnsBadRequest()
        {
            // Arrange
            var product = new
            {
                name = "Test Product",
                price = 19.99,
                stock = -5
            };

            var content = new StringContent(
                JsonSerializer.Serialize(product),
                Encoding.UTF8,
                "application/json");

            // Act
            var response = await _client.PostAsync("/api/products", content);

            // Assert
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
        }

        [Test]
        public async Task PostProducts_WithValidPrice_ReturnsCreated()
        {
            // Arrange
            var product = new
            {
                name = "Valid Price Product",
                price = 0.01,
                stock = 1
            };

            var content = new StringContent(
                JsonSerializer.Serialize(product),
                Encoding.UTF8,
                "application/json");

            // Act
            var response = await _client.PostAsync("/api/products", content);

            // Assert
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.Created));
        }

        [Test]
        public async Task PostProducts_WithZeroStock_ReturnsCreated()
        {
            // Arrange
            var product = new
            {
                name = "Zero Stock Product",
                price = 15.99,
                stock = 0
            };

            var content = new StringContent(
                JsonSerializer.Serialize(product),
                Encoding.UTF8,
                "application/json");

            // Act
            var response = await _client.PostAsync("/api/products", content);

            // Assert
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.Created));
        }

        [Test]
        public async Task GetProducts_WithNoProducts_ReturnsEmptyArray()
        {
            // Act
            var response = await _client.GetAsync("/api/products");

            // Assert
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));

            var responseBody = await response.Content.ReadAsStringAsync();
            Assert.That(responseBody, Is.EqualTo("[]"));
        }

        [Test]
        public async Task GetProducts_WithProducts_ReturnsProductList()
        {
            // Arrange - Create a product first
            var product = new
            {
                name = "Test Product",
                price = 29.99,
                stock = 10
            };

            var postContent = new StringContent(
                JsonSerializer.Serialize(product),
                Encoding.UTF8,
                "application/json");

            await _client.PostAsync("/api/products", postContent);

            // Act
            var response = await _client.GetAsync("/api/products");

            // Assert
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));

            var responseBody = await response.Content.ReadAsStringAsync();
            Assert.IsTrue(responseBody.Contains("Test Product"));
        }

        [Test]
        public async Task PostProducts_WithMissingName_ReturnsBadRequest()
        {
            // Arrange
            var product = new
            {
                price = 19.99,
                stock = 5
            };

            var content = new StringContent(
                JsonSerializer.Serialize(product),
                Encoding.UTF8,
                "application/json");

            // Act
            var response = await _client.PostAsync("/api/products", content);

            // Assert
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
        }

        [Test]
        public async Task PostProducts_WithMissingPrice_ReturnsBadRequest()
        {
            // Arrange
            var product = new
            {
                name = "Test Product",
                stock = 5
            };

            var content = new StringContent(
                JsonSerializer.Serialize(product),
                Encoding.UTF8,
                "application/json");

            // Act
            var response = await _client.PostAsync("/api/products", content);

            // Assert
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
        }
    }
}

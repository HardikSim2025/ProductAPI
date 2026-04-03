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

namespace ProductAPI.Tests
{
    [TestFixture]
    public class CategoriesControllerIntegrationTests
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
                            options.UseInMemoryDatabase("TestDbCategories");
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

        #region POST Tests

        [Test]
        public async Task PostCategories_WithValidCategory_ReturnsCreatedStatus()
        {
            // Arrange
            var category = new
            {
                name = "Electronics",
                description = "Electronic devices"
            };

            var content = new StringContent(
                JsonSerializer.Serialize(category),
                Encoding.UTF8,
                "application/json");

            // Act
            var response = await _client.PostAsync("/api/categories", content);

            // Assert
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.Created));

            var responseBody = await response.Content.ReadAsStringAsync();
            Assert.IsNotEmpty(responseBody);
            Assert.IsTrue(responseBody.Contains("Electronics"));
        }

        [Test]
        public async Task PostCategories_WithoutName_ReturnsBadRequest()
        {
            // Arrange
            var category = new
            {
                description = "Missing name"
            };

            var content = new StringContent(
                JsonSerializer.Serialize(category),
                Encoding.UTF8,
                "application/json");

            // Act
            var response = await _client.PostAsync("/api/categories", content);

            // Assert
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
        }

        [Test]
        public async Task PostCategories_WithOnlyName_ReturnsCreated()
        {
            // Arrange
            var category = new
            {
                name = "Books"
            };

            var content = new StringContent(
                JsonSerializer.Serialize(category),
                Encoding.UTF8,
                "application/json");

            // Act
            var response = await _client.PostAsync("/api/categories", content);

            // Assert
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.Created));
        }

        [Test]
        public async Task PostCategories_WithParentCategoryId_ReturnsCreated()
        {
            // Arrange - First create a parent category
            var parentCategory = new { name = "Parent Category" };
            var parentContent = new StringContent(
                JsonSerializer.Serialize(parentCategory),
                Encoding.UTF8,
                "application/json");
            var parentResponse = await _client.PostAsync("/api/categories", parentContent);
            var parentBody = await parentResponse.Content.ReadAsStringAsync();
            var parentJson = JsonDocument.Parse(parentBody);
            var parentId = parentJson.RootElement.GetProperty("id").GetInt32();

            // Create child category
            var childCategory = new
            {
                name = "Child Category",
                parentCategoryId = parentId
            };

            var childContent = new StringContent(
                JsonSerializer.Serialize(childCategory),
                Encoding.UTF8,
                "application/json");

            // Act
            var response = await _client.PostAsync("/api/categories", childContent);

            // Assert
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.Created));
            var responseBody = await response.Content.ReadAsStringAsync();
            Assert.IsTrue(responseBody.Contains("Child Category"));
        }

        [Test]
        public async Task PostCategories_WithInvalidParentCategoryId_ReturnsBadRequest()
        {
            // Arrange
            var category = new
            {
                name = "Child Category",
                parentCategoryId = 999
            };

            var content = new StringContent(
                JsonSerializer.Serialize(category),
                Encoding.UTF8,
                "application/json");

            // Act
            var response = await _client.PostAsync("/api/categories", content);

            // Assert
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
            var responseBody = await response.Content.ReadAsStringAsync();
            Assert.IsTrue(responseBody.Contains("Parent category does not exist") || responseBody.Contains("error"));
        }

        [Test]
        public async Task PostCategories_WithTooLongName_ReturnsBadRequest()
        {
            // Arrange
            var longName = new string('a', 201);
            var category = new
            {
                name = longName
            };

            var content = new StringContent(
                JsonSerializer.Serialize(category),
                Encoding.UTF8,
                "application/json");

            // Act
            var response = await _client.PostAsync("/api/categories", content);

            // Assert
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
        }

        #endregion

        #region GET All Categories Tests

        [Test]
        public async Task GetCategories_WithNoData_ReturnsEmptyList()
        {
            // Act
            var response = await _client.GetAsync("/api/categories");

            // Assert
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
            var responseBody = await response.Content.ReadAsStringAsync();
            Assert.IsTrue(responseBody.Contains("[]"));
        }

        [Test]
        public async Task GetCategories_WithMultipleCategories_ReturnsAll()
        {
            // Arrange - Create multiple categories
            for (int i = 1; i <= 3; i++)
            {
                var category = new { name = $"Category {i}" };
                var content = new StringContent(
                    JsonSerializer.Serialize(category),
                    Encoding.UTF8,
                    "application/json");
                await _client.PostAsync("/api/categories", content);
            }

            // Act
            var response = await _client.GetAsync("/api/categories");

            // Assert
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
            var responseBody = await response.Content.ReadAsStringAsync();
            var jsonDoc = JsonDocument.Parse(responseBody);
            Assert.That(jsonDoc.RootElement.GetArrayLength(), Is.EqualTo(3));
        }

        [Test]
        public async Task GetCategories_ReturnsHierarchyInformation()
        {
            // Arrange - Create parent and child
            var parent = new { name = "Parent" };
            var parentContent = new StringContent(
                JsonSerializer.Serialize(parent),
                Encoding.UTF8,
                "application/json");
            var parentResponse = await _client.PostAsync("/api/categories", parentContent);
            var parentBody = await parentResponse.Content.ReadAsStringAsync();
            var parentJson = JsonDocument.Parse(parentBody);
            var parentId = parentJson.RootElement.GetProperty("id").GetInt32();

            var child = new { name = "Child", parentCategoryId = parentId };
            var childContent = new StringContent(
                JsonSerializer.Serialize(child),
                Encoding.UTF8,
                "application/json");
            await _client.PostAsync("/api/categories", childContent);

            // Act
            var response = await _client.GetAsync("/api/categories");

            // Assert
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
            var responseBody = await response.Content.ReadAsStringAsync();
            Assert.IsTrue(responseBody.Contains("childCategories"));
        }

        #endregion

        #region GET Single Category Tests

        [Test]
        public async Task GetCategoryById_WithValidId_ReturnsCategory()
        {
            // Arrange - Create a category
            var category = new { name = "Test Category" };
            var createContent = new StringContent(
                JsonSerializer.Serialize(category),
                Encoding.UTF8,
                "application/json");
            var createResponse = await _client.PostAsync("/api/categories", createContent);
            var createBody = await createResponse.Content.ReadAsStringAsync();
            var createJson = JsonDocument.Parse(createBody);
            var categoryId = createJson.RootElement.GetProperty("id").GetInt32();

            // Act
            var response = await _client.GetAsync($"/api/categories/{categoryId}");

            // Assert
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
            var responseBody = await response.Content.ReadAsStringAsync();
            Assert.IsTrue(responseBody.Contains("Test Category"));
        }

        [Test]
        public async Task GetCategoryById_WithInvalidId_ReturnsNotFound()
        {
            // Act
            var response = await _client.GetAsync("/api/categories/999");

            // Assert
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.NotFound));
            var responseBody = await response.Content.ReadAsStringAsync();
            Assert.IsTrue(responseBody.Contains("not found"));
        }

        [Test]
        public async Task GetCategoryById_ReturnsAllProperties()
        {
            // Arrange
            var category = new
            {
                name = "Full Details Category",
                description = "This is a test description"
            };
            var content = new StringContent(
                JsonSerializer.Serialize(category),
                Encoding.UTF8,
                "application/json");
            var createResponse = await _client.PostAsync("/api/categories", content);
            var createBody = await createResponse.Content.ReadAsStringAsync();
            var createJson = JsonDocument.Parse(createBody);
            var categoryId = createJson.RootElement.GetProperty("id").GetInt32();

            // Act
            var response = await _client.GetAsync($"/api/categories/{categoryId}");

            // Assert
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
            var responseBody = await response.Content.ReadAsStringAsync();
            var json = JsonDocument.Parse(responseBody);
            Assert.That(json.RootElement.GetProperty("name").GetString(), Is.EqualTo("Full Details Category"));
            Assert.That(json.RootElement.GetProperty("description").GetString(), Is.EqualTo("This is a test description"));
        }

        #endregion

        #region PUT Tests

        [Test]
        public async Task PutCategory_WithValidUpdate_ReturnsUpdatedCategory()
        {
            // Arrange - Create a category
            var originalCategory = new { name = "Original Name" };
            var createContent = new StringContent(
                JsonSerializer.Serialize(originalCategory),
                Encoding.UTF8,
                "application/json");
            var createResponse = await _client.PostAsync("/api/categories", createContent);
            var createBody = await createResponse.Content.ReadAsStringAsync();
            var createJson = JsonDocument.Parse(createBody);
            var categoryId = createJson.RootElement.GetProperty("id").GetInt32();

            // Update the category
            var updatedCategory = new
            {
                name = "Updated Name",
                description = "Updated Description"
            };
            var updateContent = new StringContent(
                JsonSerializer.Serialize(updatedCategory),
                Encoding.UTF8,
                "application/json");

            // Act
            var response = await _client.PutAsync($"/api/categories/{categoryId}", updateContent);

            // Assert
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
            var responseBody = await response.Content.ReadAsStringAsync();
            Assert.IsTrue(responseBody.Contains("Updated Name"));
            Assert.IsTrue(responseBody.Contains("Updated Description"));
        }

        [Test]
        public async Task PutCategory_WithInvalidId_ReturnsNotFound()
        {
            // Arrange
            var category = new { name = "Test" };
            var content = new StringContent(
                JsonSerializer.Serialize(category),
                Encoding.UTF8,
                "application/json");

            // Act
            var response = await _client.PutAsync("/api/categories/999", content);

            // Assert
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.NotFound));
        }

        [Test]
        public async Task PutCategory_WithSelfReference_ReturnsBadRequest()
        {
            // Arrange - Create a category
            var category = new { name = "Test Category" };
            var createContent = new StringContent(
                JsonSerializer.Serialize(category),
                Encoding.UTF8,
                "application/json");
            var createResponse = await _client.PostAsync("/api/categories", createContent);
            var createBody = await createResponse.Content.ReadAsStringAsync();
            var createJson = JsonDocument.Parse(createBody);
            var categoryId = createJson.RootElement.GetProperty("id").GetInt32();

            // Try to set itself as parent
            var selfReferencingUpdate = new
            {
                name = "Self Referencing",
                parentCategoryId = categoryId
            };
            var updateContent = new StringContent(
                JsonSerializer.Serialize(selfReferencingUpdate),
                Encoding.UTF8,
                "application/json");

            // Act
            var response = await _client.PutAsync($"/api/categories/{categoryId}", updateContent);

            // Assert
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
            var responseBody = await response.Content.ReadAsStringAsync();
            Assert.IsTrue(responseBody.Contains("cannot be its own parent") || responseBody.Contains("error"));
        }

        [Test]
        public async Task PutCategory_WithCircularReference_ReturnsBadRequest()
        {
            // Arrange - Create three categories: A -> B -> C
            var catA = new { name = "Category A" };
            var aContent = new StringContent(
                JsonSerializer.Serialize(catA),
                Encoding.UTF8,
                "application/json");
            var aResponse = await _client.PostAsync("/api/categories", aContent);
            var aBody = await aResponse.Content.ReadAsStringAsync();
            var aJson = JsonDocument.Parse(aBody);
            var aId = aJson.RootElement.GetProperty("id").GetInt32();

            var catB = new { name = "Category B", parentCategoryId = aId };
            var bContent = new StringContent(
                JsonSerializer.Serialize(catB),
                Encoding.UTF8,
                "application/json");
            var bResponse = await _client.PostAsync("/api/categories", bContent);
            var bBody = await bResponse.Content.ReadAsStringAsync();
            var bJson = JsonDocument.Parse(bBody);
            var bId = bJson.RootElement.GetProperty("id").GetInt32();

            var catC = new { name = "Category C", parentCategoryId = bId };
            var cContent = new StringContent(
                JsonSerializer.Serialize(catC),
                Encoding.UTF8,
                "application/json");
            var cResponse = await _client.PostAsync("/api/categories", cContent);
            var cBody = await cResponse.Content.ReadAsStringAsync();
            var cJson = JsonDocument.Parse(cBody);
            var cId = cJson.RootElement.GetProperty("id").GetInt32();

            // Try to set A's parent to C (which would create circular reference)
            var circularUpdate = new
            {
                name = "Category A",
                parentCategoryId = cId
            };
            var updateContent = new StringContent(
                JsonSerializer.Serialize(circularUpdate),
                Encoding.UTF8,
                "application/json");

            // Act
            var response = await _client.PutAsync($"/api/categories/{aId}", updateContent);

            // Assert
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
            var responseBody = await response.Content.ReadAsStringAsync();
            Assert.IsTrue(responseBody.Contains("circular") || responseBody.Contains("error"));
        }

        [Test]
        public async Task PutCategory_WithInvalidParentId_ReturnsBadRequest()
        {
            // Arrange - Create a category
            var category = new { name = "Test Category" };
            var createContent = new StringContent(
                JsonSerializer.Serialize(category),
                Encoding.UTF8,
                "application/json");
            var createResponse = await _client.PostAsync("/api/categories", createContent);
            var createBody = await createResponse.Content.ReadAsStringAsync();
            var createJson = JsonDocument.Parse(createBody);
            var categoryId = createJson.RootElement.GetProperty("id").GetInt32();

            // Try to set non-existent parent
            var invalidParentUpdate = new
            {
                name = "Test Category",
                parentCategoryId = 999
            };
            var updateContent = new StringContent(
                JsonSerializer.Serialize(invalidParentUpdate),
                Encoding.UTF8,
                "application/json");

            // Act
            var response = await _client.PutAsync($"/api/categories/{categoryId}", updateContent);

            // Assert
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
            var responseBody = await response.Content.ReadAsStringAsync();
            Assert.IsTrue(responseBody.Contains("does not exist") || responseBody.Contains("error"));
        }

        #endregion

        #region DELETE Tests

        [Test]
        public async Task DeleteCategory_WithValidId_ReturnsSuccess()
        {
            // Arrange - Create a category
            var category = new { name = "To Delete" };
            var createContent = new StringContent(
                JsonSerializer.Serialize(category),
                Encoding.UTF8,
                "application/json");
            var createResponse = await _client.PostAsync("/api/categories", createContent);
            var createBody = await createResponse.Content.ReadAsStringAsync();
            var createJson = JsonDocument.Parse(createBody);
            var categoryId = createJson.RootElement.GetProperty("id").GetInt32();

            // Act
            var response = await _client.DeleteAsync($"/api/categories/{categoryId}");

            // Assert
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
            var responseBody = await response.Content.ReadAsStringAsync();
            Assert.IsTrue(responseBody.Contains("deleted successfully"));
        }

        [Test]
        public async Task DeleteCategory_WithInvalidId_ReturnsNotFound()
        {
            // Act
            var response = await _client.DeleteAsync("/api/categories/999");

            // Assert
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.NotFound));
        }

        [Test]
        public async Task DeleteCategory_WithChildren_ReturnsConflict()
        {
            // Arrange - Create parent and child
            var parent = new { name = "Parent" };
            var parentContent = new StringContent(
                JsonSerializer.Serialize(parent),
                Encoding.UTF8,
                "application/json");
            var parentResponse = await _client.PostAsync("/api/categories", parentContent);
            var parentBody = await parentResponse.Content.ReadAsStringAsync();
            var parentJson = JsonDocument.Parse(parentBody);
            var parentId = parentJson.RootElement.GetProperty("id").GetInt32();

            var child = new { name = "Child", parentCategoryId = parentId };
            var childContent = new StringContent(
                JsonSerializer.Serialize(child),
                Encoding.UTF8,
                "application/json");
            await _client.PostAsync("/api/categories", childContent);

            // Act - Try to delete parent
            var response = await _client.DeleteAsync($"/api/categories/{parentId}");

            // Assert
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.Conflict));
            var responseBody = await response.Content.ReadAsStringAsync();
            Assert.IsTrue(responseBody.Contains("child categories") || responseBody.Contains("error"));
        }

        [Test]
        public async Task DeleteCategory_VerifiesCategoryIsDeleted()
        {
            // Arrange - Create a category
            var category = new { name = "To Verify Deletion" };
            var createContent = new StringContent(
                JsonSerializer.Serialize(category),
                Encoding.UTF8,
                "application/json");
            var createResponse = await _client.PostAsync("/api/categories", createContent);
            var createBody = await createResponse.Content.ReadAsStringAsync();
            var createJson = JsonDocument.Parse(createBody);
            var categoryId = createJson.RootElement.GetProperty("id").GetInt32();

            // Delete the category
            await _client.DeleteAsync($"/api/categories/{categoryId}");

            // Act - Try to get the deleted category
            var getResponse = await _client.GetAsync($"/api/categories/{categoryId}");

            // Assert
            Assert.That(getResponse.StatusCode, Is.EqualTo(HttpStatusCode.NotFound));
        }

        #endregion

        #region Orphaned Category Tests

        [Test]
        public async Task OrphanedCategory_CanBeDeletedAfterParentDelete()
        {
            // Note: This test validates the behavior when a parent is deleted
            // In a typical orphaning scenario, child categories would need explicit handling
            // This test documents the current behavior

            // Arrange - Create parent
            var parent = new { name = "Parent to Orphan" };
            var parentContent = new StringContent(
                JsonSerializer.Serialize(parent),
                Encoding.UTF8,
                "application/json");
            var parentResponse = await _client.PostAsync("/api/categories", parentContent);
            var parentBody = await parentResponse.Content.ReadAsStringAsync();
            var parentJson = JsonDocument.Parse(parentBody);
            var parentId = parentJson.RootElement.GetProperty("id").GetInt32();

            var child = new { name = "Child", parentCategoryId = parentId };
            var childContent = new StringContent(
                JsonSerializer.Serialize(child),
                Encoding.UTF8,
                "application/json");
            var childResponse = await _client.PostAsync("/api/categories", childContent);
            var childBody = await childResponse.Content.ReadAsStringAsync();
            var childJson = JsonDocument.Parse(childBody);
            var childId = childJson.RootElement.GetProperty("id").GetInt32();

            // Try to delete parent (should fail because it has children)
            var deleteParentResponse = await _client.DeleteAsync($"/api/categories/{parentId}");

            // Assert
            Assert.That(deleteParentResponse.StatusCode, Is.EqualTo(HttpStatusCode.Conflict));

            // Verify child still exists
            var getChildResponse = await _client.GetAsync($"/api/categories/{childId}");
            Assert.That(getChildResponse.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        }

        #endregion

        #region Error Response Format Tests

        [Test]
        public async Task ErrorResponse_HasConsistentJsonFormat()
        {
            // Arrange - Invalid request
            var category = new { invalid_field = "value" };
            var content = new StringContent(
                JsonSerializer.Serialize(category),
                Encoding.UTF8,
                "application/json");

            // Act
            var response = await _client.PostAsync("/api/categories", content);

            // Assert
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
            var responseBody = await response.Content.ReadAsStringAsync();
            // Should be valid JSON
            Assert.DoesNotThrow(() => JsonDocument.Parse(responseBody));
        }

        #endregion
    }
}

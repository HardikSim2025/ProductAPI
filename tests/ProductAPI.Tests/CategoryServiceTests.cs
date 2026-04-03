using NUnit.Framework;
using ProductAPI.Data;
using ProductAPI.Models;
using ProductAPI.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;

namespace ProductAPI.Tests
{
    [TestFixture]
    public class CategoryServiceTests
    {
        private AppDbContext _context = null!;
        private CategoryService _service = null!;
        private Mock<ILogger<CategoryService>> _mockLogger = null!;

        [SetUp]
        public void Setup()
        {
            // Create in-memory database context with unique name for each test
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;
            _context = new AppDbContext(options);
            _mockLogger = new Mock<ILogger<CategoryService>>();
            _service = new CategoryService(_context, _mockLogger.Object);
        }

        [TearDown]
        public void TearDown()
        {
            _context?.Dispose();
        }

        #region CreateCategoryAsync Tests

        [Test]
        public async Task CreateCategoryAsync_WithValidCategory_ReturnsCategoryWithId()
        {
            // Arrange
            var category = new Category
            {
                Name = "Electronics",
                Description = "Electronic devices"
            };

            // Act
            var result = await _service.CreateCategoryAsync(category);

            // Assert
            Assert.IsNotNull(result);
            Assert.That(result.Id, Is.GreaterThan(0));
            Assert.That(result.Name, Is.EqualTo("Electronics"));
            Assert.That(result.Description, Is.EqualTo("Electronic devices"));
            Assert.That(result.ParentCategoryId, Is.Null);
        }

        [Test]
        public async Task CreateCategoryAsync_WithParentCategory_ReturnsCategoryWithParentId()
        {
            // Arrange
            var parentCategory = new Category { Name = "Parent" };
            var createdParent = await _service.CreateCategoryAsync(parentCategory);

            var childCategory = new Category
            {
                Name = "Child",
                ParentCategoryId = createdParent.Id
            };

            // Act
            var result = await _service.CreateCategoryAsync(childCategory);

            // Assert
            Assert.IsNotNull(result);
            Assert.That(result.ParentCategoryId, Is.EqualTo(createdParent.Id));
            Assert.That(result.Name, Is.EqualTo("Child"));
        }

        [Test]
        public async Task CreateCategoryAsync_WithoutDescription_ReturnsCategory()
        {
            // Arrange
            var category = new Category { Name = "Test Category" };

            // Act
            var result = await _service.CreateCategoryAsync(category);

            // Assert
            Assert.IsNotNull(result);
            Assert.That(result.Name, Is.EqualTo("Test Category"));
            Assert.That(result.Description, Is.Null);
        }

        #endregion

        #region GetAllCategoriesAsync Tests

        [Test]
        public async Task GetAllCategoriesAsync_WithMultipleCategories_ReturnsAllCategories()
        {
            // Arrange
            var cat1 = new Category { Name = "Category 1" };
            var cat2 = new Category { Name = "Category 2" };
            var cat3 = new Category { Name = "Category 3" };

            await _service.CreateCategoryAsync(cat1);
            await _service.CreateCategoryAsync(cat2);
            await _service.CreateCategoryAsync(cat3);

            // Act
            var result = await _service.GetAllCategoriesAsync();

            // Assert
            Assert.That(result.Count, Is.EqualTo(3));
            Assert.That(result.Any(c => c.Name == "Category 1"), Is.True);
            Assert.That(result.Any(c => c.Name == "Category 2"), Is.True);
            Assert.That(result.Any(c => c.Name == "Category 3"), Is.True);
        }

        [Test]
        public async Task GetAllCategoriesAsync_WithEmptyDatabase_ReturnsEmptyList()
        {
            // Act
            var result = await _service.GetAllCategoriesAsync();

            // Assert
            Assert.That(result.Count, Is.EqualTo(0));
        }

        [Test]
        public async Task GetAllCategoriesAsync_IncludesHierarchyInformation()
        {
            // Arrange
            var parent = new Category { Name = "Parent" };
            var createdParent = await _service.CreateCategoryAsync(parent);

            var child = new Category { Name = "Child", ParentCategoryId = createdParent.Id };
            await _service.CreateCategoryAsync(child);

            // Act
            var result = await _service.GetAllCategoriesAsync();

            // Assert
            Assert.That(result.Count, Is.EqualTo(2));
            var parentResult = result.FirstOrDefault(c => c.Id == createdParent.Id);
            Assert.IsNotNull(parentResult);
            Assert.That(parentResult.ChildCategories.Count, Is.EqualTo(1));
        }

        #endregion

        #region GetCategoryByIdAsync Tests

        [Test]
        public async Task GetCategoryByIdAsync_WithValidId_ReturnsCategory()
        {
            // Arrange
            var category = new Category { Name = "Test Category" };
            var created = await _service.CreateCategoryAsync(category);

            // Act
            var result = await _service.GetCategoryByIdAsync(created.Id);

            // Assert
            Assert.IsNotNull(result);
            Assert.That(result.Id, Is.EqualTo(created.Id));
            Assert.That(result.Name, Is.EqualTo("Test Category"));
        }

        [Test]
        public async Task GetCategoryByIdAsync_WithInvalidId_ReturnsNull()
        {
            // Act
            var result = await _service.GetCategoryByIdAsync(999);

            // Assert
            Assert.IsNull(result);
        }

        [Test]
        public async Task GetCategoryByIdAsync_IncludesChildCategories()
        {
            // Arrange
            var parent = new Category { Name = "Parent" };
            var createdParent = await _service.CreateCategoryAsync(parent);

            var child1 = new Category { Name = "Child 1", ParentCategoryId = createdParent.Id };
            var child2 = new Category { Name = "Child 2", ParentCategoryId = createdParent.Id };
            await _service.CreateCategoryAsync(child1);
            await _service.CreateCategoryAsync(child2);

            // Act
            var result = await _service.GetCategoryByIdAsync(createdParent.Id);

            // Assert
            Assert.IsNotNull(result);
            Assert.That(result.ChildCategories.Count, Is.EqualTo(2));
        }

        #endregion

        #region UpdateCategoryAsync Tests

        [Test]
        public async Task UpdateCategoryAsync_WithValidUpdate_UpdatesCategory()
        {
            // Arrange
            var category = new Category { Name = "Original Name" };
            var created = await _service.CreateCategoryAsync(category);

            var updated = new Category
            {
                Name = "Updated Name",
                Description = "Updated Description"
            };

            // Act
            var result = await _service.UpdateCategoryAsync(created.Id, updated);

            // Assert
            Assert.That(result.Name, Is.EqualTo("Updated Name"));
            Assert.That(result.Description, Is.EqualTo("Updated Description"));
        }

        [Test]
        public void UpdateCategoryAsync_WithInvalidId_ThrowsException()
        {
            // Arrange
            var category = new Category { Name = "Test" };

            // Act & Assert
            Assert.ThrowsAsync<InvalidOperationException>(async () =>
                await _service.UpdateCategoryAsync(999, category));
        }

        [Test]
        public async Task UpdateCategoryAsync_WithNewParentCategory_UpdatesParent()
        {
            // Arrange
            var parent = new Category { Name = "New Parent" };
            var createdParent = await _service.CreateCategoryAsync(parent);

            var category = new Category { Name = "Child" };
            var created = await _service.CreateCategoryAsync(category);

            var updated = new Category
            {
                Name = "Child",
                ParentCategoryId = createdParent.Id
            };

            // Act
            var result = await _service.UpdateCategoryAsync(created.Id, updated);

            // Assert
            Assert.That(result.ParentCategoryId, Is.EqualTo(createdParent.Id));
        }

        #endregion

        #region DeleteCategoryAsync Tests

        [Test]
        public async Task DeleteCategoryAsync_WithValidId_DeletesCategory()
        {
            // Arrange
            var category = new Category { Name = "To Delete" };
            var created = await _service.CreateCategoryAsync(category);

            // Act
            var result = await _service.DeleteCategoryAsync(created.Id);

            // Assert
            Assert.That(result, Is.True);
            var deleted = await _service.GetCategoryByIdAsync(created.Id);
            Assert.IsNull(deleted);
        }

        [Test]
        public async Task DeleteCategoryAsync_WithInvalidId_ReturnsFalse()
        {
            // Act
            var result = await _service.DeleteCategoryAsync(999);

            // Assert
            Assert.That(result, Is.False);
        }

        #endregion

        #region HasChildrenAsync Tests

        [Test]
        public async Task HasChildrenAsync_WithChildren_ReturnsTrue()
        {
            // Arrange
            var parent = new Category { Name = "Parent" };
            var createdParent = await _service.CreateCategoryAsync(parent);

            var child = new Category { Name = "Child", ParentCategoryId = createdParent.Id };
            await _service.CreateCategoryAsync(child);

            // Act
            var result = await _service.HasChildrenAsync(createdParent.Id);

            // Assert
            Assert.That(result, Is.True);
        }

        [Test]
        public async Task HasChildrenAsync_WithoutChildren_ReturnsFalse()
        {
            // Arrange
            var category = new Category { Name = "No Children" };
            var created = await _service.CreateCategoryAsync(category);

            // Act
            var result = await _service.HasChildrenAsync(created.Id);

            // Assert
            Assert.That(result, Is.False);
        }

        [Test]
        public async Task HasChildrenAsync_WithMultipleChildren_ReturnsTrue()
        {
            // Arrange
            var parent = new Category { Name = "Parent" };
            var createdParent = await _service.CreateCategoryAsync(parent);

            var child1 = new Category { Name = "Child 1", ParentCategoryId = createdParent.Id };
            var child2 = new Category { Name = "Child 2", ParentCategoryId = createdParent.Id };
            await _service.CreateCategoryAsync(child1);
            await _service.CreateCategoryAsync(child2);

            // Act
            var result = await _service.HasChildrenAsync(createdParent.Id);

            // Assert
            Assert.That(result, Is.True);
        }

        #endregion

        #region CategoryExistsAsync Tests

        [Test]
        public async Task CategoryExistsAsync_WithValidId_ReturnsTrue()
        {
            // Arrange
            var category = new Category { Name = "Exists" };
            var created = await _service.CreateCategoryAsync(category);

            // Act
            var result = await _service.CategoryExistsAsync(created.Id);

            // Assert
            Assert.That(result, Is.True);
        }

        [Test]
        public async Task CategoryExistsAsync_WithInvalidId_ReturnsFalse()
        {
            // Act
            var result = await _service.CategoryExistsAsync(999);

            // Assert
            Assert.That(result, Is.False);
        }

        #endregion

        #region WouldCreateCircularReferenceAsync Tests

        [Test]
        public async Task WouldCreateCircularReferenceAsync_WithNullParent_ReturnsFalse()
        {
            // Arrange
            var category = new Category { Name = "Test" };
            var created = await _service.CreateCategoryAsync(category);

            // Act
            var result = await _service.WouldCreateCircularReferenceAsync(created.Id, null);

            // Assert
            Assert.That(result, Is.False);
        }

        [Test]
        public async Task WouldCreateCircularReferenceAsync_WithSelfReference_ReturnsTrue()
        {
            // Arrange
            var category = new Category { Name = "Test" };
            var created = await _service.CreateCategoryAsync(category);

            // Act
            var result = await _service.WouldCreateCircularReferenceAsync(created.Id, created.Id);

            // Assert
            Assert.That(result, Is.True);
        }

        [Test]
        public async Task WouldCreateCircularReferenceAsync_WithValidParent_ReturnsFalse()
        {
            // Arrange
            var parent = new Category { Name = "Parent" };
            var createdParent = await _service.CreateCategoryAsync(parent);

            var child = new Category { Name = "Child" };
            var createdChild = await _service.CreateCategoryAsync(child);

            // Act
            var result = await _service.WouldCreateCircularReferenceAsync(createdChild.Id, createdParent.Id);

            // Assert
            Assert.That(result, Is.False);
        }

        [Test]
        public async Task WouldCreateCircularReferenceAsync_WithIndirectCircularReference_ReturnsTrue()
        {
            // Arrange: GrandParent -> Parent -> Child
            var grandParent = new Category { Name = "GrandParent" };
            var createdGrandParent = await _service.CreateCategoryAsync(grandParent);

            var parent = new Category { Name = "Parent", ParentCategoryId = createdGrandParent.Id };
            var createdParent = await _service.CreateCategoryAsync(parent);

            var child = new Category { Name = "Child", ParentCategoryId = createdParent.Id };
            var createdChild = await _service.CreateCategoryAsync(child);

            // Act: Try to set GrandParent's parent to Child (which would create: GrandParent -> Parent -> Child -> GrandParent)
            var result = await _service.WouldCreateCircularReferenceAsync(createdGrandParent.Id, createdChild.Id);

            // Assert
            Assert.That(result, Is.True);
        }

        [Test]
        public async Task WouldCreateCircularReferenceAsync_WithNonExistentParent_ReturnsFalse()
        {
            // Arrange
            var category = new Category { Name = "Test" };
            var created = await _service.CreateCategoryAsync(category);

            // Act
            var result = await _service.WouldCreateCircularReferenceAsync(created.Id, 999);

            // Assert
            Assert.That(result, Is.False);
        }

        #endregion
    }
}

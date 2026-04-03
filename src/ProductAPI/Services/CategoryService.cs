using ProductAPI.Data;
using ProductAPI.Models;
using Microsoft.EntityFrameworkCore;

namespace ProductAPI.Services
{
    public interface ICategoryService
    {
        Task<Category> CreateCategoryAsync(Category category);
        Task<List<Category>> GetAllCategoriesAsync();
        Task<Category?> GetCategoryByIdAsync(int id);
        Task<Category> UpdateCategoryAsync(int id, Category category);
        Task<bool> DeleteCategoryAsync(int id);
        Task<bool> HasChildrenAsync(int categoryId);
        Task<bool> CategoryExistsAsync(int categoryId);
        Task<bool> WouldCreateCircularReferenceAsync(int categoryId, int? parentCategoryId);
    }

    public class CategoryService : ICategoryService
    {
        private readonly AppDbContext _context;
        private readonly ILogger<CategoryService> _logger;

        public CategoryService(AppDbContext context, ILogger<CategoryService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<Category> CreateCategoryAsync(Category category)
        {
            _context.Categories.Add(category);
            await _context.SaveChangesAsync();
            return category;
        }

        public async Task<List<Category>> GetAllCategoriesAsync()
        {
            return await _context.Categories
                .Include(c => c.ParentCategory)
                .Include(c => c.ChildCategories)
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task<Category?> GetCategoryByIdAsync(int id)
        {
            return await _context.Categories
                .Include(c => c.ParentCategory)
                .Include(c => c.ChildCategories)
                .AsNoTracking()
                .FirstOrDefaultAsync(c => c.Id == id);
        }

        public async Task<Category> UpdateCategoryAsync(int id, Category category)
        {
            var existingCategory = await _context.Categories.FindAsync(id);
            if (existingCategory == null)
                throw new InvalidOperationException($"Category with id {id} not found");

            existingCategory.Name = category.Name;
            existingCategory.Description = category.Description;
            existingCategory.ParentCategoryId = category.ParentCategoryId;

            _context.Categories.Update(existingCategory);
            await _context.SaveChangesAsync();
            return existingCategory;
        }

        public async Task<bool> DeleteCategoryAsync(int id)
        {
            var category = await _context.Categories.FindAsync(id);
            if (category == null)
                return false;

            _context.Categories.Remove(category);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> HasChildrenAsync(int categoryId)
        {
            return await _context.Categories
                .AnyAsync(c => c.ParentCategoryId == categoryId);
        }

        public async Task<bool> CategoryExistsAsync(int categoryId)
        {
            return await _context.Categories
                .AnyAsync(c => c.Id == categoryId);
        }

        /// <summary>
        /// Checks if setting parentCategoryId would create a circular reference
        /// </summary>
        public async Task<bool> WouldCreateCircularReferenceAsync(int categoryId, int? parentCategoryId)
        {
            // No parent = no circular reference
            if (!parentCategoryId.HasValue)
                return false;

            // Self-reference check
            if (categoryId == parentCategoryId.Value)
                return true;

            // Check for circular reference by traversing up the hierarchy
            var currentParentId = parentCategoryId;
            var visited = new HashSet<int> { categoryId };

            while (currentParentId.HasValue)
            {
                if (visited.Contains(currentParentId.Value))
                    return true;

                visited.Add(currentParentId.Value);

                var parentCategory = await _context.Categories
                    .AsNoTracking()
                    .FirstOrDefaultAsync(c => c.Id == currentParentId.Value);

                if (parentCategory == null)
                    return false; // Parent doesn't exist, not a circular reference (but will fail in validation)

                currentParentId = parentCategory.ParentCategoryId;
            }

            return false;
        }
    }
}

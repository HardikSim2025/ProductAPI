using ProductAPI.Data;
using ProductAPI.Models;

namespace ProductAPI.Services
{
    public interface IProductService
    {
        Task<Product> CreateProductAsync(Product product);
        Task<List<Product>> GetAllProductsAsync();
    }

    public class ProductService : IProductService
    {
        private readonly AppDbContext _context;

        public ProductService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<Product> CreateProductAsync(Product product)
        {
            _context.Products.Add(product);
            await _context.SaveChangesAsync();
            return product;
        }

        public async Task<List<Product>> GetAllProductsAsync()
        {
            return await Task.FromResult(_context.Products.ToList());
        }
    }
}

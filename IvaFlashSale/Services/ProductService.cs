using IvaFlashSaleEngine.Data;
using IvaFlashSaleEngine.Models;
using IvaFlashSaleEngine.Exceptions;
using Microsoft.EntityFrameworkCore;
using System.Net;

namespace IvaFlashSaleEngine.Services
{
    public class ProductService : IProductService
    {
        private readonly AppDbContext _context;
        private readonly ILogger<ProductService> _logger;

        public ProductService(AppDbContext context, ILogger<ProductService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<IEnumerable<Product>> GetAllActiveProductsAsync()
        {
            return await _context.Products
                .Where(p => p.IsActive)
                .AsNoTracking() // Performance boost for read-only lists
                .ToListAsync();
        }

        public async Task<Product?> GetProductByIdAsync(int id)
        {
            var product = await _context.Products
                .FirstOrDefaultAsync(p => p.Id == id && p.IsActive);

            if (product == null)
            {
                throw new ServiceException($"Product with ID {id} not found.", "PRODUCT_NOT_FOUND", HttpStatusCode.NotFound);
            }
            return product;
        }

        public async Task<Product> CreateProductAsync(Product product)
        {
            if (product.Price <= 0)
                throw new ServiceException("Product price must be greater than zero.", "INVALID_PRODUCT_DATA");

            _context.Products.Add(product);
            await _context.SaveChangesAsync();
            return product;
        }
        
        public async Task<IEnumerable<Product>> CreateProductsBulkAsync(List<Product> products)
        {
            if (products == null || !products.Any())
            {
                throw new ServiceException("Product list cannot be empty.", "PRODUCT_BULK_EMPTY", HttpStatusCode.BadRequest);
            }

            // Validation loop before we touch the DB
            foreach (var product in products)
            {
                if (product.Price <= 0)
                    throw new ServiceException($"Product '{product.Name}' has an invalid price.", "INVALID_PRODUCT_DATA");
            }

            await _context.Products.AddRangeAsync(products);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Bulk insertion successful. {Count} products added.", products.Count);
            return products;
        }

        public async Task<bool> UpdateProductAsync(Product product)
        {
            _context.Entry(product).State = EntityState.Modified;
            try
            {
                await _context.SaveChangesAsync();
                return true;
            }
            catch (DbUpdateConcurrencyException)
            {
                throw new ServiceException("The product was modified by another admin. Please refresh.", "PRODUCT_CONCURRENCY_ERROR", HttpStatusCode.Conflict);
            }
        }

        public async Task<bool> SoftDeleteProductAsync(int id)
        {
            var product = await _context.Products.FindAsync(id);
            if (product == null)
                throw new ServiceException("Cannot delete: Product not found.", "PRODUCT_DELETE_FAILED", HttpStatusCode.NotFound);

            product.IsActive = false;
            await _context.SaveChangesAsync();
            return true;
        }
    }
}
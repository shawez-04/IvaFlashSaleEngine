using IvaFlashSaleEngine.Data;
using IvaFlashSaleEngine.DTOs;
using IvaFlashSaleEngine.Exceptions;
using IvaFlashSaleEngine.Models;
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

        public async Task<IEnumerable<ProductResponse>> GetAllActiveProductsAsync()
        {
            return await _context.Products
                .Where(p => p.IsActive)
                .AsNoTracking()
                .Select(p => MapToResponse(p))
                .ToListAsync();
        }

        public async Task<ProductResponse?> GetProductByIdAsync(int id)
        {
            var product = await _context.Products
                .FirstOrDefaultAsync(p => p.Id == id && p.IsActive);

            return product == null ? null : MapToResponse(product);
        }

        public async Task<ProductResponse> CreateProductAsync(ProductUpsertRequest request)
        {
            if (request.Price <= 0)
                throw new ServiceException("Price must be positive.", "INVALID_PRICE");

            var product = new Product
            {
                Name = request.Name,
                Description = request.Description,
                ImageUrl = request.ImageUrl,
                Price = request.Price,
                StockCount = request.StockCount,
                IsActive = true
            };

            _context.Products.Add(product);
            await _context.SaveChangesAsync();
            return MapToResponse(product);
        }

        public async Task<IEnumerable<ProductResponse>> CreateProductsBulkAsync(List<ProductUpsertRequest> requests)
        {
            if (requests == null || !requests.Any())
                throw new ServiceException("List cannot be empty.", "BULK_EMPTY", HttpStatusCode.BadRequest);

            var products = requests.Select(r => new Product
            {
                Name = r.Name,
                Description = r.Description,
                ImageUrl = r.ImageUrl,
                Price = r.Price,
                StockCount = r.StockCount,
                IsActive = true
            }).ToList();

            await _context.Products.AddRangeAsync(products);
            await _context.SaveChangesAsync();
            return products.Select(MapToResponse);
        }

        public async Task<bool> UpdateProductAsync(int id, ProductUpsertRequest request, uint rowVersion)
        {
            var product = await _context.Products.FindAsync(id);
            if (product == null) return false;

            // Update only allowed fields
            product.Name = request.Name;
            product.Description = request.Description;
            product.ImageUrl = request.ImageUrl;
            product.Price = request.Price;
            product.StockCount = request.StockCount;

            // Set original RowVersion to trigger EF Core Concurrency Check
            _context.Entry(product).Property(p => p.RowVersion).OriginalValue = rowVersion;

            try
            {
                await _context.SaveChangesAsync();
                return true;
            }
            catch (DbUpdateConcurrencyException)
            {
                throw new ServiceException("Concurrency conflict: Product was modified elsewhere.", "CONFLICT", HttpStatusCode.Conflict);
            }
        }

        public async Task<bool> SoftDeleteProductAsync(int id)
        {
            var product = await _context.Products.FindAsync(id);
            if (product == null) return false;

            product.IsActive = false;
            await _context.SaveChangesAsync();
            return true;
        }

        private static ProductResponse MapToResponse(Product p) => new ProductResponse
        {
            Id = p.Id,
            Name = p.Name,
            Description = p.Description,
            ImageUrl = p.ImageUrl,
            Price = p.Price,
            StockCount = p.StockCount,
            RowVersion = p.RowVersion
        };
    }
}
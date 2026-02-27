using IvaFlashSaleEngine.Data;
using IvaFlashSaleEngine.DTOs;
using IvaFlashSaleEngine.Exceptions;
using IvaFlashSaleEngine.Models;
using IvaFlashSaleEngine.Services;
using Microsoft.EntityFrameworkCore;
using System.Net;

namespace IvaFlashSaleEngine.Services
{
    public class PurchaseService : IPurchaseService
    {
        private readonly AppDbContext _context;
        private readonly ILogger<PurchaseService> _logger;

        public PurchaseService(AppDbContext context, ILogger<PurchaseService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<bool> ProcessPurchaseAsync(PurchaseRequest request)
        {
            _logger.LogInformation("Purchase attempt: Product {ProductId} by User {UserId}", request.ProductId, request.UserId);

            using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                var product = await _context.Products.FirstOrDefaultAsync(p => p.Id == request.ProductId);

                if (product == null)
                    throw new ServiceException("Product no longer exists.", "PURCHASE_PRODUCT_NOT_FOUND", HttpStatusCode.NotFound);

                if (!product.IsActive)
                    throw new ServiceException("This product is currently unavailable.", "PURCHASE_PRODUCT_INACTIVE");

                if (product.StockCount <= 0)
                {
                    _logger.LogWarning("Stock-out: Product {ProductId}", request.ProductId);
                    throw new ServiceException("Item is sold out!", "PURCHASE_OUT_OF_STOCK", HttpStatusCode.Gone);
                }

                product.StockCount -= 1;
                _context.Orders.Add(new Order
                {
                    ProductId = product.Id,
                    UserId = request.UserId,
                    OrderDate = DateTime.UtcNow
                });

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                return true;
            }
            catch (ServiceException) { throw; } // Re-throw business exceptions
            catch (DbUpdateConcurrencyException)
            {
                await transaction.RollbackAsync();
                // This is caught by middleware and returned as 409 Conflict
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogCritical(ex, "Purchase System Failure for User {UserId}", request.UserId);
                await transaction.RollbackAsync();
                throw new ServiceException("A critical error occurred during your purchase.", "SYSTEM_PURCHASE_FAILURE", HttpStatusCode.InternalServerError);
            }
        }
    }
}
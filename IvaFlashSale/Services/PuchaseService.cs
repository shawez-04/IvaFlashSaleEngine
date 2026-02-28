using IvaFlashSaleEngine.Data;
using IvaFlashSaleEngine.DTOs;
using IvaFlashSaleEngine.Exceptions;
using IvaFlashSaleEngine.Models;
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

        public async Task<bool> ProcessPurchaseAsync(PurchaseRequest request, string userId, string idempotencyKey)
        {
            _logger.LogInformation("Processing purchase: Product {ProductId}, User {UserId}, Key {IdempotencyKey}",
                request.ProductId, userId, idempotencyKey);

            // Idempotency Check: Prevent duplicate orders within a short window
            var existingOrder = await _context.Orders
                .AnyAsync(o => o.IdempotencyKey == idempotencyKey);

            if (existingOrder)
            {
                _logger.LogWarning("Duplicate request blocked: IdempotencyKey {Key}", idempotencyKey);
                return true; // Return true as we've already processed this "intent"
            }

            using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                // Fetch product with xmin tracking
                var product = await _context.Products.FirstOrDefaultAsync(p => p.Id == request.ProductId);

                if (product == null)
                    throw new ServiceException("Product not found.", "PURCHASE_NOT_FOUND", HttpStatusCode.NotFound);

                if (!product.IsActive)
                    throw new ServiceException("Product is inactive.", "PURCHASE_INACTIVE");

                // Robust Stock Check
                if (product.StockCount < request.Quantity)
                {
                    _logger.LogWarning("Insufficient stock for Product {Id}. Requested: {Req}, Available: {Avail}",
                        product.Id, request.Quantity, product.StockCount);
                    throw new ServiceException("Insufficient stock available.", "PURCHASE_OUT_OF_STOCK", HttpStatusCode.Conflict);
                }

                // Atomic Update
                product.StockCount -= request.Quantity;

                _context.Orders.Add(new Order
                {
                    ProductId = product.Id,
                    UserId = userId,
                    Quantity = request.Quantity,
                    TotalPrice = product.Price * request.Quantity,
                    OrderDate = DateTime.UtcNow,
                    IdempotencyKey = idempotencyKey // Saved to prevent replays
                });

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                _logger.LogInformation("Purchase successful: Order created for User {UserId}", userId);
                return true;
            }
            catch (DbUpdateConcurrencyException)
            {
                await transaction.RollbackAsync();
                _logger.LogWarning("Concurrency Conflict: Product {Id} updated by another process.", request.ProductId);
                throw; // Caught by Global Exception Middleware to return 409
            }
            catch (Exception ex) when (ex is not ServiceException)
            {
                await transaction.RollbackAsync();
                _logger.LogCritical(ex, "Transaction failed for User {UserId}", userId);
                throw new ServiceException("Transaction failed.", "SYSTEM_ERROR", HttpStatusCode.InternalServerError);
            }
        }
    }
}
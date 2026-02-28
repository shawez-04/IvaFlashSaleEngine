using IvaFlashSaleEngine.Data;
using IvaFlashSaleEngine.DTOs;
using IvaFlashSaleEngine.Exceptions;
using IvaFlashSaleEngine.Models;
using IvaFlashSaleEngine.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace IvaFlashSaleEngine.Tests
{
    public class PurchaseServiceTests : IDisposable
    {
        private readonly AppDbContext _context;
        private readonly Mock<ILogger<PurchaseService>> _loggerMock;
        private readonly PurchaseService _purchaseService;

        public PurchaseServiceTests()
        {
            // IMPORTANT: .ConfigureWarnings allows the test to bypass the "Transactions not supported" error
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .ConfigureWarnings(x => x.Ignore(InMemoryEventId.TransactionIgnoredWarning))
                .Options;

            _context = new AppDbContext(options);
            _loggerMock = new Mock<ILogger<PurchaseService>>();
            _purchaseService = new PurchaseService(_context, _loggerMock.Object);
        }

        [Fact]
        public async Task ProcessPurchase_Success_DeductsStockAndCreatesOrder()
        {
            // Arrange
            var product = new Product
            {
                Id = 1,
                Name = "Sneakers",
                Price = 100,
                StockCount = 10,
                IsActive = true
            };
            _context.Products.Add(product);
            await _context.SaveChangesAsync();

            var request = new PurchaseRequest { ProductId = 1, Quantity = 2 };

            // Act
            var result = await _purchaseService.ProcessPurchaseAsync(request, "user-001", "key-unique-1");

            // Assert
            Assert.True(result);
            var dbProduct = await _context.Products.FindAsync(1);
            Assert.Equal(8, dbProduct!.StockCount);
            Assert.True(await _context.Orders.AnyAsync(o => o.IdempotencyKey == "key-unique-1"));
        }

        [Fact]
        public async Task ProcessPurchase_DuplicateKey_ReturnsTrueWithoutDeductingAgain()
        {
            // Arrange
            var product = new Product { Id = 2, Name = "Laptop", Price = 500, StockCount = 5, IsActive = true };
            _context.Products.Add(product);

            var existingOrder = new Order
            {
                IdempotencyKey = "key-repeat",
                ProductId = 2,
                UserId = "user-001",
                Quantity = 1
            };
            _context.Orders.Add(existingOrder);
            await _context.SaveChangesAsync();

            var request = new PurchaseRequest { ProductId = 2, Quantity = 1 };

            // Act
            var result = await _purchaseService.ProcessPurchaseAsync(request, "user-001", "key-repeat");

            // Assert
            Assert.True(result);
            var dbProduct = await _context.Products.FindAsync(2);
            Assert.Equal(5, dbProduct!.StockCount); // Should still be 5, not 4
        }

        [Fact]
        public async Task ProcessPurchase_OutOfStock_ThrowsServiceException()
        {
            // Arrange
            var product = new Product { Id = 3, Name = "Rare Card", Price = 10, StockCount = 1, IsActive = true };
            _context.Products.Add(product);
            await _context.SaveChangesAsync();

            var request = new PurchaseRequest { ProductId = 3, Quantity = 5 };

            // Act & Assert
            var ex = await Assert.ThrowsAsync<ServiceException>(() =>
                _purchaseService.ProcessPurchaseAsync(request, "user-001", "key-fail"));

            Assert.Equal("PURCHASE_OUT_OF_STOCK", ex.ErrorCode);
        }

        public void Dispose()
        {
            _context.Database.EnsureDeleted();
            _context.Dispose();
        }
    }
}
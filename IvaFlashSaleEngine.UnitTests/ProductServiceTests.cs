using IvaFlashSaleEngine.Data;
using IvaFlashSaleEngine.DTOs;
using IvaFlashSaleEngine.Exceptions;
using IvaFlashSaleEngine.Models;
using IvaFlashSaleEngine.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace IvaFlashSaleEngine.Tests
{
    public class ProductServiceTests : IDisposable
    {
        private readonly AppDbContext _context;
        private readonly Mock<ILogger<ProductService>> _loggerMock;
        private readonly ProductService _productService;

        public ProductServiceTests()
        {
            // Setup In-Memory DB
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            _context = new AppDbContext(options);
            _loggerMock = new Mock<ILogger<ProductService>>();
            _productService = new ProductService(_context, _loggerMock.Object);
        }

        [Fact]
        public async Task CreateProductAsync_ShouldSaveProduct_WhenPriceIsPositive()
        {
            // Arrange
            var request = new ProductUpsertRequest
            {
                Name = "Sneakers",
                Price = 99.99m,
                StockCount = 10
            };

            // Act
            var result = await _productService.CreateProductAsync(request);

            // Assert
            Assert.NotNull(result);
            Assert.Equal("Sneakers", result.Name);
            Assert.True(result.Id > 0);
        }

        [Fact]
        public async Task CreateProductAsync_ShouldThrowException_WhenPriceIsNegative()
        {
            // Arrange
            var request = new ProductUpsertRequest { Name = "Bad Price", Price = -10m };

            // Act & Assert
            var ex = await Assert.ThrowsAsync<ServiceException>(() =>
                _productService.CreateProductAsync(request));

            Assert.Equal("INVALID_PRICE", ex.ErrorCode);
        }

        [Fact]
        public async Task CreateProductsBulkAsync_ShouldInsertAllItems()
        {
            // Arrange
            var requests = new List<ProductUpsertRequest>
            {
                new() { Name = "Item 1", Price = 10, StockCount = 5 },
                new() { Name = "Item 2", Price = 20, StockCount = 5 }
            };

            // Act
            var results = await _productService.CreateProductsBulkAsync(requests);

            // Assert
            Assert.Equal(2, results.Count());
            Assert.Equal(2, await _context.Products.CountAsync());
        }

        [Fact]
        public async Task SoftDeleteProductAsync_ShouldSetIsActiveToFalse()
        {
            // Arrange
            var product = new Product { Name = "Delete Me", IsActive = true, Price = 10 };
            _context.Products.Add(product);
            await _context.SaveChangesAsync();

            // Act
            var success = await _productService.SoftDeleteProductAsync(product.Id);

            // Assert
            Assert.True(success);
            var dbProduct = await _context.Products.FindAsync(product.Id);
            Assert.False(dbProduct!.IsActive);
        }

        [Fact]
        public async Task UpdateProductAsync_ShouldUpdateFields()
        {
            // Arrange
            var product = new Product { Name = "Old Name", Price = 10, IsActive = true };
            _context.Products.Add(product);
            await _context.SaveChangesAsync();

            var updateRequest = new ProductUpsertRequest { Name = "New Name", Price = 50, StockCount = 100 };

            // Act
            await _productService.UpdateProductAsync(product.Id, updateRequest, product.RowVersion);

            // Assert
            var updated = await _context.Products.FindAsync(product.Id);
            Assert.Equal("New Name", updated!.Name);
            Assert.Equal(50, updated.Price);
        }

        public void Dispose()
        {
            _context.Database.EnsureDeleted();
            _context.Dispose();
        }
    }
}
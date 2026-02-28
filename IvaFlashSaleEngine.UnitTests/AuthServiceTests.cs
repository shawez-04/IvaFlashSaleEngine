using IvaFlashSaleEngine.Data;
using IvaFlashSaleEngine.Exceptions;
using IvaFlashSaleEngine.Models;
using IvaFlashSaleEngine.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace IvaFlashSaleEngine.Tests
{
    public class AuthServiceTests
    {
        private readonly AppDbContext _context;
        private readonly Mock<ILogger<AuthService>> _loggerMock;
        private readonly AuthService _authService;

        public AuthServiceTests()
        {
            // Setup In-Memory Database for isolation
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            _context = new AppDbContext(options);
            _loggerMock = new Mock<ILogger<AuthService>>();
            _authService = new AuthService(_context, _loggerMock.Object);
        }

        [Fact]
        public async Task RegisterAsync_ShouldCreateUser_WhenDataIsValid()
        {
            // Arrange
            string username = "testuser";
            string password = "Password123";

            // Act
            var result = await _authService.RegisterAsync(username, password);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(username, result.Username);
            Assert.True(BCrypt.Net.BCrypt.Verify(password, result.PasswordHash));
        }

        [Fact]
        public async Task RegisterAsync_ShouldThrowConflict_WhenUserAlreadyExists()
        {
            // Arrange
            await _authService.RegisterAsync("duplicate", "pass");

            // Act & Assert
            var ex = await Assert.ThrowsAsync<ServiceException>(() =>
                _authService.RegisterAsync("duplicate", "newpass"));

            Assert.Equal("AUTH_DUPLICATE_USERNAME", ex.ErrorCode);
        }

        [Fact]
        public async Task AuthenticateAsync_ShouldReturnUser_WhenCredentialsAreCorrect()
        {
            // Arrange
            string user = "loginuser";
            string pass = "correctpass";
            await _authService.RegisterAsync(user, pass);

            // Act
            var authenticatedUser = await _authService.AuthenticateAsync(user, pass);

            // Assert
            Assert.NotNull(authenticatedUser);
            Assert.Equal(user, authenticatedUser.Username);
        }

        [Fact]
        public async Task AuthenticateAsync_ShouldThrowUnauthorized_WhenPasswordIsWrong()
        {
            // Arrange
            await _authService.RegisterAsync("wrongpassuser", "correct");

            // Act & Assert
            await Assert.ThrowsAsync<ServiceException>(() =>
                _authService.AuthenticateAsync("wrongpassuser", "wrong"));
        }
    }
}
using IvaFlashSaleEngine.Data;
using IvaFlashSaleEngine.Exceptions;
using IvaFlashSaleEngine.Models;
using Microsoft.EntityFrameworkCore;
using System.Net;

namespace IvaFlashSaleEngine.Services
{
    public class AuthService : IAuthService
    {
        private readonly AppDbContext _context;

        private readonly ILogger _logger;

        public AuthService(AppDbContext context, ILogger<AuthService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<User?> AuthenticateAsync(string username, string password)
        {
            var user = await _context.Users.SingleOrDefaultAsync(u => u.Username == username);

            if (user == null || !BCrypt.Net.BCrypt.Verify(password, user.PasswordHash))
            {
                _logger.LogWarning("Failed login attempt for {Username}", username);
                throw new ServiceException("Invalid username or password.", "AUTH_INVALID_CREDENTIALS", HttpStatusCode.Unauthorized);
            }

            return user;
        }

        public async Task<User> RegisterAsync(string username, string password, UserRole role = UserRole.User)
        {
            // Force standard User role for public signups if the method is called without a specific role
            // This is a "Fail-Safe" approach.

            var existingUser = await _context.Users.AnyAsync(u => u.Username.ToLower() == username.ToLower());
            if (existingUser)
            {
                throw new ServiceException("Username is already taken.", "AUTH_DUPLICATE_USERNAME", HttpStatusCode.Conflict);
            }

            var user = new User
            {
                Username = username,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(password),
                Role = role // This is now safe because we control who calls this method with 'Admin'
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();
            return user;
        }
    }
}
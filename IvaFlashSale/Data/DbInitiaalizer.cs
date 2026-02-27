using IvaFlashSaleEngine.Models;
using Microsoft.EntityFrameworkCore;

namespace IvaFlashSaleEngine.Data
{
    public static class DbInitializer
    {
        public static async Task SeedAdminAsync(IServiceProvider serviceProvider, IConfiguration configuration)
        {
            using var scope = serviceProvider.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();

            // 1. Ensure the database exists
            await context.Database.EnsureCreatedAsync();

            // 2. Read Admin info from AppSettings
            var adminConfig = configuration.GetSection("InitialAdmin").Get<AdminSeedConfig>();

            if (adminConfig != null && !await context.Users.AnyAsync(u => u.Role == UserRole.Admin))
            {
                var adminUser = new User
                {
                    Username = adminConfig.Username,
                    // Hash the password from appsettings using BCrypt
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword(adminConfig.Password),
                    Role = UserRole.Admin
                };

                context.Users.Add(adminUser);
                await context.SaveChangesAsync();
            }
        }
    }
}
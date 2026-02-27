using IvaFlashSaleEngine.Models;
using Microsoft.EntityFrameworkCore;

namespace IvaFlashSaleEngine.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<Product> Products => Set<Product>();
        public DbSet<Order> Orders => Set<Order>();
        public DbSet<User> Users => Set<User>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            //  Fix the Decimal Warning
            // Configures 'Price' to have a maximum of 18 digits with 2 after the decimal point
            modelBuilder.Entity<Product>()
                .Property(p => p.Price)
                .HasPrecision(18, 2);

            // Seed a product for testing
            modelBuilder.Entity<Product>().HasData
                (new Product
                {
                    Id = 1,
                    Name = "Limited Edition Sneakers",
                    Description = "",
                    ImageUrl = "",
                    Price = 99.99m,
                    StockCount = 10,
                    IsActive = true
                }
                );
        }
    }
}
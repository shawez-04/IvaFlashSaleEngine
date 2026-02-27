using IvaFlashSaleEngine.Models;
using Microsoft.EntityFrameworkCore;
using System.Reflection.Emit;

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

            // Seed a product for testing
            modelBuilder.Entity<Product>().HasData(
                new Product { Id = 1, Name = "Limited Edition Sneakers", Price = 99.99m, StockCount = 10 }
            );
        }
    }
}
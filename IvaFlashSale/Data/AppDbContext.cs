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

            // Decimal Precision Fix
            modelBuilder.Entity<Product>()
                .Property(p => p.Price)
                .HasPrecision(18, 2);

            // Index the IdempotencyKey for lightning-fast lookups
            // This also prevents two rows with the same key from ever being inserted
            modelBuilder.Entity<Order>()
                .HasIndex(o => o.IdempotencyKey)
                .IsUnique();

            // PostgreSQL Concurrency Mapping
            // This tells EF Core to use the hidden 'xmin' column in Postgres
            modelBuilder.Entity<Product>()
                .Property(p => p.RowVersion)
                .IsRowVersion();

            // Seed Data (RowVersion should be omitted here; DB handles it)
            modelBuilder.Entity<Product>().HasData(new Product
            {
                Id = 1,
                Name = "Limited Edition Sneakers",
                Description = "High-performance flash sale item",
                ImageUrl = "https://example.com/sneakers.jpg",
                Price = 99.99m,
                StockCount = 10,
                IsActive = true
            });
        }
    }
}
using System.ComponentModel.DataAnnotations;

namespace IvaFlashSaleEngine.Models
{
    public class Product
    {
        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        public string Name { get; set; } = string.Empty;

        public string Description { get; set; } = string.Empty;

        public string ImageUrl { get; set; } = string.Empty;

        public decimal Price { get; set; }

        public int StockCount { get; set; }

        public bool IsActive { get; set; } = true;

        // Optimized for PostgreSQL Concurrency
        [Timestamp]
        public uint RowVersion { get; set; }
    }
}
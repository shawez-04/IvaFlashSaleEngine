using System.ComponentModel.DataAnnotations;
namespace IvaFlashSaleEngine.DTOs 
{
    public class ProductUpsertRequest
    {
        [Required]
        [StringLength(100)]
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string ImageUrl { get; set; } = string.Empty;
        [Range(0.01, 1000000)]
        public decimal Price { get; set; }
        [Range(0, 10000)]
        public int StockCount { get; set; }
    }
}
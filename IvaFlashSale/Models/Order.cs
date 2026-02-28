using System.ComponentModel.DataAnnotations;

namespace IvaFlashSaleEngine.Models
{
    public class Order
    {
        public int Id { get; set; }

        public int ProductId { get; set; }

        public string UserId { get; set; } = string.Empty;

        // Track exactly how many items were bought in this specific order
        public int Quantity { get; set; }

        // The exact price at the time of purchase (in case product price changes later)
        public decimal TotalPrice { get; set; }

        public DateTime OrderDate { get; set; }

        // The critical "Flash Sale" protection key
        [Required]
        [StringLength(100)]
        public string IdempotencyKey { get; set; } = string.Empty;
    }
}
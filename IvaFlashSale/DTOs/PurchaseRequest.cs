namespace IvaFlashSaleEngine.DTOs
{
    public class PurchaseRequest
    {
        public int ProductId { get; set; }
        public int Quantity { get; set; } = 1; // Default to 1 for flash sales
    }
}
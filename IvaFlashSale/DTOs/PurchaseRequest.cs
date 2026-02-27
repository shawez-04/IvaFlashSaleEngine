namespace IvaFlashSaleEngine.DTOs
{
    public class PurchaseRequest
    {
        // The ID of the product the user wants to buy
        public int ProductId { get; set; }

        // A simple identifier for the user (e.g., "User_123")
        public string UserId { get; set; } = string.Empty;
    }
}
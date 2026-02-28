namespace IvaFlashSaleEngine.DTOs
{
    public class ProductResponse
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string ImageUrl { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public int StockCount { get; set; }
        public uint RowVersion { get; set; } // Essential for admin updates to handle concurrency
    }
}

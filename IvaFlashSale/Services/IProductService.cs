using IvaFlashSaleEngine.Models;

namespace IvaFlashSaleEngine.Services
{
    public interface IProductService
    {
        Task<IEnumerable<Product>> GetAllActiveProductsAsync();
        Task<Product?> GetProductByIdAsync(int id);
        Task<Product> CreateProductAsync(Product product);
        Task<IEnumerable<Product>> CreateProductsBulkAsync(List<Product> products);
        Task<bool> UpdateProductAsync(Product product);
        Task<bool> SoftDeleteProductAsync(int id); // The "Pro" way to delete
    }
}
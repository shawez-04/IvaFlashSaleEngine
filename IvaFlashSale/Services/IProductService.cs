using IvaFlashSaleEngine.DTOs;

namespace IvaFlashSaleEngine.Services
{
    public interface IProductService
    {
        Task<IEnumerable<ProductResponse>> GetAllActiveProductsAsync();
        Task<ProductResponse?> GetProductByIdAsync(int id);
        Task<ProductResponse> CreateProductAsync(ProductUpsertRequest request);
        Task<IEnumerable<ProductResponse>> CreateProductsBulkAsync(List<ProductUpsertRequest> requests);
        Task<bool> UpdateProductAsync(int id, ProductUpsertRequest request, uint rowVersion);
        Task<bool> SoftDeleteProductAsync(int id);
    }
}
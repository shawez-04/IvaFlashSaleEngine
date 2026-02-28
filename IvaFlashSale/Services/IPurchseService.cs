using IvaFlashSaleEngine.DTOs;

namespace IvaFlashSaleEngine.Services
{
    public interface IPurchaseService
    {
        Task<bool> ProcessPurchaseAsync(PurchaseRequest request, string userId, string idempotencyKey);
    }
}
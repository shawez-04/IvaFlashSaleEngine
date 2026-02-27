using IvaFlashSaleEngine.Models;

namespace IvaFlashSaleEngine.Services
{
    public interface IAuthService
    {
        Task<User?> AuthenticateAsync(string username, string password);
        Task<User> RegisterAsync(string username, string password, UserRole role = UserRole.User);
    }
}
using IvaFlashSaleEngine.Models;

namespace IvaFlashSaleEngine.DTOs
{
    public class RegisterRequest
    {
        public required string Username { get; set; }
        public required string Password { get; set; }
        public UserRole Role { get; set; } = UserRole.User;
    }
}
using IvaFlashSaleEngine.DTOs;
using IvaFlashSaleEngine.Models;
using IvaFlashSaleEngine.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace IvaFlashSaleEngine.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;
        private readonly IConfiguration _config;

        public AuthController(IAuthService authService, IConfiguration config)
        {
            _authService = authService;
            _config = config;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterRequest request)
        {
                var user = await _authService.RegisterAsync(request.Username, request.Password, request.Role);
                return Ok(new { message = "User registered successfully", username = user.Username });            
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {
            var user = await _authService.AuthenticateAsync(request.Username, request.Password);

            if (user == null) return Unauthorized(new { message = "Invalid username or password" });

            var token = GenerateJwtToken(user.Username, user.Role.ToString());
            return Ok(new { token });
        }

        [ApiController]
        [Route("api/admin/manage")]
        [Authorize(Roles = "Admin")]
        public class AdminManagementController : ControllerBase
        {
            private readonly IAuthService _authService;

            public AdminManagementController(IAuthService authService)
            {
                _authService = authService;
            }

            [HttpPost("create-admin")]
            public async Task<IActionResult> CreateNewAdmin([FromBody] RegisterRequest request)
            {
                // Because this endpoint has [Authorize(Roles = "Admin")], 
                // a standard user cannot create an admin.
                var newUser = await _authService.RegisterAsync(request.Username, request.Password, UserRole.Admin);
                return Ok(new { message = "New Admin created successfully", username = newUser.Username });
            }
        }

        private string GenerateJwtToken(string username, string role)
        {
            var jwtKey = _config["Jwt:Key"];

            if (string.IsNullOrEmpty(jwtKey))
            {
                throw new InvalidOperationException("JWT Key is not configured in appsettings.json.");
            }

            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey));
            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256); 
            var claims = new[]
            {
                new Claim(ClaimTypes.Name, username),
                // .ToString() ensures "Admin" is put into the token as a string
                new Claim(ClaimTypes.Role, role.ToString()),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
            };

            var token = new JwtSecurityToken(
                issuer: _config["Jwt:Issuer"],
                audience: _config["Jwt:Audience"],
                claims: claims,
                expires: DateTime.Now.AddHours(3),
                signingCredentials: credentials);

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}
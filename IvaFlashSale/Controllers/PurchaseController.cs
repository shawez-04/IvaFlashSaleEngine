using IvaFlashSaleEngine.DTOs;
using IvaFlashSaleEngine.Infrastructure;
using IvaFlashSaleEngine.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace IvaFlashSaleEngine.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class PurchaseController : ControllerBase
    {
        private readonly IPurchaseService _purchaseService;
        private readonly ILogger<PurchaseController> _logger;

        public PurchaseController(IPurchaseService purchaseService, ILogger<PurchaseController> logger)
        {
            _purchaseService = purchaseService;
            _logger = logger;
        }

        [HttpPost]
        [RequiresIdempotency]
        public async Task<IActionResult> Buy([FromBody] PurchaseRequest request)
        {
            // Extract Identity from JWT
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId)) return Unauthorized();

            // Extract Idempotency Key from Headers
            if (!Request.Headers.TryGetValue("X-Idempotency-Key", out var idempotencyKey) || string.IsNullOrEmpty(idempotencyKey))
            {
                return BadRequest(new { message = "X-Idempotency-Key header is required for flash sales." });
            }

            // Execute
            var result = await _purchaseService.ProcessPurchaseAsync(request, userId, idempotencyKey!);

            return Ok(new { message = "Order placed successfully!" });
        }
    }
}
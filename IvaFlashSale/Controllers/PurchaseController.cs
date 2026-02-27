using IvaFlashSaleEngine.DTOs;
using IvaFlashSaleEngine.Services;
using Microsoft.AspNetCore.Mvc;

namespace IvaFlashSaleEngine.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PurchaseController : ControllerBase
    {
        private readonly IPurchaseService _purchaseService;
        private readonly ILogger<PurchaseService> _logger;


        public PurchaseController(IPurchaseService purchaseService , ILogger<PurchaseService> logger)
        {
            _purchaseService = purchaseService;
            _logger = logger;
        }

        [HttpPost]
        public async Task<IActionResult> Buy([FromBody] PurchaseRequest request)
        {
            var result = await _purchaseService.ProcessPurchaseAsync(request);

            if (!result)
            {
                _logger.LogWarning("Inventory depletion: Product {ProductId} sold out.", request.ProductId);
                return BadRequest(new { message = "Item sold out." });
            }

            return Ok(new { message = "Success!" });
        }
    }
}
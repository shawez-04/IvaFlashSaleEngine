using IvaFlashSaleEngine.DTOs;
using IvaFlashSaleEngine.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/[controller]")]
public class ProductsController : ControllerBase
{
    private readonly IProductService _productService;

    public ProductsController(IProductService productService) => _productService = productService;

    [HttpGet]
    [AllowAnonymous]
    public async Task<IActionResult> GetProducts() => Ok(await _productService.GetAllActiveProductsAsync());

    [HttpGet("{id}")]
    [AllowAnonymous]
    public async Task<IActionResult> GetProduct(int id)
    {
        var result = await _productService.GetProductByIdAsync(id);
        return result == null ? NotFound() : Ok(result);
    }

    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> CreateProduct([FromBody] ProductUpsertRequest request)
    {
        var result = await _productService.CreateProductAsync(request);
        return CreatedAtAction(nameof(GetProduct), new { id = result.Id }, result);
    }

    [HttpPost("bulk")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> CreateBulk([FromBody] List<ProductUpsertRequest> requests)
    {
        var results = await _productService.CreateProductsBulkAsync(requests);
        return Ok(new { message = "Bulk addition successful.", data = results });
    }

    [HttpPut("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> UpdateProduct(int id, [FromBody] ProductUpsertRequest request, [FromHeader(Name = "If-Match")] uint rowVersion)
    {
        var success = await _productService.UpdateProductAsync(id, request, rowVersion);
        return success ? NoContent() : NotFound();
    }

    [HttpDelete("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> DeleteProduct(int id)
    {
        var success = await _productService.SoftDeleteProductAsync(id);
        return success ? Ok(new { message = "Soft deleted successfully." }) : NotFound();
    }
}
using IvaFlashSaleEngine.Models;
using IvaFlashSaleEngine.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace IvaFlashSaleEngine.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ProductsController : ControllerBase
    {
        private readonly IProductService _productService;

        public ProductsController(IProductService productService)
        {
            _productService = productService;
        }

        [HttpGet]
        [AllowAnonymous]
        public async Task<ActionResult<IEnumerable<Product>>> GetProducts()
        {
            var products = await _productService.GetAllActiveProductsAsync();
            return Ok(products);
        }

        [HttpGet("{id}")]
        [AllowAnonymous]
        public async Task<ActionResult<Product>> GetProduct(int id)
        {
            var product = await _productService.GetProductByIdAsync(id);
            if (product == null) return NotFound(new { message = "Product not found or inactive." });

            return Ok(product);
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<Product>> CreateProduct(Product product)
        {
            // Always validate incoming data
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var createdProduct = await _productService.CreateProductAsync(product);

            // Returns a 201 Created status with the location of the new resource
            return CreatedAtAction(nameof(GetProduct), new { id = createdProduct.Id }, createdProduct);
        }

        [Authorize(Roles = "Admin")]
        [HttpPost("bulk")]
        public async Task<IActionResult> CreateBulk([FromBody] List<Product> products)
        {
            // The ProductService now handles the logic and throws ServiceExceptions
            var createdProducts = await _productService.CreateProductsBulkAsync(products);

            return Ok(new
            {
                message = $"{createdProducts.Count()} products added to the engine.",
                data = createdProducts
            });
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> UpdateProduct(int id, Product product)
        {
            if (id != product.Id) return BadRequest(new { message = "ID mismatch." });

            var result = await _productService.UpdateProductAsync(product);
            if (!result) return Conflict(new { message = "Concurrency conflict or product not found." });

            return NoContent();
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteProduct(int id)
        {
            var result = await _productService.SoftDeleteProductAsync(id);
            if (!result) return NotFound(new { message = "Product not found." });

            return Ok(new { message = "Product successfully deactivated (Soft Delete)." });
        }
    }
}

using StockTrackingAuthAPI.Models;
using StockTrackingAuthAPI.Services;
using Microsoft.AspNetCore.Mvc;
[ApiController]
[Route("api/[controller]")]
public class ScanController : ControllerBase
{
    private readonly ProductService _productService;

    public ScanController(ProductService productService)
    {
        _productService = productService;
    }

    // Get product info by QR code (productCode)
    [HttpGet("{productCode}")]
    public async Task<IActionResult> GetProductByCode(string productCode)
    {
        var product = await _productService.GetProductByCodeAsync(productCode);
        if (product == null) return NotFound("Product not found.");
        return Ok(product);
    }

    // Submit scan data (quantity entered)
    [HttpPost("submit")]
    public async Task<IActionResult> SubmitScan([FromBody] ScanDataModel data)
    {
        if (string.IsNullOrEmpty(data.ProductCode) || data.EnteredQty < 0)
            return BadRequest("Invalid scan data.");

        var result = await _productService.SaveScanDataAsync(data);
        return result ? Ok("Scan saved.") : StatusCode(500, "Failed to save scan.");
    }
}

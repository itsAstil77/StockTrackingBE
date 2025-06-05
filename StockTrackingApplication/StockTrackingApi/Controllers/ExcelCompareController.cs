using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

[ApiController]
[Route("api/[controller]")]
public class ExcelCompareController : ControllerBase
{
    private readonly ExcelCompareService _excelCompareService;

    public ExcelCompareController(ExcelCompareService excelCompareService)
    {
        _excelCompareService = excelCompareService;
    }

    [HttpPost("compare")]
public async Task<IActionResult> CompareExcelFiles([FromForm] CompareExcelRequest request)
{
    if (request.File1 == null || request.File2 == null)
        return BadRequest("Both files are required.");

    var comparison = await _excelCompareService.CompareExcelByProductCodeAsync(request.File1, request.File2);
    return Ok(comparison);
}

}

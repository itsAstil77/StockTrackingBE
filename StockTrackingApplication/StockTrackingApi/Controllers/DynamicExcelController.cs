
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using StockTrackingAuthAPI.Models;
using StockTrackingAuthAPI.Services;
using System.Threading.Tasks;




[ApiController]
[Route("api/[controller]")]
public class DynamicExportController : ControllerBase
{
    private readonly DynamicExcelExportService _exportService;

    public DynamicExportController(DynamicExcelExportService exportService)
    {
        _exportService = exportService;
    }

    [HttpPost("export-by-date")]
    public async Task<IActionResult> ExportByDate([FromBody] DateRangeRequest request)
    {
        var fileBytes = await _exportService.ExportAsync(request.StartDate, request.EndDate);

        if (fileBytes.Length == 0)
            return NotFound("No data found in the given date range.");

        return File(fileBytes,
            "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
            $"Export_{request.StartDate:yyyyMMdd}_{request.EndDate:yyyyMMdd}.xlsx");
    }
}

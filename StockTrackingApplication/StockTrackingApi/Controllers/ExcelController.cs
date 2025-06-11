using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using StockTrackingAuthAPI.Models;
using StockTrackingAuthAPI.Services;
using System;
using System.Threading.Tasks;

namespace StockTrackingAuthAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ExcelController : ControllerBase
    {
        private readonly ExcelService _excelService;

        public ExcelController(ExcelService excelService)
        {
            _excelService = excelService;
        }

        // Upload Excel file endpoint (existing)
        [HttpPost("upload")]
        [Consumes("multipart/form-data")]
        public async Task<IActionResult> UploadExcel([FromForm] ExcelUploadRequest request)
        {
            var (isSuccess, message, inserted, skipped) = await _excelService.ProcessExcelFileAsync(request.File);

            if (!isSuccess)
                return BadRequest(new { message });

            return Ok(new
            {
                message,
                inserted,
                skipped
            });
        }

        // New: Export data as Excel file by date range
        [HttpGet("export")]
        public async Task<IActionResult> ExportData([FromQuery] DateTime startDate, [FromQuery] DateTime endDate)
        {
            if (startDate > endDate)
                return BadRequest("startDate must be less than or equal to endDate.");

            var excelBytes = await _excelService.ExportDataToExcelAsync(startDate, endDate);

            // Return file as downloadable Excel (.xlsx)
            return File(excelBytes,
                        "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                        $"Export_{startDate:yyyyMMdd}_{endDate:yyyyMMdd}.xlsx");
        }
        
        [HttpGet("ItemMasterSummary")]
public async Task<IActionResult> GetAllExcelData()
{
    var data = await _excelService.GetAllDataAsync();
    return Ok(data);
}
[HttpPost("StockUpload")]
[Consumes("multipart/form-data")]
public async Task<IActionResult> UploadExcelWithoutDuplicationCheck([FromForm] ExcelUploadRequest request)
{
    var (isSuccess, message, inserted) = await _excelService.ProcessExcelFileWithoutDuplicationCheckAsync(request.File);

    if (!isSuccess)
        return BadRequest(new { message });

    return Ok(new
    {
        message,
        inserted
    });
}

    }
}

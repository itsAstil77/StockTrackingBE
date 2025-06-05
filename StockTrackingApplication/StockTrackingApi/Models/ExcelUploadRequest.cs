using Microsoft.AspNetCore.Http;

namespace StockTrackingAuthAPI.Models
{
    public class ExcelUploadRequest
    {
        public IFormFile File { get; set; }
    }
}

using Microsoft.AspNetCore.Http;

public class CompareExcelRequest
{
    public IFormFile File1 { get; set; }
    public IFormFile File2 { get; set; }
}

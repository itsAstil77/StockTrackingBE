using OfficeOpenXml;

using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

public class ExcelCompareService
{
    public async Task<List<ComparisonResult>> CompareExcelByProductCodeAsync(IFormFile file1, IFormFile file2)
    {
       // ExcelPackage.License = LicenseContext.NonCommercial;

        var data1 = await LoadDataAsync(file1);
        var data2 = await LoadDataAsync(file2);

        var allProductCodes = data1.Keys.Union(data2.Keys).Distinct();

        var results = new List<ComparisonResult>();

        foreach (var code in allProductCodes)
        {
            data1.TryGetValue(code, out int qty1);
            data2.TryGetValue(code, out int qty2);

            var status = qty1 == qty2
                ? "Match"
                : qty1 == 0
                    ? "Missing in F1"
                    : qty2 == 0
                        ? "Missing in F2"
                        : "Mismatch";

            results.Add(new ComparisonResult
            {
                ProductCode = code,
    File1Qty = qty1,
    File2Qty = qty2,
   
    Status = status
            });
        }

        return results;
    }

    private async Task<Dictionary<string, int>> LoadDataAsync(IFormFile file)
{
    var result = new Dictionary<string, int>();

    using var stream = new MemoryStream();
    await file.CopyToAsync(stream);
    using var package = new ExcelPackage(stream);
    var sheet = package.Workbook.Worksheets.First();

    // Detect header columns
    var headerRow = 1;
    int codeColumn = -1, qtyColumn = -1;

    for (int col = 1; col <= sheet.Dimension.End.Column; col++)
    {
        var header = sheet.Cells[headerRow, col].Text.Trim().ToLower();
        if (header.Contains("product") || header.Contains("code"))
            codeColumn = col;
        else if (header.Contains("qty") || header.Contains("quantity"))
            qtyColumn = col;
    }

    if (codeColumn == -1 || qtyColumn == -1)
        throw new Exception("Product Code or Quantity columns not found in the Excel file.");

    for (int row = 2; row <= sheet.Dimension.End.Row; row++)
    {
        var code = sheet.Cells[row, codeColumn].Text.Trim();
        var qtyText = sheet.Cells[row, qtyColumn].Text.Trim();

        if (!string.IsNullOrEmpty(code))
        {
            if (int.TryParse(qtyText, out int qty))
                result[code] = qty;
            else
                result[code] = 0; // if not parsable, default to 0
        }
    }

    return result;
}

}

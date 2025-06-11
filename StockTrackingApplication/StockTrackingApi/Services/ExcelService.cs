using OfficeOpenXml;
using MongoDB.Driver;
using StockTrackingAuthAPI.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace StockTrackingAuthAPI.Services
{
    public class ExcelService
    {
        private readonly IMongoCollection<ExcelData> _collection;
        private readonly IMongoCollection<ExcelData1> _StockCount;

        public ExcelService(IMongoDatabase database)
        {
            _collection = database.GetCollection<ExcelData>("ExcelData");
            _StockCount = database.GetCollection<ExcelData1>("StockCount");
        }
        public async Task<List<ExcelData>> GetAllDataAsync()
        {
            return await _collection.Find(Builders<ExcelData>.Filter.Empty).ToListAsync();
        }

        // Upload Excel and save data to MongoDB
        public async Task<(bool IsSuccess, string Message, int Inserted, int Skipped)> ProcessExcelFileAsync(IFormFile file)
        {
            if (file == null || file.Length == 0)
                return (false, "No file uploaded", 0, 0);

            try
            {
                using var stream = new MemoryStream();
                await file.CopyToAsync(stream);
                using var package = new ExcelPackage(stream);
                var worksheet = package.Workbook.Worksheets[0];

                int rowCount = worksheet.Dimension.Rows;
                int colCount = worksheet.Dimension.Columns;

                var headers = new List<string>();
                for (int col = 1; col <= colCount; col++)
                {
                    headers.Add(worksheet.Cells[1, col].Text);
                }

                int inserted = 0;
                int skipped = 0;

                for (int row = 2; row <= rowCount; row++)
                {
                    var rowDict = new Dictionary<string, object>();
                    string? barcode = null;

                    for (int col = 1; col <= colCount; col++)
                    {
                        string key = headers[col - 1];
                        string value = worksheet.Cells[row, col].Text;

                        if (key.Equals("Barcode", StringComparison.OrdinalIgnoreCase))
                            barcode = value;

                        rowDict[key] = value;
                    }

                    if (string.IsNullOrWhiteSpace(barcode))
                    {
                        skipped++;
                        continue;
                    }

                    bool exists = await _collection.Find(x => x.Barcode == barcode).AnyAsync();
                    if (exists)
                    {
                        skipped++;
                        continue;
                    }

                    var doc = new ExcelData
                    {
                        Barcode = barcode,
                        CreatedDate = DateTime.Now,
                        AdditionalFields = rowDict
                    };

                    await _collection.InsertOneAsync(doc);
                    inserted++;
                }

                return (true, "File processed successfully", inserted, skipped);
            }
            catch (Exception ex)
            {
                return (false, $"Error processing file: {ex.Message}", 0, 0);
            }
        }

        // Export MongoDB data filtered by date range to Excel file stream
        public async Task<byte[]> ExportDataToExcelAsync(DateTime startDate, DateTime endDate)
        {
            // Filter by CreatedDate between startDate and endDate (inclusive)
            var filter = Builders<ExcelData1>.Filter.And(
                Builders<ExcelData1>.Filter.Gte(x => x.CreatedDate, startDate),
                Builders<ExcelData1>.Filter.Lte(x => x.CreatedDate, endDate.AddDays(1))
            );

            var data = await _StockCount.Find(filter).ToListAsync();

            using var package = new ExcelPackage();
            var worksheet = package.Workbook.Worksheets.Add("DataExport");

            if (data.Count == 0)
            {
                worksheet.Cells[1, 1].Value = "No data found for the given date range.";
            }
            else
            {
                // Collect all possible headers dynamically
                var allHeaders = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
                {
                    "Id", "Barcode", "CreatedDate"
                };

                foreach (var doc in data)
                {
                    if (doc.AdditionalFields != null)
                    {
                        foreach (var key in doc.AdditionalFields.Keys)
                            allHeaders.Add(key);
                    }
                }

                var headersList = allHeaders.ToList();

                // Write headers
                for (int col = 0; col < headersList.Count; col++)
                {
                    worksheet.Cells[1, col + 1].Value = headersList[col];
                }

                // Write data rows
                for (int row = 0; row < data.Count; row++)
                {
                    var doc = data[row];
                    for (int col = 0; col < headersList.Count; col++)
                    {
                        string header = headersList[col];
                        object? val = header switch
                        {
                            "Id" => doc.Id,
                           // "Barcode" => doc.Barcode,
                            "CreatedDate" => doc.CreatedDate.ToString("yyyy-MM-dd HH:mm:ss"),
                            _ => doc.AdditionalFields != null && doc.AdditionalFields.ContainsKey(header) ? doc.AdditionalFields[header]?.ToString() : null
                        };

                        worksheet.Cells[row + 2, col + 1].Value = val;
                    }
                }

                worksheet.Cells.AutoFitColumns();
            }

            return package.GetAsByteArray();
        }
       public async Task<(bool IsSuccess, string Message, int Inserted)> ProcessExcelFileWithoutDuplicationCheckAsync(IFormFile file)
{
    if (file == null || file.Length == 0)
        return (false, "No file uploaded", 0);

    try
    {
        using var stream = new MemoryStream();
        await file.CopyToAsync(stream);
        using var package = new ExcelPackage(stream);

        var worksheet = package.Workbook.Worksheets.FirstOrDefault();
        if (worksheet == null || worksheet.Dimension == null)
            return (false, "Excel file has no worksheet or is empty.", 0);

        int rowCount = worksheet.Dimension.Rows;
        int colCount = worksheet.Dimension.Columns;

        var headers = new List<string>();
        for (int col = 1; col <= colCount; col++)
        {
            headers.Add(worksheet.Cells[1, col].Text);
        }

        int inserted = 0;

        for (int row = 2; row <= rowCount; row++)
        {
            var rowDict = new Dictionary<string, object>();

            for (int col = 1; col <= colCount; col++)
            {
                string key = headers[col - 1];
                string value = worksheet.Cells[row, col].Text;

                rowDict[key] = value;
            }

            var doc = new ExcelData1
            {
                //CreatedDate = DateTime.Now,
                AdditionalFields = rowDict
            };

            await _StockCount.InsertOneAsync(doc);
            inserted++;
        }

        return (true, "File processed and saved successfully", inserted);
    }
    catch (Exception ex)
    {
        return (false, $"Error processing file: {ex.Message}", 0);
    }
}


    }
}

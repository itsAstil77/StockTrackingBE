using MongoDB.Bson;
using MongoDB.Driver;
using OfficeOpenXml;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

public class DynamicExcelExportService
{
    private readonly IMongoDatabase _mongoDatabase;

    public DynamicExcelExportService(IMongoClient mongoClient)
    {
        _mongoDatabase = mongoClient.GetDatabase("ExcelData"); // replace with actual DB name
    }

    public async Task<byte[]> ExportAsync(DateTime startDate, DateTime endDate)
    {
        var collection = _mongoDatabase.GetCollection<BsonDocument>("ExcelData"); // replace with collection name

        var filter = Builders<BsonDocument>.Filter.And(
            Builders<BsonDocument>.Filter.Gte("CreatedDate", startDate),
            Builders<BsonDocument>.Filter.Lte("CreatedDate", endDate)
        );

        var documents = await collection.Find(filter).ToListAsync();

        using var package = new ExcelPackage();
        var worksheet = package.Workbook.Worksheets.Add("Export");

        if (documents.Count == 0)
            return package.GetAsByteArray();

        // Get all unique field names
        var headers = new HashSet<string>();
        foreach (var doc in documents)
        {
            foreach (var element in doc.Elements)
            {
                headers.Add(element.Name);
            }
        }

        var headerList = headers.ToList();

        // Write headers
        for (int col = 0; col < headerList.Count; col++)
        {
            worksheet.Cells[1, col + 1].Value = headerList[col];
        }

        // Write rows
        for (int row = 0; row < documents.Count; row++)
        {
            for (int col = 0; col < headerList.Count; col++)
            {
                var fieldName = headerList[col];
                var value = documents[row].Contains(fieldName) ? documents[row][fieldName].ToString() : "";
                worksheet.Cells[row + 2, col + 1].Value = value;
            }
        }

        return package.GetAsByteArray();
    }
}

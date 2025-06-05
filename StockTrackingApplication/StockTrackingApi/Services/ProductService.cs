using OfficeOpenXml;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using MongoDB.Driver;
namespace StockTrackingAuthAPI.Services
{
    public class ProductService
    {
        private readonly IMongoCollection<Product> _productCollection;
        private readonly IMongoCollection<ScanDataModel> _scanLogCollection;

        public ProductService(IConfiguration config)
        {
            var client = new MongoClient(config["MongoDbSettings:ConnectionString"]);
            var database = client.GetDatabase(config["MongoDbSettings:DatabaseName"]);
            _productCollection = database.GetCollection<Product>("Products");
            _scanLogCollection = database.GetCollection<ScanDataModel>("ScanLogs");
        }

        public async Task<Product> GetProductByCodeAsync(string productCode)
        {
            return await _productCollection.Find(p => p.ProductCode == productCode).FirstOrDefaultAsync();
        }

        public async Task<bool> SaveScanDataAsync(ScanDataModel data)
        {
            await _scanLogCollection.InsertOneAsync(data);
            return true;
        }
    }
}
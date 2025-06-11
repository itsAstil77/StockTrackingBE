using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;

namespace StockTrackingAuthAPI.Models
{
    public class ExcelData
    {
        [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string? Id { get; set; }

    [BsonDateTimeOptions(Kind = DateTimeKind.Local)]
    public DateTime CreatedDate { get; set; } = DateTime.Now;

    public string? Barcode { get; set; }

    [BsonExtraElements] // Handles additional unknown fields
    public Dictionary<string, object> AdditionalFields { get; set; } = new();
    }
    public class ExcelData1
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }

        public DateTime CreatedDate { get; set; } = DateTime.Now;

        // This dictionary stores any additional or dynamic fields from the Excel file
        [BsonExtraElements]
        public Dictionary<string, object> AdditionalFields { get; set; }
    }
}

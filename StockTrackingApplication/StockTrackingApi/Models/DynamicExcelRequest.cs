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

        [BsonElement("Barcode")]
        public string Barcode { get; set; }

        [BsonElement("CreatedDate")]
        [BsonDateTimeOptions(Kind = DateTimeKind.Local)] // Optional: ensures consistent timezone handling
        public DateTime CreatedDate { get; set; }

     [BsonExtraElements]
        public Dictionary<string, object> AdditionalFields { get; set; } = new();
    }
}

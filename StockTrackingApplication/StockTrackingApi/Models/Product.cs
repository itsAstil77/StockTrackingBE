using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

public class Product
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string Id { get; set; }

    [BsonElement("productCode")]
    public string ProductCode { get; set; } // e.g., ITEM1001 or QR code

    [BsonElement("productName")]
    public string ProductName { get; set; } // e.g., "Cotton Yarn"

    [BsonElement("description")]
    public string Description { get; set; } // optional

    

    [BsonElement("quantity")]
    public int Quantity { get; set; } // stock quantity


   

    [BsonElement("createdAt")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    
}

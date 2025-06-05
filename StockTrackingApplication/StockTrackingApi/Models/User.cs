using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace StockTrackingAuthAPI.Models
{
    public class User
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; } = "";

        public string UserName { get; set; } = "";
        public string Email { get; set; } = "";
        public string Password { get; set; } = "";
        public List<string> AssignedRoles { get; set; } = new List<string>();
        public string OTP { get; set; } = "";
        public DateTime OTPExpiry { get; set; }
    }

    public class CreateUserRequest
    {
        public string UserName { get; set; } = "";
        public string Email { get; set; } = "";
        public string Password { get; set; } = "";
        public string ConfirmPassword { get; set; } = "";
     
    }
}

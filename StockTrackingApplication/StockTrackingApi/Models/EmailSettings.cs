namespace StockTrackingAuthAPI.Models
{
    public class EmailSettings
    {
        public required string Host { get; set; }
        public int Port { get; set; }
        public required string Username { get; set; }
        public required string Password { get; set; }
    }
}

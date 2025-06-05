namespace StockTrackingAuthAPI.Models;

public class LoginRequest
{
    public required string Email { get; set; }
    public required string Password { get; set; }
}

public class OTPRequest
{
    public required string Email { get; set; }
    public required string OTP { get; set; }
}
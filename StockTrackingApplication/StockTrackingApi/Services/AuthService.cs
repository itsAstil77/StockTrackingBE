using StockTrackingAuthAPI.Config;
using StockTrackingAuthAPI.Models;
using Microsoft.Extensions.Options;
using MongoDB.Driver;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace StockTrackingAuthAPI.Services;

public class AuthService
{
    private readonly IMongoCollection<User> _users;
    private readonly EmailService _emailService;

    public AuthService(IOptions<MongoDbSettings> dbSettings, EmailService emailService)
    {
        var mongoClient = new MongoClient(dbSettings.Value.ConnectionString);
        var database = mongoClient.GetDatabase(dbSettings.Value.DatabaseName);
        _users = database.GetCollection<User>(dbSettings.Value.UserCollection);
        _emailService = emailService;
    }

    public async Task<ServiceResult<string>> CreateUserAsync(CreateUserRequest request)
    {
        if (request.Password != request.ConfirmPassword)
            return ServiceResult<string>.ErrorResult("Passwords do not match");

        if (!IsValidPassword(request.Password))
            return ServiceResult<string>.ErrorResult("Password must be at least 6 characters, include uppercase, lowercase, number, and special character.");

        var existingUser = await _users.Find(u => u.Email == request.Email).FirstOrDefaultAsync();
        if (existingUser != null)
            return ServiceResult<string>.ErrorResult("Email already exists");

        string hashedPassword = BCrypt.Net.BCrypt.HashPassword(request.Password);

        var newUser = new User
        {
            UserName = request.UserName,
            Email = request.Email,
            Password = hashedPassword,
            OTP = "",
            OTPExpiry = DateTime.UtcNow
        };

        await _users.InsertOneAsync(newUser);
        return ServiceResult<string>.SuccessResult(null, "User created successfully");
    }

    private bool IsValidPassword(string password)
    {
        return password.Length >= 6 &&
               password.Any(char.IsUpper) &&
               password.Any(char.IsLower) &&
               password.Any(char.IsDigit) &&
               password.Any(ch => !char.IsLetterOrDigit(ch));
    }

    public async Task<ServiceResult<string>> LoginAsync(string email, string password)
    {
        var user = await _users.Find(u => u.Email == email).FirstOrDefaultAsync();
        if (user == null || !BCrypt.Net.BCrypt.Verify(password, user.Password))
            return ServiceResult<string>.ErrorResult("Invalid credentials");

        var otp = new Random().Next(1000, 9999).ToString();
        user.OTP = otp;
        user.OTPExpiry = DateTime.UtcNow.AddMinutes(1);
        await _users.ReplaceOneAsync(u => u.Id == user.Id, user);

        await _emailService.SendEmailAsync(
            email,
            "Your OTP Code",
            $"Your one-time password is: <b>{otp}</b>. It is valid for 1 minute.");

        return ServiceResult<string>.SuccessResult(null, "OTP sent to email");
    }

    public async Task<ServiceResult<string>> VerifyOtpAsync(string email, string otp)
    {
        var user = await _users.Find(u => u.Email == email).FirstOrDefaultAsync();
        if (user == null)
            return ServiceResult<string>.ErrorResult("User not found");

        if (user.OTP != otp)
            return ServiceResult<string>.ErrorResult("Invalid OTP");

        if (user.OTPExpiry < DateTime.UtcNow)
            return ServiceResult<string>.ErrorResult("OTP expired");

        return ServiceResult<string>.SuccessResult(null, "OTP verified successfully");
    }

    public async Task<ServiceResult<string>> ResendOtpAsync(string email)
    {
        var user = await _users.Find(u => u.Email == email).FirstOrDefaultAsync();
        if (user == null)
            return ServiceResult<string>.ErrorResult("User not found");

        var otp = new Random().Next(1000, 9999).ToString();
        user.OTP = otp;
        user.OTPExpiry = DateTime.UtcNow.AddMinutes(1);
        await _users.ReplaceOneAsync(u => u.Id == user.Id, user);

        await _emailService.SendEmailAsync(
            email,
            "Your OTP Code",
            $"Your one-time password is: <b>{otp}</b>. It is valid for 1 minute.");

        return ServiceResult<string>.SuccessResult(null, "OTP resent");
    }
}

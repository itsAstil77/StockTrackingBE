using Microsoft.AspNetCore.Mvc;
using StockTrackingAuthAPI.Models;
using StockTrackingAuthAPI.Services;
using System.Threading.Tasks;

namespace AssetTrackingAuthAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly AuthService _authService;

    public AuthController(AuthService authService)
    {
        _authService = authService;
    }

    [HttpPost("create-user")]
    public async Task<IActionResult> CreateUser([FromBody] CreateUserRequest request)
    {
        var result = await _authService.CreateUserAsync(request);
        if (!result.Success)
        {
            return result.Message switch
            {
                "Passwords do not match" => BadRequest(new { message = result.Message }),
                "Password must be at least 6 characters, include uppercase, lowercase, number, and special character." => BadRequest(new { message = result.Message }),
                "Email already exists" => Conflict(new { message = result.Message }),
                _ => BadRequest(new { message = result.Message })
            };
        }

        return Ok(new { message = result.Message });
    }

    [HttpPost("login")]
public async Task<IActionResult> Login([FromBody] LoginRequest request)
{
    var result = await _authService.LoginAsync(request.Email, request.Password);
    if (!result.Success)
        return Unauthorized(new { message = result.Message });

    return Ok(new { message = result.Message });
}


    // [HttpPost("verify-otp")]
    // public async Task<IActionResult> VerifyOtp([FromBody] OTPRequest request)
    // {
    //     var result = await _authService.VerifyOtpAsync(request.Email, request.OTP);
    //     if (!result.Success)
    //     {
    //         return result.Message switch
    //         {
    //             "User not found" => NotFound(new { message = result.Message }),
    //             "Invalid OTP" => BadRequest(new { message = result.Message }),
    //             "OTP expired" => BadRequest(new{message=result.Message}),
    //             _ => BadRequest(new { message = result.Message })
    //         };
    //     }

    //     return Ok(new { message = result.Message });
    // }

    // [HttpPost("resend-otp")]
    // public async Task<IActionResult> ResendOtp([FromBody] OTPRequest request)
    // {
    //     var result = await _authService.ResendOtpAsync(request.Email);
    //     if (!result.Success)
    //         return NotFound(new { message = result.Message });

    //     return Ok(new { message = result.Message });
    // }
}

// Controllers/AuthController.cs
using ChatApp.API.Services;
using Microsoft.AspNetCore.Mvc;

namespace ChatApp.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController(AuthService authService) : ControllerBase
{
    // POST api/auth/send-otp
    [HttpPost("send-otp")]
    public async Task<IActionResult> SendOtp([FromBody] SendOtpRequest req)
    {
        if (string.IsNullOrWhiteSpace(req.Email))
            return BadRequest("Email is required.");

        await authService.SendOtpAsync(req.Email);

        return Ok(new { message = "OTP sent to your email." });
    }

    // POST api/auth/verify-otp
    [HttpPost("verify-otp")]
    public async Task<IActionResult> VerifyOtp([FromBody] VerifyOtpRequest req)
    {
        if (string.IsNullOrWhiteSpace(req.Email) ||
            string.IsNullOrWhiteSpace(req.Code))
            return BadRequest("Email and code are required.");

        var result = await authService.VerifyOtpAsync(req.Email, req.Code);

        if (!result.Success)
            return Unauthorized(new { message = result.Message });

        return Ok(new
        {
            message  = result.Message,
            isNew    = result.Message == "Registered",
            userId   = result.User!.Id,
            username = result.User.Username,
            email    = result.User.Email,
        });
    }
}

public record SendOtpRequest(string Email);
public record VerifyOtpRequest(string Email, string Code);
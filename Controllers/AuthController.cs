using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using POS.API.Services.Contracts;
using POS.API.Models;

namespace POS.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;
    private readonly ILogger<AuthController> _logger;

    public AuthController(IAuthService authService, ILogger<AuthController> logger)
    {
        _authService = authService;
        _logger = logger;
    }

    [AllowAnonymous]
    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        _logger.LogInformation("POST /api/auth/login called");
        var result = await _authService.LoginAsync(request);
        if (!result.Success)
        {
            _logger.LogWarning("Login failed for username {Username}: {Error}", request.Username, result.ErrorMessage);
            return StatusCode(result.StatusCode, new { error = result.ErrorMessage });
        }

        _logger.LogInformation("Login completed for username {Username}", request.Username);
        return Ok(result.Payload);
    }

    [AllowAnonymous]
    [HttpPost("refresh")]
    public async Task<IActionResult> Refresh([FromBody] RefreshTokenRequest request)
    {
        _logger.LogInformation("POST /api/auth/refresh called");
        var result = await _authService.RefreshTokenAsync(request);
        if (!result.Success)
        {
            _logger.LogWarning("Token refresh failed: {Error}", result.ErrorMessage);
            return StatusCode(result.StatusCode, new { error = result.ErrorMessage });
        }

        _logger.LogInformation("Token refresh completed");
        return Ok(result.Payload);
    }

    [AllowAnonymous]
    [HttpPost("logout")]
    public async Task<IActionResult> Logout([FromBody] RefreshTokenRequest request)
    {
        _logger.LogInformation("POST /api/auth/logout called");
        var result = await _authService.LogoutAsync(request);
        if (!result.Success)
        {
            _logger.LogWarning("Logout failed: {Error}", result.ErrorMessage);
            return StatusCode(result.StatusCode, new { error = result.ErrorMessage });
        }

        _logger.LogInformation("Logout completed");
        return Ok(new { success = true });
    }

    [AllowAnonymous]
    [HttpPost("request-reset")]
    public async Task<IActionResult> RequestPasswordReset([FromBody] RequestPasswordResetRequest request)
    {
        _logger.LogInformation("POST /api/auth/request-reset called");
        var result = await _authService.RequestPasswordResetAsync(request);
        if (!result.Success)
        {
            _logger.LogWarning("Password reset request failed for username {Username}: {Error}", request.Username, result.ErrorMessage);
            return StatusCode(result.StatusCode, new { error = result.ErrorMessage });
        }

        _logger.LogInformation("Password reset request completed for username {Username}", request.Username);
        return Ok(result.Payload);
    }

    [AllowAnonymous]
    [HttpPost("request-otp")]
    public async Task<IActionResult> RequestOtp([FromBody] RequestOtpRequest request)
    {
        _logger.LogInformation("POST /api/auth/request-otp called");
        var result = await _authService.RequestOtpAsync(request);
        if (!result.Success)
        {
            _logger.LogWarning("OTP request failed: {Error}", result.ErrorMessage);
            return StatusCode(result.StatusCode, new { error = result.ErrorMessage });
        }

        _logger.LogInformation("OTP request completed");
        return Ok(new { message = "If that email is registered, an OTP has been sent." });
    }

    [AllowAnonymous]
    [HttpPost("verify-otp")]
    public async Task<IActionResult> VerifyOtp([FromBody] VerifyOtpRequest request)
    {
        _logger.LogInformation("POST /api/auth/verify-otp called");
        var result = await _authService.VerifyOtpAsync(request);
        if (!result.Success)
        {
            _logger.LogWarning("OTP verification failed: {Error}", result.ErrorMessage);
            return StatusCode(result.StatusCode, new { error = result.ErrorMessage });
        }

        _logger.LogInformation("OTP verification completed");
        return Ok(result.Payload);
    }

    [AllowAnonymous]
    [HttpPost("reset-password")]
    public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordRequest request)
    {
        _logger.LogInformation("POST /api/auth/reset-password called");
        var result = await _authService.ResetPasswordAsync(request);
        if (!result.Success)
        {
            _logger.LogWarning("Password reset failed: {Error}", result.ErrorMessage);
            return StatusCode(result.StatusCode, new { error = result.ErrorMessage });
        }

        _logger.LogInformation("Password reset completed");
        return Ok(new { success = true });
    }

    [Authorize]
    [HttpPost("change-password")]
    public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordRequest request)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId))
            return Unauthorized();

        _logger.LogInformation("POST /api/auth/change-password called for userId {UserId}", userId);
        var result = await _authService.ChangePasswordAsync(userId, request.CurrentPassword, request.NewPassword);
        if (!result.Success)
        {
            _logger.LogWarning("Change password failed for userId {UserId}: {Error}", userId, result.ErrorMessage);
            return StatusCode(result.StatusCode, new { error = result.ErrorMessage });
        }

        _logger.LogInformation("Change password completed for userId {UserId}", userId);
        return Ok(new { success = true });
    }

    [Authorize]
    [HttpGet("me")]
    public async Task<IActionResult> Me()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId))
            return Unauthorized();

        _logger.LogInformation("GET /api/auth/me called for userId {UserId}", userId);
        var result = await _authService.GetUserAsync(userId);
        if (!result.Success)
        {
            _logger.LogWarning("Get user failed for userId {UserId}: {Error}", userId, result.ErrorMessage);
            return StatusCode(result.StatusCode, new { error = result.ErrorMessage });
        }

        _logger.LogInformation("Get user completed for userId {UserId}", userId);
        return Ok(result.Payload);
    }
}

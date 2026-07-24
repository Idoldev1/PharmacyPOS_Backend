using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using POS.API.Repositories.Interfaces;
using POS.API.Services.Contracts;
using POS.API.Models;
using POS.API.Constants;

namespace POS.API.Services.Services;

public class AuthService : IAuthService
{
    private readonly JwtSettings _jwtSettings;
    private readonly UserManager<UserRecord> _userManager;
    private readonly SignInManager<UserRecord> _signInManager;
    private readonly IRefreshTokenRepository _refreshTokenRepository;
    private readonly IPasswordResetRepository _passwordResetRepository;
    private readonly IOtpRepository _otpRepository;
    private readonly IEmailService _emailService;
    private readonly ILogger<AuthService> _logger;

    public AuthService(
        UserManager<UserRecord> userManager,
        SignInManager<UserRecord> signInManager,
        IRefreshTokenRepository refreshTokenRepository,
        IPasswordResetRepository passwordResetRepository,
        IOtpRepository otpRepository,
        IEmailService emailService,
        IOptions<JwtSettings> jwtOptions,
        ILogger<AuthService> logger)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _refreshTokenRepository = refreshTokenRepository;
        _passwordResetRepository = passwordResetRepository;
        _otpRepository = otpRepository;
        _emailService = emailService;
        _jwtSettings = jwtOptions.Value;
        _logger = logger;
    }

    public async Task<OperationResult<AuthResponse>> LoginAsync(LoginRequest request)
    {
        _logger.LogInformation("Login attempt for username {Username}", request.Username);

        var user = await _userManager.FindByNameAsync(request.Username);
        if (user is null || !string.Equals(user.UserName, request.Username, StringComparison.Ordinal))
        {
            _logger.LogWarning("Login failed: user not found or casing mismatch for username {Username}", request.Username);
            return OperationResult<AuthResponse>.Fail("Invalid username or password.");
        }
        var result = await _signInManager.CheckPasswordSignInAsync(user, request.Password, lockoutOnFailure: false);
        if (!result.Succeeded)
        {
            _logger.LogWarning("Login failed: invalid password for username {Username}", request.Username);
            return OperationResult<AuthResponse>.Fail("Invalid username or password.");
        }

        if (!string.Equals(request.Role, user.Role, StringComparison.OrdinalIgnoreCase))
        {
            _logger.LogWarning("Login failed: role mismatch for username {Username}", request.Username);
            return OperationResult<AuthResponse>.Fail("Invalid username or password.");
        }

        var authResponse = await CreateAuthResponse(user);
        _logger.LogInformation("Login successful for username {Username}, userId {UserId}", request.Username, user.Id);
        return OperationResult<AuthResponse>.Ok(authResponse);

    }

    public async Task<OperationResult<AuthResponse>> RefreshTokenAsync(RefreshTokenRequest request)
    {
        _logger.LogInformation("Token refresh attempt");

        var entry = await _refreshTokenRepository.GetByTokenAsync(request.Token);
        if (entry is null || entry.ExpiresAt < DateTime.UtcNow)
        {
            _logger.LogWarning("Token refresh failed: token is invalid or expired");
            return OperationResult<AuthResponse>.Fail("Refresh token is invalid or expired.", 401);
        }

        var user = await _userManager.FindByIdAsync(entry.UserId);
        if (user is null)
        {
            _logger.LogWarning("Token refresh failed: user not found for userId {UserId}", entry.UserId);
            return OperationResult<AuthResponse>.Fail("User not found.", 404);
        }

        await _refreshTokenRepository.DeleteAsync(entry.Token);

        var authResponse = await CreateAuthResponse(user);
        _logger.LogInformation("Token refresh successful for userId {UserId}", user.Id);
        return OperationResult<AuthResponse>.Ok(authResponse);
    }

    public async Task<OperationResult> LogoutAsync(RefreshTokenRequest request)
    {
        _logger.LogInformation("Logout attempt");

        await _refreshTokenRepository.DeleteAsync(request.Token);

        _logger.LogInformation("Logout successful");
        return OperationResult.Ok();
    }

    public async Task<OperationResult<RequestPasswordResetResponse>> RequestPasswordResetAsync(RequestPasswordResetRequest request)
    {
        _logger.LogInformation("Password reset requested for username {Username}", request.Username);

        var user = await _userManager.FindByNameAsync(request.Username);
        if (user is null)
        {
            _logger.LogWarning("Password reset requested for non-existent username {Username}", request.Username);
            return OperationResult<RequestPasswordResetResponse>.Ok(new RequestPasswordResetResponse
            {
                ResetToken = string.Empty,
                Message = "If that account exists, a reset token has been issued."
            });
        }

        var resetToken = Convert.ToHexString(RandomNumberGenerator.GetBytes(24));
        await _passwordResetRepository.AddAsync(new PasswordResetEntry
        {
            Token = resetToken,
            UserId = user.Id,
            ExpiresAt = DateTime.UtcNow.AddMinutes(_jwtSettings.ResetTokenMinutes)
        });

        _logger.LogInformation("Password reset token generated for userId {UserId}", user.Id);
        return OperationResult<RequestPasswordResetResponse>.Ok(new RequestPasswordResetResponse
        {
            ResetToken = resetToken,
            Message = "Use this reset token to complete the password reset flow."
        });
    }

    public async Task<OperationResult> ResetPasswordAsync(ResetPasswordRequest request)
    {
        _logger.LogInformation("Password reset attempt");

        var entry = await _passwordResetRepository.GetByTokenAsync(request.Token);
        if (entry is null || entry.ExpiresAt < DateTime.UtcNow)
        {
            _logger.LogWarning("Password reset failed: reset token is invalid or expired");
            return OperationResult.Fail("Reset token is invalid or has expired.");
        }

        var user = await _userManager.FindByIdAsync(entry.UserId);
        if (user is null)
        {
            _logger.LogWarning("Password reset failed: user not found for userId {UserId}", entry.UserId);
            return OperationResult.Fail("User not found.", 404);
        }

        await _userManager.RemovePasswordAsync(user);
        var result = await _userManager.AddPasswordAsync(user, request.NewPassword);
        if (!result.Succeeded)
        {
            var error = result.Errors.FirstOrDefault()?.Description ?? "Failed to reset password.";
            _logger.LogWarning("Password reset failed for userId {UserId}: {Error}", user.Id, error);
            return OperationResult.Fail(error);
        }

        await _passwordResetRepository.DeleteAsync(request.Token);

        _logger.LogInformation("Password reset successful for userId {UserId}", user.Id);
        return OperationResult.Ok();
    }

    public async Task<OperationResult> ChangePasswordAsync(string userId, string currentPassword, string newPassword)
    {
        _logger.LogInformation("Password change attempt for userId {UserId}", userId);

        var user = await _userManager.FindByIdAsync(userId);
        if (user is null)
        {
            _logger.LogWarning("Password change failed: user not found for userId {UserId}", userId);
            return OperationResult.Fail("User not found.", 404);
        }

        var result = await _userManager.ChangePasswordAsync(user, currentPassword, newPassword);
        if (!result.Succeeded)
        {
            var error = result.Errors.FirstOrDefault()?.Description ?? "Failed to change password.";
            _logger.LogWarning("Password change failed for userId {UserId}: {Error}", userId, error);
            return OperationResult.Fail(error);
        }

        _logger.LogInformation("Password change successful for userId {UserId}", userId);
        return OperationResult.Ok();
    }

    public async Task<OperationResult> RequestOtpAsync(RequestOtpRequest request)
    {
        _logger.LogInformation("OTP requested for email {Email}", request.Email);

        var user = await _userManager.FindByEmailAsync(request.Email);
        if (user is null)
        {
            _logger.LogWarning("OTP requested for non-existent email {Email}", request.Email);
            return OperationResult.Ok();
        }

        var otp = Random.Shared.Next(100000, 999999).ToString();

        await _otpRepository.DeleteByUserIdAsync(user.Id);
        await _otpRepository.AddAsync(new OtpEntry
        {
            UserId = user.Id,
            Code = otp,
            ExpiresAt = DateTime.UtcNow.AddMinutes(10)
        });

        await _emailService.SendOtpAsync(request.Email, otp);

        _logger.LogInformation("OTP sent to email {Email}", request.Email);
        return OperationResult.Ok();
    }

    public async Task<OperationResult<VerifyOtpResponse>> VerifyOtpAsync(VerifyOtpRequest request)
    {
        _logger.LogInformation("OTP verification attempt for email {Email}", request.Email);

        var user = await _userManager.FindByEmailAsync(request.Email);
        if (user is null)
        {
            _logger.LogWarning("OTP verification failed: email not found {Email}", request.Email);
            return OperationResult<VerifyOtpResponse>.Fail("Invalid OTP or email.");
        }

        var entry = await _otpRepository.GetByUserIdAsync(user.Id);
        if (entry is null || entry.ExpiresAt < DateTime.UtcNow || entry.Code != request.Otp)
        {
            _logger.LogWarning("OTP verification failed for userId {UserId}: invalid or expired", user.Id);
            return OperationResult<VerifyOtpResponse>.Fail("Invalid or expired OTP.");
        }

        await _otpRepository.DeleteByUserIdAsync(user.Id);

        var resetToken = Convert.ToHexString(RandomNumberGenerator.GetBytes(24));
        await _passwordResetRepository.AddAsync(new PasswordResetEntry
        {
            Token = resetToken,
            UserId = user.Id,
            ExpiresAt = DateTime.UtcNow.AddMinutes(_jwtSettings.ResetTokenMinutes)
        });

        _logger.LogInformation("OTP verified for userId {UserId}, reset token issued", user.Id);
        return OperationResult<VerifyOtpResponse>.Ok(new VerifyOtpResponse
        {
            ResetToken = resetToken,
            Message = "OTP verified. You may now reset your password."
        });
    }

    public async Task<OperationResult<AuthUser>> GetUserAsync(string userId)
    {
        _logger.LogInformation("Retrieving user for userId {UserId}", userId);

        var user = await _userManager.FindByIdAsync(userId);
        if (user is null)
        {
            _logger.LogWarning("User not found for userId {UserId}", userId);
            return OperationResult<AuthUser>.Fail("User not found.", 404);
        }

        _logger.LogInformation("User retrieved successfully for userId {UserId}", userId);
        return OperationResult<AuthUser>.Ok(ToAuthUser(user));
    }

    private async Task<AuthResponse> CreateAuthResponse(UserRecord user)
    {
        _logger.LogInformation("Creating auth response for userId {UserId}", user.Id);

        var expiresAt = DateTime.UtcNow.AddMinutes(_jwtSettings.AccessTokenMinutes);
        var accessToken = GenerateAccessToken(user, expiresAt);
        var refreshToken = Convert.ToHexString(RandomNumberGenerator.GetBytes(32));

        await _refreshTokenRepository.AddAsync(new RefreshTokenEntry
        {
            Token = refreshToken,
            UserId = user.Id,
            ExpiresAt = DateTime.UtcNow.AddDays(_jwtSettings.RefreshTokenDays)
        });

        _logger.LogInformation("Auth response created for userId {UserId}", user.Id);
        return new AuthResponse
        {
            AccessToken = accessToken,
            RefreshToken = refreshToken,
            ExpiresIn = _jwtSettings.AccessTokenMinutes * 60,
            User = ToAuthUser(user)
        };
    }

    private static AuthUser ToAuthUser(UserRecord user) => new()
    {
        Id = user.Id,
        Username = user.UserName!,
        Email = user.Email ?? "",
        FirstName = user.FirstName,
        LastName = user.LastName,
        Role = user.Role,
        BranchId = user.BranchId,
        Permissions = RolePermissions.GetPermissions(user.Role)
    };

    private string GenerateAccessToken(UserRecord user, DateTime expiresAt)
    {
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtSettings.Key));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, user.Id),
            new(ClaimTypes.Name, user.UserName!),
            new(ClaimTypes.Role, user.Role),
            new("branchId", user.BranchId)
        };

        foreach (var perm in RolePermissions.GetPermissions(user.Role))
            claims.Add(new Claim("permission", perm));

        var token = new JwtSecurityToken(
            issuer: _jwtSettings.Issuer,
            audience: _jwtSettings.Audience,
            claims: claims,
            expires: expiresAt,
            signingCredentials: creds);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}

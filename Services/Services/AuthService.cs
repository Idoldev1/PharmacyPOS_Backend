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
    private readonly RoleManager<IdentityRole> _roleManager;
    private readonly IRefreshTokenRepository _refreshTokenRepository;
    private readonly IPasswordResetRepository _passwordResetRepository;
    private readonly ILogger<AuthService> _logger;

    public AuthService(
        UserManager<UserRecord> userManager,
        SignInManager<UserRecord> signInManager,
        RoleManager<IdentityRole> roleManager,
        IRefreshTokenRepository refreshTokenRepository,
        IPasswordResetRepository passwordResetRepository,
        IOptions<JwtSettings> jwtOptions,
        ILogger<AuthService> logger)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _roleManager = roleManager;
        _refreshTokenRepository = refreshTokenRepository;
        _passwordResetRepository = passwordResetRepository;
        _jwtSettings = jwtOptions.Value;
        _logger = logger;
    }

    public async Task<OperationResult<AuthResponse>> LoginAsync(LoginRequest request)
    {
        _logger.LogInformation("Login attempt for username {Username}", request.Username);

        var user = await _userManager.FindByNameAsync(request.Username);
        if (user is null)
        {
            _logger.LogWarning("Login failed: user not found for username {Username}", request.Username);
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

    public async Task<OperationResult<AuthResponse>> SignupAsync(SignupRequest request)
    {
        _logger.LogInformation("Signup attempt for username {Username}, role {Role}", request.Username, request.Role);

        var normalizedRole = Roles.Normalize(request.Role);
        if (normalizedRole is null)
        {
            var error = "Invalid role.";
            _logger.LogWarning("Registration rejected for {Email}: invalid role {Role}", request.FirstName, request.Role);
            return OperationResult<AuthResponse>.Fail(error);
        }

        var user = new UserRecord
        {
            UserName = request.Username,
            FirstName = request.FirstName,
            LastName = request.LastName,
            Role = request.Role,
            BranchId = request.BranchId
        };

        var result = await _userManager.CreateAsync(user, request.Password);
        if (!result.Succeeded)
        {
            var error = result.Errors.FirstOrDefault()?.Description ?? "Failed to create account.";
            _logger.LogWarning("Signup failed for username {Username}: {Error}", request.Username, error);
            return OperationResult<AuthResponse>.Fail(error);
        }

        if (!await _roleManager.RoleExistsAsync(normalizedRole))
        {
            _logger.LogInformation("Creating missing role {Role}", normalizedRole);
            await _roleManager.CreateAsync(new IdentityRole(normalizedRole));
        }

        var authResponse = await CreateAuthResponse(user);
        _logger.LogInformation("Signup successful for username {Username}, userId {UserId}", request.Username, user.Id);
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
        _logger.LogDebug("Creating auth response for userId {UserId}", user.Id);

        var expiresAt = DateTime.UtcNow.AddMinutes(_jwtSettings.AccessTokenMinutes);
        var accessToken = GenerateAccessToken(user, expiresAt);
        var refreshToken = Convert.ToHexString(RandomNumberGenerator.GetBytes(32));

        await _refreshTokenRepository.AddAsync(new RefreshTokenEntry
        {
            Token = refreshToken,
            UserId = user.Id,
            ExpiresAt = DateTime.UtcNow.AddDays(_jwtSettings.RefreshTokenDays)
        });

        _logger.LogDebug("Auth response created for userId {UserId}", user.Id);
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
        FirstName = user.FirstName,
        LastName = user.LastName,
        Role = user.Role,
        BranchId = user.BranchId
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

        var token = new JwtSecurityToken(
            issuer: _jwtSettings.Issuer,
            audience: _jwtSettings.Audience,
            claims: claims,
            expires: expiresAt,
            signingCredentials: creds);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}

using POS.API.Models;

namespace POS.API.Services.Contracts;

public interface IAuthService
{
    Task<OperationResult<AuthResponse>> LoginAsync(LoginRequest request);
    Task<OperationResult<AuthResponse>> RefreshTokenAsync(RefreshTokenRequest request);
    Task<OperationResult> LogoutAsync(RefreshTokenRequest request);
    Task<OperationResult<RequestPasswordResetResponse>> RequestPasswordResetAsync(RequestPasswordResetRequest request);
    Task<OperationResult> ResetPasswordAsync(ResetPasswordRequest request);
    Task<OperationResult> RequestOtpAsync(RequestOtpRequest request);
    Task<OperationResult<VerifyOtpResponse>> VerifyOtpAsync(VerifyOtpRequest request);
    Task<OperationResult> ChangePasswordAsync(string userId, string currentPassword, string newPassword);
    Task<OperationResult<AuthUser>> GetUserAsync(string userId);
}

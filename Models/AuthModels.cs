namespace POS.API.Models;

public class LoginRequest
{
    public string Username { get; set; } = null!;
    public string Password { get; set; } = null!;
    public string Role { get; set; } = null!;
}

public class SignupRequest
{
    public string Username { get; set; } = null!;
    public string Password { get; set; } = null!;
    public string FirstName { get; set; } = null!;
    public string LastName { get; set; } = null!;
    public string Role { get; set; } = "Pharmacist";
    public string BranchId { get; set; } = "HQ";
}

public class RefreshTokenRequest
{
    public string Token { get; set; } = null!;
}

public class RequestPasswordResetRequest
{
    public string Username { get; set; } = null!;
}

public class RequestPasswordResetResponse
{
    public string ResetToken { get; set; } = null!;
    public string Message { get; set; } = null!;
}

public class ResetPasswordRequest
{
    public string Token { get; set; } = null!;
    public string NewPassword { get; set; } = null!;
}

public class ChangePasswordRequest
{
    public string CurrentPassword { get; set; } = null!;
    public string NewPassword { get; set; } = null!;
}

public class AuthResponse
{
    public string AccessToken { get; set; } = null!;
    public string RefreshToken { get; set; } = null!;
    public int ExpiresIn { get; set; }
    public AuthUser User { get; set; } = null!;
}

public class AuthUser
{
    public string Id { get; set; } = null!;
    public string Username { get; set; } = null!;
    public string FirstName { get; set; } = null!;
    public string LastName { get; set; } = null!;
    public string Role { get; set; } = null!;
    public string BranchId { get; set; } = null!;
}

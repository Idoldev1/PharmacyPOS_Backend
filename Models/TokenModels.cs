namespace POS.API.Models;

public class RefreshTokenEntry
{
    public string Token { get; set; } = null!;
    public string UserId { get; set; } = null!;
    public DateTime ExpiresAt { get; set; }
}

public class PasswordResetEntry
{
    public string Token { get; set; } = null!;
    public string UserId { get; set; } = null!;
    public DateTime ExpiresAt { get; set; }
}

public class OtpEntry
{
    public int Id { get; set; }
    public string UserId { get; set; } = null!;
    public string Code { get; set; } = null!;
    public DateTime ExpiresAt { get; set; }
}

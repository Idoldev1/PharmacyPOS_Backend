namespace POS.API.Models;

public class BranchSettingsDto
{
    public string PharmacyName { get; set; } = null!;
    public string BranchName { get; set; } = null!;
    public string? Address { get; set; }
    public string? Phone { get; set; }
    public string? Email { get; set; }
    public string? NafdacNumber { get; set; }
    public string ReceiptHeader { get; set; } = null!;
    public string ReceiptFooter { get; set; } = null!;
    public bool ShowLogo { get; set; }
    public bool ShowBarcode { get; set; }
    public decimal VatRate { get; set; }
    public string? TaxId { get; set; }
    public bool ApplyVat { get; set; }
    public bool AutoBackup { get; set; }
    public DateTime? LastBackupAt { get; set; }
}

public class UpdateSettingsRequest
{
    public string PharmacyName { get; set; } = null!;
    public string BranchName { get; set; } = null!;
    public string? Address { get; set; }
    public string? Phone { get; set; }
    public string? Email { get; set; }
    public string? NafdacNumber { get; set; }
    public string ReceiptHeader { get; set; } = null!;
    public string ReceiptFooter { get; set; } = null!;
    public bool ShowLogo { get; set; }
    public bool ShowBarcode { get; set; }
    public decimal VatRate { get; set; }
    public string? TaxId { get; set; }
    public bool ApplyVat { get; set; }
    public bool AutoBackup { get; set; }
}

public class StaffUserDto
{
    public string Id { get; set; } = null!;
    public string FirstName { get; set; } = null!;
    public string LastName { get; set; } = null!;
    public string FullName => $"{FirstName} {LastName}";
    public string Username { get; set; } = null!;
    public string Role { get; set; } = null!;
    public bool IsActive { get; set; }
}

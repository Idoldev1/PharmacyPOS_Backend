namespace POS.API.Models;

public class BranchSettings
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string BranchId { get; set; } = null!;
    public string PharmacyName { get; set; } = "HealthPlus Pharmacy";
    public string BranchName { get; set; } = "Main Branch";
    public string? Address { get; set; }
    public string? Phone { get; set; }
    public string? Email { get; set; }
    public string? NafdacNumber { get; set; }
    public string ReceiptHeader { get; set; } = "HealthPlus Pharmacy - Your Health, Our Priority";
    public string ReceiptFooter { get; set; } = "Thank you for your patronage.";
    public bool ShowLogo { get; set; } = true;
    public bool ShowBarcode { get; set; } = true;
    public decimal VatRate { get; set; } = 7.5m;
    public string? TaxId { get; set; }
    public bool ApplyVat { get; set; } = true;
    public bool AutoBackup { get; set; } = true;
    public DateTime? LastBackupAt { get; set; }
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}

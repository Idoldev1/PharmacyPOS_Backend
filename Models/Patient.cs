namespace POS.API.Models;

public class Patient
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string FirstName { get; set; } = null!;
    public string LastName { get; set; } = null!;
    public string Phone { get; set; } = null!;
    public string? Email { get; set; }
    public DateTime DateOfBirth { get; set; }
    public string Gender { get; set; } = null!;
    public string? Address { get; set; }
    public List<string> Allergies { get; set; } = [];
    public string? NhisNumber { get; set; }
    public string BranchId { get; set; } = null!;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}

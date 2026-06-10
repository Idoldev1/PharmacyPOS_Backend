namespace POS.API.Models;

public class PatientDto
{
    public Guid Id { get; set; }
    public string FirstName { get; set; } = null!;
    public string LastName { get; set; } = null!;
    public string FullName => $"{FirstName} {LastName}";
    public string Phone { get; set; } = null!;
    public string? Email { get; set; }
    public DateTime DateOfBirth { get; set; }
    public string Gender { get; set; } = null!;
    public string? Address { get; set; }
    public List<string> Allergies { get; set; } = [];
    public string? NhisNumber { get; set; }
    public string BranchId { get; set; } = null!;
    public DateTime CreatedAt { get; set; }
}

public class CreatePatientRequest
{
    public string FirstName { get; set; } = null!;
    public string LastName { get; set; } = null!;
    public string Phone { get; set; } = null!;
    public string? Email { get; set; }
    public DateTime DateOfBirth { get; set; }
    public string Gender { get; set; } = null!;
    public string? Address { get; set; }
    public List<string> Allergies { get; set; } = [];
    public string? NhisNumber { get; set; }
}

public class PatientListResult
{
    public List<PatientDto> Items { get; set; } = [];
    public int Total { get; set; }
    public int Page { get; set; }
    public int PageSize { get; set; }
}

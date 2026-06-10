namespace POS.API.Models;

public class Prescription
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string RxNumber { get; set; } = null!;
    public string PatientName { get; set; } = null!;
    public string? PatientId { get; set; }
    public string DoctorName { get; set; } = null!;
    public string? DoctorLicense { get; set; }
    public string? HospitalName { get; set; }
    public DateTime PrescribedDate { get; set; } = DateTime.UtcNow;
    public string Status { get; set; } = "pending";
    public string? FlagReason { get; set; }
    public string? VerifiedBy { get; set; }
    public string? DispensedBy { get; set; }
    public DateTime? VerifiedAt { get; set; }
    public DateTime? DispensedAt { get; set; }
    public string BranchId { get; set; } = null!;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public List<PrescriptionLine> Lines { get; set; } = [];
}

public class PrescriptionLine
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid PrescriptionId { get; set; }
    public Prescription Prescription { get; set; } = null!;
    public string DrugName { get; set; } = null!;
    public Guid? DrugId { get; set; }
    public string Dosage { get; set; } = null!;
    public int Quantity { get; set; }
    public string? Instructions { get; set; }
}

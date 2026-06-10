namespace POS.API.Models;

public class PrescriptionDto
{
    public Guid Id { get; set; }
    public string RxNumber { get; set; } = null!;
    public string PatientName { get; set; } = null!;
    public string? PatientId { get; set; }
    public string DoctorName { get; set; } = null!;
    public string? DoctorLicense { get; set; }
    public string? HospitalName { get; set; }
    public DateTime PrescribedDate { get; set; }
    public string Status { get; set; } = null!;
    public string? FlagReason { get; set; }
    public string? VerifiedBy { get; set; }
    public string? DispensedBy { get; set; }
    public DateTime? VerifiedAt { get; set; }
    public DateTime? DispensedAt { get; set; }
    public DateTime CreatedAt { get; set; }
    public List<PrescriptionLineDto> Lines { get; set; } = [];
}

public class PrescriptionLineDto
{
    public Guid Id { get; set; }
    public string DrugName { get; set; } = null!;
    public Guid? DrugId { get; set; }
    public string Dosage { get; set; } = null!;
    public int Quantity { get; set; }
    public string? Instructions { get; set; }
}

public class CreatePrescriptionRequest
{
    public string PatientName { get; set; } = null!;
    public string? PatientId { get; set; }
    public string DoctorName { get; set; } = null!;
    public string? DoctorLicense { get; set; }
    public string? HospitalName { get; set; }
    public DateTime PrescribedDate { get; set; }
    public List<CreatePrescriptionLineRequest> Lines { get; set; } = [];
}

public class CreatePrescriptionLineRequest
{
    public string DrugName { get; set; } = null!;
    public Guid? DrugId { get; set; }
    public string Dosage { get; set; } = null!;
    public int Quantity { get; set; }
    public string? Instructions { get; set; }
}

public class FlagPrescriptionRequest
{
    public string Reason { get; set; } = null!;
}

public class PrescriptionListResult
{
    public List<PrescriptionDto> Items { get; set; } = [];
    public int Total { get; set; }
    public int Page { get; set; }
    public int PageSize { get; set; }
}

using POS.API.Models;
using POS.API.Repositories.Interfaces;
using POS.API.Services.Contracts;

namespace POS.API.Services.Services;

public class PatientService : IPatientService
{
    private readonly IPatientRepository _patientRepo;
    private readonly ISaleRepository _saleRepo;
    private readonly ILogger<PatientService> _logger;

    public PatientService(IPatientRepository patientRepo, ISaleRepository saleRepo, ILogger<PatientService> logger)
    {
        _patientRepo = patientRepo;
        _saleRepo = saleRepo;
        _logger = logger;
    }

    public async Task<OperationResult<PatientListResult>> GetPatientsAsync(string? query, int page, int pageSize, string branchId)
    {
        var (items, total) = await _patientRepo.GetPagedAsync(query, page, pageSize, branchId);
        return OperationResult<PatientListResult>.Ok(new PatientListResult
        {
            Items = items.Select(ToDto).ToList(),
            Total = total,
            Page = page,
            PageSize = pageSize
        });
    }

    public async Task<OperationResult<PatientDto>> GetByIdAsync(Guid id)
    {
        var patient = await _patientRepo.GetByIdAsync(id);
        if (patient is null) return OperationResult<PatientDto>.Fail("Patient not found.", 404);
        return OperationResult<PatientDto>.Ok(ToDto(patient));
    }

    public async Task<OperationResult<PatientDto>> CreateAsync(string branchId, CreatePatientRequest request)
    {
        var patient = new Patient
        {
            FirstName = request.FirstName,
            LastName = request.LastName,
            Phone = request.Phone,
            Email = request.Email,
            DateOfBirth = request.DateOfBirth,
            Gender = request.Gender,
            Address = request.Address,
            Allergies = request.Allergies,
            NhisNumber = request.NhisNumber,
            BranchId = branchId
        };

        await _patientRepo.AddAsync(patient);
        _logger.LogInformation("Patient {FullName} created with id {Id}", $"{patient.FirstName} {patient.LastName}", patient.Id);
        return OperationResult<PatientDto>.Ok(ToDto(patient));
    }

    public async Task<OperationResult<List<SaleDto>>> GetPurchaseHistoryAsync(Guid patientId)
    {
        var patientIdStr = patientId.ToString();
        var allSales = await _saleRepo.FindAsync(s => s.PatientId == patientIdStr);
        var dtos = allSales
            .OrderByDescending(s => s.CreatedAt)
            .Select(s => new SaleDto
            {
                Id = s.Id,
                ReceiptNo = s.ReceiptNo,
                PatientId = s.PatientId,
                Subtotal = s.Subtotal,
                Discount = s.Discount,
                Tax = s.Tax,
                Total = s.Total,
                PaymentMethod = s.PaymentMethod,
                CashierId = s.CashierId,
                Status = s.Status,
                CreatedAt = s.CreatedAt,
                Items = s.Items.Select(i => new SaleItemDto
                {
                    DrugId = i.DrugId,
                    DrugName = i.DrugName,
                    Quantity = i.Quantity,
                    UnitPrice = i.UnitPrice,
                    Subtotal = i.Subtotal
                }).ToList()
            }).ToList();
        return OperationResult<List<SaleDto>>.Ok(dtos);
    }

    private static PatientDto ToDto(Patient p) => new()
    {
        Id = p.Id,
        FirstName = p.FirstName,
        LastName = p.LastName,
        Phone = p.Phone,
        Email = p.Email,
        DateOfBirth = p.DateOfBirth,
        Gender = p.Gender,
        Address = p.Address,
        Allergies = p.Allergies,
        NhisNumber = p.NhisNumber,
        BranchId = p.BranchId,
        CreatedAt = p.CreatedAt
    };
}

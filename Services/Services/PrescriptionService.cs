using POS.API.Repositories.Interfaces;
using POS.API.Models;
using POS.API.Services.Contracts;

namespace POS.API.Services.Services;

public class PrescriptionService : IPrescriptionService
{
    private readonly IPrescriptionRepository _rxRepo;
    private readonly ILogger<PrescriptionService> _logger;

    public PrescriptionService(IPrescriptionRepository rxRepo, ILogger<PrescriptionService> logger)
    {
        _rxRepo = rxRepo;
        _logger = logger;
    }

    public async Task<OperationResult<PrescriptionListResult>> GetPrescriptionsAsync(string? status, string? query, int page, int pageSize, string branchId)
    {
        var (items, total) = await _rxRepo.GetPagedAsync(status, query, page, pageSize, branchId);
        return OperationResult<PrescriptionListResult>.Ok(new PrescriptionListResult
        {
            Items = items.Select(ToDto).ToList(),
            Total = total,
            Page = page,
            PageSize = pageSize
        });
    }

    public async Task<OperationResult<PrescriptionDto>> GetByIdAsync(Guid id)
    {
        var rx = await _rxRepo.GetByIdAsync(id);
        if (rx is null) return OperationResult<PrescriptionDto>.Fail("Prescription not found.", 404);
        return OperationResult<PrescriptionDto>.Ok(ToDto(rx));
    }

    public Task<int> CountByStatusAsync(string status, string branchId) =>
        _rxRepo.CountByStatusAsync(status, branchId);

    public async Task<OperationResult<PrescriptionDto>> CreateAsync(string branchId, CreatePrescriptionRequest request)
    {
        _logger.LogInformation("Creating prescription for patient {PatientName}, branch {BranchId}", request.PatientName, branchId);

        if (request.Lines.Count == 0)
            return OperationResult<PrescriptionDto>.Fail("Prescription must contain at least one drug.");

        var rx = new Prescription
        {
            RxNumber = $"RX-{DateTime.UtcNow:yyyyMMdd}-{Random.Shared.Next(100, 999)}",
            PatientName = request.PatientName,
            PatientId = request.PatientId,
            DoctorName = request.DoctorName,
            DoctorLicense = request.DoctorLicense,
            HospitalName = request.HospitalName,
            PrescribedDate = request.PrescribedDate == default ? DateTime.UtcNow : request.PrescribedDate,
            BranchId = branchId,
            Lines = request.Lines.Select(l => new PrescriptionLine
            {
                DrugName = l.DrugName,
                DrugId = l.DrugId,
                Dosage = l.Dosage,
                Quantity = l.Quantity,
                Instructions = l.Instructions
            }).ToList()
        };

        var saved = await _rxRepo.AddAsync(rx);
        _logger.LogInformation("Prescription {RxNumber} created", saved.RxNumber);
        return OperationResult<PrescriptionDto>.Ok(ToDto(saved));
    }

    public async Task<OperationResult> VerifyAsync(Guid id, string userId)
    {
        var rx = await _rxRepo.GetByIdAsync(id);
        if (rx is null) return OperationResult.Fail("Prescription not found.", 404);
        if (rx.Status != "Pending") return OperationResult.Fail("Only pending prescriptions can be verified.");

        rx.Status = "Verified";
        rx.VerifiedBy = userId;
        rx.VerifiedAt = DateTime.UtcNow;
        await _rxRepo.UpdateAsync(rx);
        _logger.LogInformation("Prescription {RxId} verified by {UserId}", id, userId);
        return OperationResult.Ok();
    }

    public async Task<OperationResult> DispenseAsync(Guid id, string userId)
    {
        var rx = await _rxRepo.GetByIdAsync(id);
        if (rx is null) return OperationResult.Fail("Prescription not found.", 404);
        if (rx.Status != "Verified") return OperationResult.Fail("Only verified prescriptions can be dispensed.");

        rx.Status = "Dispensed";
        rx.DispensedBy = userId;
        rx.DispensedAt = DateTime.UtcNow;
        await _rxRepo.UpdateAsync(rx);
        _logger.LogInformation("Prescription {RxId} dispensed by {UserId}", id, userId);
        return OperationResult.Ok();
    }

    public async Task<OperationResult> FlagAsync(Guid id, string reason)
    {
        var rx = await _rxRepo.GetByIdAsync(id);
        if (rx is null) return OperationResult.Fail("Prescription not found.", 404);
        if (rx.Status == "Dispensed") return OperationResult.Fail("Dispensed prescriptions cannot be flagged.");

        rx.Status = "Flagged";
        rx.FlagReason = reason;
        await _rxRepo.UpdateAsync(rx);
        _logger.LogInformation("Prescription {RxId} flagged: {Reason}", id, reason);
        return OperationResult.Ok();
    }

    private static PrescriptionDto ToDto(Prescription p) => new()
    {
        Id = p.Id,
        RxNumber = p.RxNumber,
        PatientName = p.PatientName,
        PatientId = p.PatientId,
        DoctorName = p.DoctorName,
        DoctorLicense = p.DoctorLicense,
        HospitalName = p.HospitalName,
        PrescribedDate = p.PrescribedDate,
        Status = p.Status,
        FlagReason = p.FlagReason,
        VerifiedBy = p.VerifiedBy,
        DispensedBy = p.DispensedBy,
        VerifiedAt = p.VerifiedAt,
        DispensedAt = p.DispensedAt,
        CreatedAt = p.CreatedAt,
        Lines = p.Lines.Select(l => new PrescriptionLineDto
        {
            Id = l.Id,
            DrugName = l.DrugName,
            DrugId = l.DrugId,
            Dosage = l.Dosage,
            Quantity = l.Quantity,
            Instructions = l.Instructions
        }).ToList()
    };
}

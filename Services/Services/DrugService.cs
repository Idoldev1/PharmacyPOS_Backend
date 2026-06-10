using POS.API.Repositories.Interfaces;
using POS.API.Models;
using POS.API.Services.Contracts;

namespace POS.API.Services.Services;

public class DrugService : IDrugService
{
    private readonly IDrugRepository _drugRepo;
    private readonly ILogger<DrugService> _logger;

    public DrugService(IDrugRepository drugRepo, ILogger<DrugService> logger)
    {
        _drugRepo = drugRepo;
        _logger = logger;
    }

    public async Task<OperationResult<DrugListResult>> GetDrugsAsync(string? query, string? category, int page, int pageSize, string branchId)
    {
        _logger.LogInformation("Fetching drugs for branch {BranchId}, query {Query}", branchId, query);
        var (items, total) = await _drugRepo.GetPagedAsync(query, category, page, pageSize, branchId);
        return OperationResult<DrugListResult>.Ok(new DrugListResult
        {
            Items = items.Select(ToDto).ToList(),
            Total = total,
            Page = page,
            PageSize = pageSize
        });
    }

    public async Task<OperationResult<List<DrugDto>>> GetLowStockAsync(string branchId)
    {
        _logger.LogInformation("Fetching low stock drugs for branch {BranchId}", branchId);
        var items = await _drugRepo.GetLowStockAsync(branchId);
        return OperationResult<List<DrugDto>>.Ok(items.Select(ToDto).ToList());
    }

    public async Task<OperationResult<DrugDto>> GetByIdAsync(Guid id)
    {
        var drug = await _drugRepo.GetByIdAsync(id);
        if (drug is null) return OperationResult<DrugDto>.Fail("Drug not found.", 404);
        return OperationResult<DrugDto>.Ok(ToDto(drug));
    }

    public async Task<OperationResult<DrugDto>> CreateAsync(string branchId, CreateDrugRequest request)
    {
        _logger.LogInformation("Creating drug {Name} for branch {BranchId}", request.Name, branchId);
        var drug = new Drug
        {
            Name = request.Name,
            GenericName = request.GenericName,
            Strength = request.Strength,
            Form = request.Form,
            Category = request.Category,
            BatchNo = request.BatchNo,
            ExpiryDate = request.ExpiryDate,
            StockQty = request.StockQty,
            ReorderLevel = request.ReorderLevel,
            UnitCost = request.UnitCost,
            SellingPrice = request.SellingPrice,
            NafdacNo = request.NafdacNo,
            SupplierId = request.SupplierId,
            BranchId = branchId
        };
        var saved = await _drugRepo.AddAsync(drug);
        return OperationResult<DrugDto>.Ok(ToDto(saved));
    }

    public async Task<OperationResult> UpdateStockAsync(Guid id, int quantity)
    {
        var drug = await _drugRepo.GetByIdAsync(id);
        if (drug is null) return OperationResult.Fail("Drug not found.", 404);
        drug.StockQty = quantity;
        drug.UpdatedAt = DateTime.UtcNow;
        await _drugRepo.UpdateAsync(drug);
        _logger.LogInformation("Stock updated for drug {DrugId}: qty {Quantity}", id, quantity);
        return OperationResult.Ok();
    }

    public async Task<OperationResult> DeleteAsync(Guid id)
    {
        var drug = await _drugRepo.GetByIdAsync(id);
        if (drug is null) return OperationResult.Fail("Drug not found.", 404);
        drug.IsActive = false;
        drug.UpdatedAt = DateTime.UtcNow;
        await _drugRepo.UpdateAsync(drug);
        return OperationResult.Ok();
    }

    internal static DrugDto ToDto(Drug d) => new()
    {
        Id = d.Id,
        Name = d.Name,
        GenericName = d.GenericName,
        Strength = d.Strength,
        Form = d.Form,
        Category = d.Category,
        BatchNo = d.BatchNo,
        ExpiryDate = d.ExpiryDate,
        StockQty = d.StockQty,
        ReorderLevel = d.ReorderLevel,
        UnitCost = d.UnitCost,
        SellingPrice = d.SellingPrice,
        NafdacNo = d.NafdacNo,
        SupplierId = d.SupplierId,
        BranchId = d.BranchId,
        Status = ComputeStatus(d),
        IsActive = d.IsActive,
        CreatedAt = d.CreatedAt
    };

    private static string ComputeStatus(Drug d)
    {
        if (d.StockQty == 0) return "out_of_stock";
        if (d.ExpiryDate.HasValue && d.ExpiryDate.Value <= DateTime.UtcNow.AddMonths(3)) return "near_expiry";
        if (d.StockQty <= d.ReorderLevel) return "low_stock";
        return "in_stock";
    }
}

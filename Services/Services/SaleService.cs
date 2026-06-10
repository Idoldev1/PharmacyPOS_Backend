using POS.API.Repositories.Interfaces;
using POS.API.Models;
using POS.API.Services.Contracts;

namespace POS.API.Services.Services;

public class SaleService : ISaleService
{
    private readonly ISaleRepository _saleRepo;
    private readonly IDrugRepository _drugRepo;
    private readonly ILogger<SaleService> _logger;

    public SaleService(ISaleRepository saleRepo, IDrugRepository drugRepo, ILogger<SaleService> logger)
    {
        _saleRepo = saleRepo;
        _drugRepo = drugRepo;
        _logger = logger;
    }

    public async Task<OperationResult<SaleDto>> CreateSaleAsync(string cashierId, string branchId, CreateSaleRequest request)
    {
        _logger.LogInformation("Creating sale for cashier {CashierId}, branch {BranchId}", cashierId, branchId);

        if (request.Items.Count == 0)
            return OperationResult<SaleDto>.Fail("Sale must contain at least one item.");

        var saleItems = new List<SaleItem>();
        decimal subtotal = 0;

        foreach (var item in request.Items)
        {
            if (item.Quantity <= 0)
                return OperationResult<SaleDto>.Fail("Quantity must be greater than zero.");

            var drug = await _drugRepo.GetByIdAsync(item.DrugId);
            if (drug is null)
                return OperationResult<SaleDto>.Fail($"Drug not found.", 404);
            if (!drug.IsActive)
                return OperationResult<SaleDto>.Fail($"'{drug.Name}' is not available.");
            if (drug.StockQty < item.Quantity)
                return OperationResult<SaleDto>.Fail($"Insufficient stock for '{drug.Name}'. Available: {drug.StockQty}.");

            var lineTotal = drug.SellingPrice * item.Quantity;
            subtotal += lineTotal;
            saleItems.Add(new SaleItem
            {
                DrugId = drug.Id,
                DrugName = drug.Name,
                Quantity = item.Quantity,
                UnitPrice = drug.SellingPrice,
                Subtotal = lineTotal
            });

            drug.StockQty -= item.Quantity;
            drug.UpdatedAt = DateTime.UtcNow;
            await _drugRepo.UpdateAsync(drug);
        }

        var discount = Math.Min(Math.Max(request.Discount, 0), subtotal);
        var sale = new Sale
        {
            ReceiptNo = $"RCP-{DateTime.UtcNow:yyyyMMdd}-{Random.Shared.Next(1000, 9999)}",
            PatientId = request.PatientId,
            CashierId = cashierId,
            BranchId = branchId,
            PaymentMethod = request.PaymentMethod,
            Subtotal = subtotal,
            Discount = discount,
            Tax = 0,
            Total = subtotal - discount,
            Items = saleItems,
            Status = "completed"
        };

        var saved = await _saleRepo.AddAsync(sale);
        _logger.LogInformation("Sale created: {ReceiptNo}, total ₦{Total}", saved.ReceiptNo, saved.Total);
        return OperationResult<SaleDto>.Ok(ToDto(saved));
    }

    public async Task<OperationResult<SaleDto>> GetByIdAsync(Guid id)
    {
        var sale = await _saleRepo.GetByIdAsync(id);
        if (sale is null) return OperationResult<SaleDto>.Fail("Sale not found.", 404);
        return OperationResult<SaleDto>.Ok(ToDto(sale));
    }

    public async Task<OperationResult<SaleListResult>> GetSalesAsync(int page, int pageSize, string branchId)
    {
        var items = await _saleRepo.GetPagedAsync(page, pageSize, branchId);
        var total = await _saleRepo.GetTotalCountAsync(branchId);
        return OperationResult<SaleListResult>.Ok(new SaleListResult
        {
            Items = items.Select(ToDto).ToList(),
            Total = total,
            Page = page,
            PageSize = pageSize
        });
    }

    public async Task<OperationResult<List<SaleDto>>> GetTodaySalesAsync(string branchId)
    {
        var sales = await _saleRepo.GetTodaySalesAsync(branchId);
        return OperationResult<List<SaleDto>>.Ok(sales.Select(ToDto).ToList());
    }

    public async Task<OperationResult> RefundAsync(Guid id)
    {
        var sale = await _saleRepo.GetByIdAsync(id);
        if (sale is null) return OperationResult.Fail("Sale not found.", 404);
        if (sale.Status == "refunded") return OperationResult.Fail("This sale has already been refunded.");

        foreach (var item in sale.Items)
        {
            var drug = await _drugRepo.GetByIdAsync(item.DrugId);
            if (drug is not null)
            {
                drug.StockQty += item.Quantity;
                drug.UpdatedAt = DateTime.UtcNow;
                await _drugRepo.UpdateAsync(drug);
            }
        }

        sale.Status = "refunded";
        await _saleRepo.UpdateAsync(sale);
        _logger.LogInformation("Sale {SaleId} refunded", id);
        return OperationResult.Ok();
    }

    private static SaleDto ToDto(Sale s) => new()
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
    };
}

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using POS.API.Data;
using POS.API.Repositories.Interfaces;
using POS.API.Models;
using POS.API.Services.Contracts;

namespace POS.API.Services.Services;

public class SaleService : ISaleService
{
    private readonly AppDbContext _context;
    private readonly ISaleRepository _saleRepo;
    private readonly IPendingSaleRepository _pendingSaleRepo;
    private readonly IDrugRepository _drugRepo;
    private readonly SalesSettings _salesSettings;
    private readonly ILogger<SaleService> _logger;

    public SaleService(
        AppDbContext context,
        ISaleRepository saleRepo,
        IPendingSaleRepository pendingSaleRepo,
        IDrugRepository drugRepo,
        IOptions<SalesSettings> salesSettings,
        ILogger<SaleService> logger)
    {
        _context = context;
        _saleRepo = saleRepo;
        _pendingSaleRepo = pendingSaleRepo;
        _drugRepo = drugRepo;
        _salesSettings = salesSettings.Value;
        _logger = logger;
    }

    public async Task<OperationResult<PendingSaleDto>> InitiateSaleAsync(string pharmacistId, string branchId, InitiateSaleRequest request)
    {
        _logger.LogInformation("Initiating sale for pharmacist {PharmacistId}, branch {BranchId}", pharmacistId, branchId);

        if (request.Items.Count == 0)
            return OperationResult<PendingSaleDto>.Fail("Sale must contain at least one item.");

        foreach (var item in request.Items)
        {
            if (item.Quantity <= 0)
                return OperationResult<PendingSaleDto>.Fail("Quantity must be greater than zero.");
        }

        var shortages = new List<string>();
        var pendingItems = new List<PendingSaleItem>();
        decimal subtotal = 0;

        foreach (var item in request.Items)
        {
            var drug = await _drugRepo.GetByIdAsync(item.DrugId);
            if (drug is null)
                return OperationResult<PendingSaleDto>.Fail("Drug not found.", 404);
            if (!drug.IsActive)
                return OperationResult<PendingSaleDto>.Fail($"'{drug.Name}' is not available.");

            var available = drug.StockQty - drug.ReservedQty;
            if (available < item.Quantity)
                shortages.Add($"{drug.Name} (available: {available}, requested: {item.Quantity})");

            var lineTotal = drug.SellingPrice * item.Quantity;
            subtotal += lineTotal;
            pendingItems.Add(new PendingSaleItem
            {
                DrugId = drug.Id,
                DrugName = drug.Name,
                BrandName = drug.Brand.Name,
                Quantity = item.Quantity,
                UnitPrice = drug.SellingPrice,
                Subtotal = lineTotal
            });
        }

        if (shortages.Count > 0)
            return OperationResult<PendingSaleDto>.Fail($"Insufficient available stock for: {string.Join(", ", shortages)}.");

        var discount = Math.Min(Math.Max(request.Discount, 0), subtotal);
        var pendingSale = new PendingSale
        {
            Code = await GenerateUniqueCodeAsync(),
            PatientId = request.PatientId,
            InitiatedByUserId = pharmacistId,
            BranchId = branchId,
            Subtotal = subtotal,
            Discount = discount,
            Tax = 0,
            Total = subtotal - discount,
            Items = pendingItems,
            ExpiresAt = DateTime.UtcNow.AddMinutes(_salesSettings.PendingSaleExpiryMinutes)
        };

        // Reservations are applied via ExecuteUpdateAsync (a single atomic UPDATE ... WHERE
        // per drug) rather than read-then-write, so concurrent pharmacists initiating sales
        // against the same drug can't both succeed in reserving more than is available.
        try
        {
            var strategy = _context.Database.CreateExecutionStrategy();
            await strategy.ExecuteAsync(async () =>
            {
                using var transaction = await _context.Database.BeginTransactionAsync();

                foreach (var item in request.Items)
                {
                    var affected = await _context.Set<Drug>()
                        .Where(d => d.Id == item.DrugId && d.StockQty - d.ReservedQty >= item.Quantity)
                        .ExecuteUpdateAsync(setters => setters
                            .SetProperty(d => d.ReservedQty, d => d.ReservedQty + item.Quantity)
                            .SetProperty(d => d.UpdatedAt, DateTime.UtcNow));

                    if (affected == 0)
                        throw new InvalidOperationException("Stock availability changed while initiating this sale. Please try again.");
                }

                await _pendingSaleRepo.AddAsync(pendingSale);
                await transaction.CommitAsync();
            });
        }
        catch (InvalidOperationException ex)
        {
            return OperationResult<PendingSaleDto>.Fail(ex.Message);
        }

        _logger.LogInformation("Pending sale {Code} initiated by {PharmacistId}, total ₦{Total}", pendingSale.Code, pharmacistId, pendingSale.Total);
        return OperationResult<PendingSaleDto>.Ok(ToPendingDto(pendingSale));
    }

    public async Task<OperationResult<PendingSaleDto>> GetPendingSaleByCodeAsync(string code)
    {
        var pending = await _pendingSaleRepo.GetByCodeAsync(code);
        if (pending is null)
            return OperationResult<PendingSaleDto>.Fail("Invalid or already-used code.", 404);

        if (pending.ExpiresAt < DateTime.UtcNow)
        {
            await ReleaseReservationAsync(pending, "Expired");
            return OperationResult<PendingSaleDto>.Fail("This code has expired.", 410);
        }

        return OperationResult<PendingSaleDto>.Ok(ToPendingDto(pending));
    }

    public async Task<OperationResult<List<PendingSaleDto>>> GetActivePendingSalesAsync(string branchId)
    {
        var sales = await _pendingSaleRepo.GetActiveByBranchAsync(branchId);
        return OperationResult<List<PendingSaleDto>>.Ok(sales.Select(ToPendingDto).ToList());
    }

    public async Task<OperationResult<SaleDto>> CompleteSaleAsync(string cashierId, string code, CompleteSaleRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.PaymentMethod))
            return OperationResult<SaleDto>.Fail("Payment method is required.");

        var pending = await _pendingSaleRepo.GetByCodeAsync(code);
        if (pending is null)
            return OperationResult<SaleDto>.Fail("Invalid or already-used code.", 404);

        if (pending.ExpiresAt < DateTime.UtcNow)
        {
            await ReleaseReservationAsync(pending, "Expired");
            return OperationResult<SaleDto>.Fail("This code has expired.", 410);
        }

        var sale = new Sale
        {
            ReceiptNo = $"RCP-{DateTime.UtcNow:yyyyMMdd}-{Random.Shared.Next(1000, 9999)}",
            PatientId = pending.PatientId,
            CashierId = cashierId,
            BranchId = pending.BranchId,
            PaymentMethod = request.PaymentMethod,
            Subtotal = pending.Subtotal,
            Discount = pending.Discount,
            Tax = pending.Tax,
            Total = pending.Total,
            Status = "Completed",
            Items = pending.Items.Select(i => new SaleItem
            {
                DrugId = i.DrugId,
                DrugName = i.DrugName,
                BrandName = i.BrandName,
                Quantity = i.Quantity,
                UnitPrice = i.UnitPrice,
                Subtotal = i.Subtotal
            }).ToList()
        };

        var strategy = _context.Database.CreateExecutionStrategy();
        await strategy.ExecuteAsync(async () =>
        {
            using var transaction = await _context.Database.BeginTransactionAsync();

            foreach (var item in pending.Items)
            {
                await _context.Set<Drug>()
                    .Where(d => d.Id == item.DrugId)
                    .ExecuteUpdateAsync(setters => setters
                        .SetProperty(d => d.StockQty, d => d.StockQty - item.Quantity)
                        .SetProperty(d => d.ReservedQty, d => d.ReservedQty - item.Quantity)
                        .SetProperty(d => d.UpdatedAt, DateTime.UtcNow));
            }

            await _saleRepo.AddAsync(sale);

            pending.Status = "Completed";
            await _pendingSaleRepo.UpdateAsync(pending);

            await transaction.CommitAsync();
        });

        _logger.LogInformation("Sale {ReceiptNo} completed by cashier {CashierId} from code {Code}", sale.ReceiptNo, cashierId, code);
        var cashier = await _context.Users.FindAsync(cashierId);
        var patientNames = await GetPatientNamesAsync([sale.PatientId]);
        return OperationResult<SaleDto>.Ok(ToDto(sale, cashier?.FirstName, sale.PatientId is not null ? patientNames.GetValueOrDefault(sale.PatientId) : null));
    }

    public async Task<OperationResult> CancelPendingSaleAsync(Guid id)
    {
        var pending = await _pendingSaleRepo.GetByIdAsync(id);
        if (pending is null || pending.Status != "Pending")
            return OperationResult.Fail("Pending sale not found or already finalized.", 404);

        await ReleaseReservationAsync(pending, "Cancelled");
        _logger.LogInformation("Pending sale {Code} cancelled", pending.Code);
        return OperationResult.Ok();
    }

    public async Task ExpireDueSalesAsync()
    {
        var expired = await _pendingSaleRepo.GetExpiredAsync(DateTime.UtcNow);
        foreach (var sale in expired)
            await ReleaseReservationAsync(sale, "Expired");
    }

    private async Task ReleaseReservationAsync(PendingSale sale, string newStatus)
    {
        var strategy = _context.Database.CreateExecutionStrategy();
        await strategy.ExecuteAsync(async () =>
        {
            using var transaction = await _context.Database.BeginTransactionAsync();

            foreach (var item in sale.Items)
            {
                await _context.Set<Drug>()
                    .Where(d => d.Id == item.DrugId)
                    .ExecuteUpdateAsync(setters => setters
                        .SetProperty(d => d.ReservedQty, d => d.ReservedQty - item.Quantity)
                        .SetProperty(d => d.UpdatedAt, DateTime.UtcNow));
            }

            sale.Status = newStatus;
            await _pendingSaleRepo.UpdateAsync(sale);

            await transaction.CommitAsync();
        });

        _logger.LogInformation("Pending sale {Code} reservation released, status {Status}", sale.Code, newStatus);
    }

    private async Task<string> GenerateUniqueCodeAsync()
    {
        // The filtered unique index on PendingSales.Code (Status = 'pending') is the
        // authoritative backstop against collisions; this loop just avoids the common case.
        for (var attempt = 0; attempt < 5; attempt++)
        {
            var code = Random.Shared.Next(100000, 999999).ToString();
            var existing = await _pendingSaleRepo.GetByCodeAsync(code);
            if (existing is null) return code;
        }

        throw new InvalidOperationException("Failed to generate a unique sale code. Please try again.");
    }

    public async Task<OperationResult<SaleDto>> GetByIdAsync(Guid id)
    {
        var sale = await _saleRepo.GetByIdAsync(id);
        if (sale is null) return OperationResult<SaleDto>.Fail("Sale not found.", 404);
        var cashierFirstNames = await GetCashierFirstNamesAsync([sale.CashierId]);
        var patientNames = await GetPatientNamesAsync([sale.PatientId]);
        return OperationResult<SaleDto>.Ok(ToDto(sale, cashierFirstNames.GetValueOrDefault(sale.CashierId), sale.PatientId is not null ? patientNames.GetValueOrDefault(sale.PatientId) : null));
    }

    public async Task<OperationResult<SaleListResult>> GetSalesAsync(int page, int pageSize, string branchId)
    {
        var items = await _saleRepo.GetPagedAsync(page, pageSize, branchId);
        var total = await _saleRepo.GetTotalCountAsync(branchId);
        var cashierFirstNames = await GetCashierFirstNamesAsync(items.Select(s => s.CashierId));
        var patientNames = await GetPatientNamesAsync(items.Select(s => s.PatientId));
        return OperationResult<SaleListResult>.Ok(new SaleListResult
        {
            Items = items.Select(s => ToDto(
                s,
                cashierFirstNames.GetValueOrDefault(s.CashierId),
                s.PatientId is not null ? patientNames.GetValueOrDefault(s.PatientId) : null)).ToList(),
            Total = total,
            Page = page,
            PageSize = pageSize
        });
    }

    public async Task<OperationResult<List<SaleDto>>> GetTodaySalesAsync(string branchId)
    {
        var sales = await _saleRepo.GetTodaySalesAsync(branchId);
        var cashierFirstNames = await GetCashierFirstNamesAsync(sales.Select(s => s.CashierId));
        var patientNames = await GetPatientNamesAsync(sales.Select(s => s.PatientId));
        return OperationResult<List<SaleDto>>.Ok(sales.Select(s => ToDto(
            s,
            cashierFirstNames.GetValueOrDefault(s.CashierId),
            s.PatientId is not null ? patientNames.GetValueOrDefault(s.PatientId) : null)).ToList());
    }

    private async Task<Dictionary<string, string>> GetCashierFirstNamesAsync(IEnumerable<string> cashierIds)
    {
        var ids = cashierIds.Distinct().ToList();
        return await _context.Users
            .Where(u => ids.Contains(u.Id))
            .ToDictionaryAsync(u => u.Id, u => u.FirstName);
    }

    private async Task<Dictionary<string, string>> GetPatientNamesAsync(IEnumerable<string?> patientIds)
    {
        var ids = patientIds
            .Where(id => !string.IsNullOrWhiteSpace(id) && Guid.TryParse(id, out _))
            .Select(id => Guid.Parse(id!))
            .Distinct()
            .ToList();

        if (ids.Count == 0) return new Dictionary<string, string>();

        return await _context.Patients
            .Where(p => ids.Contains(p.Id))
            .ToDictionaryAsync(p => p.Id.ToString(), p => $"{p.FirstName} {p.LastName}");
    }

    public async Task<OperationResult> RefundAsync(Guid id)
    {
        var sale = await _saleRepo.GetByIdAsync(id);
        if (sale is null) return OperationResult.Fail("Sale not found.", 404);
        if (sale.Status == "Refunded") return OperationResult.Fail("This sale has already been refunded.");

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

        sale.Status = "Refunded";
        await _saleRepo.UpdateAsync(sale);
        _logger.LogInformation("Sale {SaleId} refunded", id);
        return OperationResult.Ok();
    }

    private static PendingSaleDto ToPendingDto(PendingSale p) => new()
    {
        Id = p.Id,
        Code = p.Code,
        PatientId = p.PatientId,
        Subtotal = p.Subtotal,
        Discount = p.Discount,
        Tax = p.Tax,
        Total = p.Total,
        Status = p.Status,
        CreatedAt = p.CreatedAt,
        ExpiresAt = p.ExpiresAt,
        Items = p.Items.Select(i => new PendingSaleItemDto
        {
            DrugId = i.DrugId,
            DrugName = i.DrugName,
            BrandName = i.BrandName,
            Quantity = i.Quantity,
            UnitPrice = i.UnitPrice,
            Subtotal = i.Subtotal
        }).ToList()
    };

    private static SaleDto ToDto(Sale s, string? cashierFirstName = null, string? patientName = null) => new()
    {
        Id = s.Id,
        ReceiptNo = s.ReceiptNo,
        PatientId = s.PatientId,
        PatientName = patientName,
        Subtotal = s.Subtotal,
        Discount = s.Discount,
        Tax = s.Tax,
        Total = s.Total,
        PaymentMethod = s.PaymentMethod,
        CashierId = s.CashierId,
        CashierFirstName = cashierFirstName,
        Status = s.Status,
        CreatedAt = s.CreatedAt,
        Items = s.Items.Select(i => new SaleItemDto
        {
            DrugId = i.DrugId,
            DrugName = i.DrugName,
            BrandName = i.BrandName,
            Quantity = i.Quantity,
            UnitPrice = i.UnitPrice,
            Subtotal = i.Subtotal
        }).ToList()
    };
}

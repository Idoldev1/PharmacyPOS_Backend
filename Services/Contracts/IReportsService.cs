using POS.API.Models;

namespace POS.API.Services.Contracts;

public interface IReportsService
{
    Task<OperationResult<ReportsSummary>> GetSummaryAsync(string branchId);
    Task<OperationResult<List<WeeklyRevenuePoint>>> GetWeeklyRevenueAsync(string branchId);
    Task<OperationResult<List<PaymentMethodPoint>>> GetPaymentBreakdownAsync(string branchId);
    Task<OperationResult<List<TopDrugEntry>>> GetTopDrugsAsync(string branchId);
}

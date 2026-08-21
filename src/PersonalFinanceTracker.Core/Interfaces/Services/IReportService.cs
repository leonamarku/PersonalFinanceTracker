using PersonalFinanceTracker.Core.Common;
using PersonalFinanceTracker.Core.DTOs.Report;

namespace PersonalFinanceTracker.Core.Interfaces.Services;

public interface IReportService
{
    /// <summary>Income vs expense totals + net balance for a given month.</summary>
    Task<Result<MonthlySummaryDto>> GetMonthlySummaryAsync(int userId, int month, int year);

    /// <summary>Spending grouped by category for a given month.</summary>
    Task<Result<IEnumerable<CategoryBreakdownDto>>> GetCategoryBreakdownAsync(int userId, int month, int year);

    /// <summary>Each budget's limit vs actual spend + over-budget flag.</summary>
    Task<Result<IEnumerable<BudgetStatusDto>>> GetBudgetStatusAsync(int userId, int month, int year);
}

using PersonalFinanceTracker.Core.Common;
using PersonalFinanceTracker.Core.DTOs.Report;
using PersonalFinanceTracker.Core.Enums;
using PersonalFinanceTracker.Core.Interfaces.Repositories;
using PersonalFinanceTracker.Core.Interfaces.Services;

namespace PersonalFinanceTracker.Infrastructure.Services;

/// <summary>
/// Produces read-only analytics reports from posted transaction data.
/// All methods are pure reads — no database writes occur here.
/// </summary>
public class ReportService : IReportService
{
    private readonly ITransactionRepository _txRepo;
    private readonly IBudgetRepository      _budgetRepo;

    public ReportService(ITransactionRepository txRepo, IBudgetRepository budgetRepo)
    {
        _txRepo     = txRepo;
        _budgetRepo = budgetRepo;
    }

    /// <summary>
    /// Monthly summary: total income, total expenses, and net balance for a given month.
    /// Net = Income - Expenses. A positive net means the user saved money that month.
    /// </summary>
    public async Task<Result<MonthlySummaryDto>> GetMonthlySummaryAsync(int userId, int month, int year)
    {
        var transactions = (await _txRepo.GetByUserAndMonthAsync(userId, month, year)).ToList();

        var income   = transactions.Where(t => t.Type == TransactionType.Income)
                                   .Sum(t => t.Amount);
        var expenses = transactions.Where(t => t.Type == TransactionType.Expense)
                                   .Sum(t => t.Amount);

        return Result<MonthlySummaryDto>.Success(new MonthlySummaryDto
        {
            Month            = month,
            Year             = year,
            TotalIncome      = income,
            TotalExpenses    = expenses,
            NetBalance       = income - expenses,
            TransactionCount = transactions.Count
        });
    }

    /// <summary>
    /// Category breakdown: expense totals grouped by category for a given month,
    /// including each category's percentage of total monthly spending.
    /// </summary>
    public async Task<Result<IEnumerable<CategoryBreakdownDto>>> GetCategoryBreakdownAsync(
        int userId, int month, int year)
    {
        var transactions = (await _txRepo.GetByUserAndMonthAsync(userId, month, year))
            .Where(t => t.Type == TransactionType.Expense)
            .ToList();

        var totalExpenses = transactions.Sum(t => t.Amount);

        var breakdown = transactions
            .GroupBy(t => new { t.CategoryId, t.Category.Name, t.Category.Color })
            .Select(g => new CategoryBreakdownDto
            {
                CategoryId   = g.Key.CategoryId,
                CategoryName = g.Key.Name,
                Color        = g.Key.Color,
                TotalAmount  = g.Sum(t => t.Amount),
                Count        = g.Count(),
                Percentage   = totalExpenses > 0
                    ? Math.Round(g.Sum(t => t.Amount) / totalExpenses * 100, 1)
                    : 0
            })
            .OrderByDescending(b => b.TotalAmount)
            .ToList();

        return Result<IEnumerable<CategoryBreakdownDto>>.Success(breakdown);
    }

    /// <summary>
    /// Budget status: for each budget set in the given month, compares the limit
    /// against actual spending and flags categories that have exceeded their budget.
    /// This is the key "over-budget alert" business rule.
    /// </summary>
    public async Task<Result<IEnumerable<BudgetStatusDto>>> GetBudgetStatusAsync(
        int userId, int month, int year)
    {
        var budgets      = (await _budgetRepo.GetByUserAndMonthAsync(userId, month, year)).ToList();
        var transactions = (await _txRepo.GetByUserAndMonthAsync(userId, month, year))
            .Where(t => t.Type == TransactionType.Expense)
            .ToList();

        var result = budgets.Select(b =>
        {
            var spent     = transactions.Where(t => t.CategoryId == b.CategoryId)
                                        .Sum(t => t.Amount);
            var remaining = b.LimitAmount - spent;
            var pct       = b.LimitAmount > 0
                ? Math.Round(spent / b.LimitAmount * 100, 1)
                : 0;

            return new BudgetStatusDto
            {
                BudgetId        = b.Id,
                CategoryId      = b.CategoryId,
                CategoryName    = b.Category.Name,
                CategoryColor   = b.Category.Color,
                LimitAmount     = b.LimitAmount,
                SpentAmount     = spent,
                RemainingAmount = remaining,
                IsOverBudget    = spent >= b.LimitAmount,
                PercentUsed     = pct
            };
        })
        .OrderByDescending(s => s.PercentUsed)
        .ToList();

        return Result<IEnumerable<BudgetStatusDto>>.Success(result);
    }
}

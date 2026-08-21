using Microsoft.AspNetCore.Mvc;
using PersonalFinanceTracker.Core.DTOs.Report;
using PersonalFinanceTracker.Core.Interfaces.Services;

namespace PersonalFinanceTracker.API.Controllers;

/// <summary>
/// Read-only analytics endpoints.
/// All reports are scoped to the authenticated user and a specific month/year.
/// </summary>
[Route("api/reports")]
public class ReportsController : ApiControllerBase
{
    private readonly IReportService _service;

    public ReportsController(IReportService service) => _service = service;

    /// <summary>
    /// Monthly summary: total income, total expenses, net balance, and transaction count.
    /// </summary>
    [HttpGet("summary/{year:int}/{month:int}")]
    [ProducesResponseType(typeof(MonthlySummaryDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetMonthlySummary(int year, int month)
    {
        var result = await _service.GetMonthlySummaryAsync(CurrentUserId, month, year);
        return Ok(result.Data);
    }

    /// <summary>
    /// Category breakdown: expense totals per category for the month,
    /// including percentage of total spending.
    /// </summary>
    [HttpGet("categories/{year:int}/{month:int}")]
    [ProducesResponseType(typeof(IEnumerable<CategoryBreakdownDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetCategoryBreakdown(int year, int month)
    {
        var result = await _service.GetCategoryBreakdownAsync(CurrentUserId, month, year);
        return Ok(result.Data);
    }

    /// <summary>
    /// Budget status: for each active budget, shows limit vs spent, remaining amount,
    /// percentage used, and an over-budget flag.
    /// </summary>
    [HttpGet("budgets/{year:int}/{month:int}")]
    [ProducesResponseType(typeof(IEnumerable<BudgetStatusDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetBudgetStatus(int year, int month)
    {
        var result = await _service.GetBudgetStatusAsync(CurrentUserId, month, year);
        return Ok(result.Data);
    }
}

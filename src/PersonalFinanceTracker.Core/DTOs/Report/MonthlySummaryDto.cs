namespace PersonalFinanceTracker.Core.DTOs.Report;

public class MonthlySummaryDto
{
    public int     Month          { get; set; }
    public int     Year           { get; set; }
    public decimal TotalIncome    { get; set; }
    public decimal TotalExpenses  { get; set; }
    public decimal NetBalance     { get; set; } // Income - Expenses
    public int     TransactionCount { get; set; }
}

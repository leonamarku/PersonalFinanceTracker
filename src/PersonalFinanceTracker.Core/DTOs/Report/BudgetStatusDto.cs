namespace PersonalFinanceTracker.Core.DTOs.Report;

public class BudgetStatusDto
{
    public int     BudgetId      { get; set; }
    public int     CategoryId    { get; set; }
    public string  CategoryName  { get; set; } = string.Empty;
    public string  CategoryColor { get; set; } = string.Empty;
    public decimal LimitAmount   { get; set; }
    public decimal SpentAmount   { get; set; }
    public decimal RemainingAmount { get; set; }
    /// <summary>True when SpentAmount >= LimitAmount.</summary>
    public bool    IsOverBudget  { get; set; }
    /// <summary>Percentage of the budget consumed (0–100+).</summary>
    public decimal PercentUsed   { get; set; }
}

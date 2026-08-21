namespace PersonalFinanceTracker.Core.DTOs.Budget;

public class BudgetDto
{
    public int     Id           { get; set; }
    public int     CategoryId   { get; set; }
    public string  CategoryName { get; set; } = string.Empty;
    public string  CategoryColor { get; set; } = string.Empty;
    public int     Month        { get; set; }
    public int     Year         { get; set; }
    public decimal LimitAmount  { get; set; }
}

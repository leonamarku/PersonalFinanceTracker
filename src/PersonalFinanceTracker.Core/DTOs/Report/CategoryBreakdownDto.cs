namespace PersonalFinanceTracker.Core.DTOs.Report;

public class CategoryBreakdownDto
{
    public int     CategoryId   { get; set; }
    public string  CategoryName { get; set; } = string.Empty;
    public string  Color        { get; set; } = string.Empty;
    public decimal TotalAmount  { get; set; }
    public int     Count        { get; set; }
    /// <summary>Percentage of total expenses for the month.</summary>
    public decimal Percentage   { get; set; }
}

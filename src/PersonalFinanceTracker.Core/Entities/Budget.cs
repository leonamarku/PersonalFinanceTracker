namespace PersonalFinanceTracker.Core.Entities;

/// <summary>Monthly spending limit set by a user for a specific expense category.</summary>
public class Budget
{
    public int     Id          { get; set; }
    public int     UserId      { get; set; }
    public int     CategoryId  { get; set; }
    public int     Month       { get; set; } // 1-12
    public int     Year        { get; set; }
    public decimal LimitAmount { get; set; }

    // Navigation properties
    public User     User     { get; set; } = null!;
    public Category Category { get; set; } = null!;
}

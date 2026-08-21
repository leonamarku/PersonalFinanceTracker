using PersonalFinanceTracker.Core.Enums;

namespace PersonalFinanceTracker.Core.Entities;

/// <summary>
/// Transaction category. Global categories (IsGlobal = true) are created by Admins
/// and shared across all users; personal categories belong to a single user.
/// </summary>
public class Category
{
    public int     Id       { get; set; }
    public string  Name     { get; set; } = string.Empty;
    public TransactionType Type { get; set; }
    public string  Color    { get; set; } = "#6366f1"; // default indigo
    public bool    IsGlobal { get; set; } = false;
    public int?    UserId   { get; set; }

    // Navigation properties
    public User?                    User         { get; set; }
    public ICollection<Transaction> Transactions { get; set; } = new List<Transaction>();
    public ICollection<Budget>      Budgets      { get; set; } = new List<Budget>();
}

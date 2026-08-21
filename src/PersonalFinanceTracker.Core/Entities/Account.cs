using PersonalFinanceTracker.Core.Enums;

namespace PersonalFinanceTracker.Core.Entities;

/// <summary>A financial account owned by a user (bank account, wallet, etc.).</summary>
public class Account
{
    public int         Id       { get; set; }
    public int         UserId   { get; set; }
    public string      Name     { get; set; } = string.Empty;
    public AccountType Type     { get; set; }
    public decimal     Balance  { get; set; }
    public string      Currency { get; set; } = "USD";
    public DateTime    CreatedAt { get; set; } = DateTime.UtcNow;

    // Navigation properties
    public User                   User         { get; set; } = null!;
    public ICollection<Transaction> Transactions { get; set; } = new List<Transaction>();
}

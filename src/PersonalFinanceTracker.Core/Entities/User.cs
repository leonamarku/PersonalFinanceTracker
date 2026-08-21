namespace PersonalFinanceTracker.Core.Entities;

/// <summary>Application user. Role is either "Admin" or "User".</summary>
public class User
{
    public int    Id           { get; set; }
    public string FullName     { get; set; } = string.Empty;
    public string Email        { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public string Role         { get; set; } = "User"; // "Admin" | "User"
    public DateTime CreatedAt  { get; set; } = DateTime.UtcNow;

    // Navigation properties
    public ICollection<Account>     Accounts    { get; set; } = new List<Account>();
    public ICollection<Category>    Categories  { get; set; } = new List<Category>();
    public ICollection<Budget>      Budgets     { get; set; } = new List<Budget>();
}

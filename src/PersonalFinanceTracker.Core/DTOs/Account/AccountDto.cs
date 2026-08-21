using PersonalFinanceTracker.Core.Enums;

namespace PersonalFinanceTracker.Core.DTOs.Account;

public class AccountDto
{
    public int         Id        { get; set; }
    public string      Name      { get; set; } = string.Empty;
    public AccountType Type      { get; set; }
    public string      TypeName  { get; set; } = string.Empty;
    public decimal     Balance   { get; set; }
    public string      Currency  { get; set; } = string.Empty;
    public DateTime    CreatedAt { get; set; }
}

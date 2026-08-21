using System.ComponentModel.DataAnnotations;
using PersonalFinanceTracker.Core.Enums;

namespace PersonalFinanceTracker.Core.DTOs.Account;

public class UpdateAccountDto
{
    [Required, MaxLength(100)]
    public string Name { get; set; } = string.Empty;

    [Required]
    public AccountType Type { get; set; }

    [MaxLength(3)]
    public string Currency { get; set; } = "USD";
}

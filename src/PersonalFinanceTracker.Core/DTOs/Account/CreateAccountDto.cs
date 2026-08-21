using System.ComponentModel.DataAnnotations;
using PersonalFinanceTracker.Core.Enums;

namespace PersonalFinanceTracker.Core.DTOs.Account;

public class CreateAccountDto
{
    [Required, MaxLength(100)]
    public string Name { get; set; } = string.Empty;

    [Required]
    public AccountType Type { get; set; }

    [Range(0, double.MaxValue, ErrorMessage = "Balance must be non-negative.")]
    public decimal Balance { get; set; } = 0;

    [MaxLength(3)]
    public string Currency { get; set; } = "USD";
}

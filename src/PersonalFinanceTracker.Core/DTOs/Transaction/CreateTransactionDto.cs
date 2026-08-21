using System.ComponentModel.DataAnnotations;
using PersonalFinanceTracker.Core.Enums;

namespace PersonalFinanceTracker.Core.DTOs.Transaction;

public class CreateTransactionDto
{
    [Required]
    public int AccountId { get; set; }

    [Required]
    public int CategoryId { get; set; }

    [Required, Range(0.01, double.MaxValue, ErrorMessage = "Amount must be greater than zero.")]
    public decimal Amount { get; set; }

    [Required]
    public TransactionType Type { get; set; }

    [Required]
    public DateTime Date { get; set; }

    [Required, MaxLength(200)]
    public string Description { get; set; } = string.Empty;

    [MaxLength(500)]
    public string? Notes { get; set; }
}

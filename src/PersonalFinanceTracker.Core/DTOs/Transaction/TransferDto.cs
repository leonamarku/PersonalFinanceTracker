using System.ComponentModel.DataAnnotations;

namespace PersonalFinanceTracker.Core.DTOs.Transaction;

/// <summary>Request payload for a transfer between two accounts owned by the same user.</summary>
public class TransferDto
{
    [Required]
    public int FromAccountId { get; set; }

    [Required]
    public int ToAccountId { get; set; }

    [Required, Range(0.01, double.MaxValue, ErrorMessage = "Amount must be greater than zero.")]
    public decimal Amount { get; set; }

    [Required]
    public DateTime Date { get; set; }

    [MaxLength(200)]
    public string Description { get; set; } = "Transfer";

    [MaxLength(500)]
    public string? Notes { get; set; }
}

using PersonalFinanceTracker.Core.Enums;

namespace PersonalFinanceTracker.Core.Entities;

/// <summary>
/// A single financial movement on an account.
/// Transfers are represented as two transactions linked by TransferPairId.
/// </summary>
public class Transaction
{
    public int             Id             { get; set; }
    public int             AccountId      { get; set; }
    public int             CategoryId     { get; set; }
    public decimal         Amount         { get; set; }
    public TransactionType Type           { get; set; }
    public DateTime        Date           { get; set; }
    public string          Description    { get; set; } = string.Empty;
    public string?         Notes          { get; set; }
    /// <summary>Links the two legs of an inter-account transfer.</summary>
    public int?            TransferPairId { get; set; }
    public DateTime        CreatedAt      { get; set; } = DateTime.UtcNow;

    // Navigation properties
    public Account  Account  { get; set; } = null!;
    public Category Category { get; set; } = null!;
}

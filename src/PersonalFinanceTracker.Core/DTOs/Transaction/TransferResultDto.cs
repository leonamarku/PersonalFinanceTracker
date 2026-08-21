namespace PersonalFinanceTracker.Core.DTOs.Transaction;

/// <summary>The two transactions created by a transfer operation.</summary>
public class TransferResultDto
{
    public TransactionDto Outgoing { get; set; } = null!;
    public TransactionDto Incoming { get; set; } = null!;
}

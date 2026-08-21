using PersonalFinanceTracker.Core.Enums;

namespace PersonalFinanceTracker.Core.DTOs.Transaction;

public class TransactionDto
{
    public int             Id           { get; set; }
    public int             AccountId    { get; set; }
    public string          AccountName  { get; set; } = string.Empty;
    public int             CategoryId   { get; set; }
    public string          CategoryName { get; set; } = string.Empty;
    public string          CategoryColor { get; set; } = string.Empty;
    public decimal         Amount       { get; set; }
    public TransactionType Type         { get; set; }
    public string          TypeName     { get; set; } = string.Empty;
    public DateTime        Date         { get; set; }
    public string          Description  { get; set; } = string.Empty;
    public string?         Notes        { get; set; }
    public int?            TransferPairId { get; set; }
    public DateTime        CreatedAt    { get; set; }
}

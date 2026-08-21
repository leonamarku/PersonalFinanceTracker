using PersonalFinanceTracker.Core.Enums;

namespace PersonalFinanceTracker.Core.DTOs.Category;

public class CategoryDto
{
    public int             Id       { get; set; }
    public string          Name     { get; set; } = string.Empty;
    public TransactionType Type     { get; set; }
    public string          TypeName { get; set; } = string.Empty;
    public string          Color    { get; set; } = string.Empty;
    public bool            IsGlobal { get; set; }
}

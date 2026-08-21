using System.ComponentModel.DataAnnotations;
using PersonalFinanceTracker.Core.Enums;

namespace PersonalFinanceTracker.Core.DTOs.Category;

public class CreateCategoryDto
{
    [Required, MaxLength(100)]
    public string Name { get; set; } = string.Empty;

    [Required]
    public TransactionType Type { get; set; }

    [RegularExpression("^#[0-9A-Fa-f]{6}$", ErrorMessage = "Color must be a valid hex code.")]
    public string Color { get; set; } = "#6366f1";

    /// <summary>Only Admins may create global categories.</summary>
    public bool IsGlobal { get; set; } = false;
}

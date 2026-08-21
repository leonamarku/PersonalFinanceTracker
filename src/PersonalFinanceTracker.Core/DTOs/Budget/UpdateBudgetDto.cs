using System.ComponentModel.DataAnnotations;

namespace PersonalFinanceTracker.Core.DTOs.Budget;

public class UpdateBudgetDto
{
    [Required, Range(0.01, double.MaxValue, ErrorMessage = "Limit must be greater than zero.")]
    public decimal LimitAmount { get; set; }
}

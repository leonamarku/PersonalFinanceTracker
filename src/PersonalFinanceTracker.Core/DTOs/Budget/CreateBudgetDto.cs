using System.ComponentModel.DataAnnotations;

namespace PersonalFinanceTracker.Core.DTOs.Budget;

public class CreateBudgetDto
{
    [Required]
    public int CategoryId { get; set; }

    [Required, Range(1, 12)]
    public int Month { get; set; }

    [Required, Range(2000, 2100)]
    public int Year { get; set; }

    [Required, Range(0.01, double.MaxValue, ErrorMessage = "Limit must be greater than zero.")]
    public decimal LimitAmount { get; set; }
}

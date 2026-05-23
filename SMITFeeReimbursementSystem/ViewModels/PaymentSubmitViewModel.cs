using System.ComponentModel.DataAnnotations;
using SMITFeeReimbursementSystem.Models;

namespace SMITFeeReimbursementSystem.ViewModels;

public class PaymentSubmitViewModel
{
    [Required]
    [Display(Name = "Course")]
    public int CourseId { get; set; }

    [Required]
    [Range(0, double.MaxValue)]
    public decimal Amount { get; set; } = Payment.DefaultAmount;

    [Required]
    [StringLength(100)]
    [Display(Name = "Transaction Id")]
    public string TransactionId { get; set; } = string.Empty;

    [Required]
    [Display(Name = "Payment Screenshot")]
    public IFormFile? Screenshot { get; set; }
}

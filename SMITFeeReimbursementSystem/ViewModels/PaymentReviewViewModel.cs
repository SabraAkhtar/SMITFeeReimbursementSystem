using System.ComponentModel.DataAnnotations;
using SMITFeeReimbursementSystem.Models;

namespace SMITFeeReimbursementSystem.ViewModels;

public class PaymentReviewViewModel
{
    public int Id { get; set; }
    public string StudentName { get; set; } = string.Empty;
    public string StudentEmail { get; set; } = string.Empty;
    public string CourseName { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public string TransactionId { get; set; } = string.Empty;
    public string ScreenshotPath { get; set; } = string.Empty;
    public PaymentStatus Status { get; set; }
    public DateTime SubmittedAt { get; set; }

    [StringLength(500)]
    [Display(Name = "Admin Remarks")]
    public string? AdminRemarks { get; set; }
}

public class PaymentListItemViewModel
{
    public int Id { get; set; }
    public string StudentName { get; set; } = string.Empty;
    public string CourseName { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public string TransactionId { get; set; } = string.Empty;
    public PaymentStatus Status { get; set; }
    public DateTime SubmittedAt { get; set; }
    public string? ReceiptNumber { get; set; }
    public string? AdminRemarks { get; set; }
}

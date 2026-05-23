using System.ComponentModel.DataAnnotations;

namespace SMITFeeReimbursementSystem.Models;

public class Payment
{
    public const decimal DefaultAmount = 3000m;

    public int Id { get; set; }

    public string StudentId { get; set; } = string.Empty;
    public ApplicationUser Student { get; set; } = null!;

    public int CourseId { get; set; }
    public Course Course { get; set; } = null!;

    [Range(0, double.MaxValue)]
    public decimal Amount { get; set; } = DefaultAmount;

    [Required]
    [StringLength(100)]
    [Display(Name = "Transaction Id")]
    public string TransactionId { get; set; } = string.Empty;

    [Required]
    [StringLength(500)]
    public string ScreenshotPath { get; set; } = string.Empty;

    public PaymentStatus Status { get; set; } = PaymentStatus.Pending;

    [StringLength(500)]
    public string? AdminRemarks { get; set; }

    public DateTime SubmittedAt { get; set; } = DateTime.UtcNow;
    public DateTime? ReviewedAt { get; set; }
    public string? ReviewedById { get; set; }
    public ApplicationUser? ReviewedBy { get; set; }

    public Receipt? Receipt { get; set; }
}

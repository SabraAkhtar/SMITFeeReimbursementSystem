using System.ComponentModel.DataAnnotations;

namespace SMITFeeReimbursementSystem.Models;

public class Refund
{
    public const decimal EligibilityThreshold = 80m;

    public int RefundId { get; set; }

    public string StudentId { get; set; } = string.Empty;
    public ApplicationUser Student { get; set; } = null!;

    public int CourseId { get; set; }
    public Course Course { get; set; } = null!;

    [Range(0, 100)]
    public decimal AttendancePercentage { get; set; }

    public RefundStatus RefundStatus { get; set; } = RefundStatus.Pending;

    [StringLength(500)]
    public string? AdminRemarks { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? ReviewedAt { get; set; }
    public string? ReviewedById { get; set; }
    public ApplicationUser? ReviewedBy { get; set; }
}

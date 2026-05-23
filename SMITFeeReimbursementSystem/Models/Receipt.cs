using System.ComponentModel.DataAnnotations;

namespace SMITFeeReimbursementSystem.Models;

public class Receipt
{
    public int Id { get; set; }

    public int PaymentId { get; set; }
    public Payment Payment { get; set; } = null!;

    [StringLength(50)]
    public string ReceiptNumber { get; set; } = string.Empty;

    public string StudentId { get; set; } = string.Empty;
    public ApplicationUser Student { get; set; } = null!;

    [StringLength(150)]
    public string StudentName { get; set; } = string.Empty;

    [StringLength(50)]
    public string RollNumber { get; set; } = string.Empty;

    public int CourseId { get; set; }
    public Course Course { get; set; } = null!;

    [StringLength(150)]
    public string CourseName { get; set; } = string.Empty;

    public decimal Amount { get; set; }

    [StringLength(100)]
    public string TransactionId { get; set; } = string.Empty;

    public DateTime PaymentDate { get; set; }

    [StringLength(150)]
    public string ApprovedByName { get; set; } = string.Empty;

    public string? ApprovedById { get; set; }

    [StringLength(500)]
    public string? PdfFilePath { get; set; }

    public DateTime GeneratedAt { get; set; } = DateTime.UtcNow;
}

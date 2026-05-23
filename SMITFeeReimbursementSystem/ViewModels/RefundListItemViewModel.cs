using SMITFeeReimbursementSystem.Models;

namespace SMITFeeReimbursementSystem.ViewModels;

public class RefundListItemViewModel
{
    public int RefundId { get; set; }
    public string StudentName { get; set; } = string.Empty;
    public string CourseName { get; set; } = string.Empty;
    public decimal AttendancePercentage { get; set; }
    public RefundStatus RefundStatus { get; set; }
    public bool IsEligible { get; set; }
    public DateTime CreatedAt { get; set; }
    public string? AdminRemarks { get; set; }
}

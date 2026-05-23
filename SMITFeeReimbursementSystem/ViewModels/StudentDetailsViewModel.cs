using SMITFeeReimbursementSystem.Models;

namespace SMITFeeReimbursementSystem.ViewModels;

public class StudentDetailsViewModel
{
    public string Id { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string RollNumber { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? Phone { get; set; }
    public List<CourseEnrollment> Enrollments { get; set; } = [];
    public List<Payment> Payments { get; set; } = [];
    public List<Refund> Refunds { get; set; } = [];
}

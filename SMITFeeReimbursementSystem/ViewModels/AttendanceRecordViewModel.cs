using SMITFeeReimbursementSystem.Models;

namespace SMITFeeReimbursementSystem.ViewModels;

public class AttendanceRecordViewModel
{
    public int AttendanceId { get; set; }
    public string StudentName { get; set; } = string.Empty;
    public string CourseName { get; set; } = string.Empty;
    public DateOnly Date { get; set; }
    public AttendanceStatus Status { get; set; }
}

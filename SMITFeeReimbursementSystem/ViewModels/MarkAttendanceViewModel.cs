using System.ComponentModel.DataAnnotations;
using SMITFeeReimbursementSystem.Models;

namespace SMITFeeReimbursementSystem.ViewModels;

public class MarkAttendanceViewModel
{
    [Required]
    [Display(Name = "Course")]
    public int CourseId { get; set; }

    [Required]
    [DataType(DataType.Date)]
    public DateOnly Date { get; set; } = DateOnly.FromDateTime(DateTime.Today);

    public List<StudentAttendanceRow> Students { get; set; } = [];
}

public class StudentAttendanceRow
{
    public string StudentId { get; set; } = string.Empty;
    public string StudentName { get; set; } = string.Empty;
    public AttendanceStatus Status { get; set; } = AttendanceStatus.Present;
    public bool HasExistingRecord { get; set; }
}

using System.ComponentModel.DataAnnotations;

namespace SMITFeeReimbursementSystem.Models;

public class Attendance
{
    public int AttendanceId { get; set; }

    public string StudentId { get; set; } = string.Empty;
    public ApplicationUser Student { get; set; } = null!;

    public int CourseId { get; set; }
    public Course Course { get; set; } = null!;

    [DataType(DataType.Date)]
    public DateOnly Date { get; set; }

    public AttendanceStatus Status { get; set; }

    public string? MarkedById { get; set; }
    public ApplicationUser? MarkedBy { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}

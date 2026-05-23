using SMITFeeReimbursementSystem.Models;

namespace SMITFeeReimbursementSystem.Services;

public record AttendanceSummary(
    string StudentId,
    string StudentName,
    int CourseId,
    string CourseName,
    int TotalDays,
    int PresentDays,
    int AbsentDays,
    int LeaveDays,
    decimal AttendancePercentage);

public interface IAttendanceCalculationService
{
    Task<AttendanceSummary> GetSummaryAsync(string studentId, int courseId, int? year = null, int? month = null);
    Task<List<AttendanceSummary>> GetSummariesForCourseAsync(int courseId, int? year = null, int? month = null);
    Task<List<AttendanceSummary>> GetSummariesForStudentAsync(string studentId, int? courseId = null, int? year = null, int? month = null);
    decimal CalculatePercentage(int presentDays, int totalDays);
    bool IsEligibleForRefund(decimal attendancePercentage);
}

using Microsoft.EntityFrameworkCore;
using SMITFeeReimbursementSystem.Data;
using SMITFeeReimbursementSystem.Models;

namespace SMITFeeReimbursementSystem.Services;

public class AttendanceCalculationService(ApplicationDbContext context) : IAttendanceCalculationService
{
    public decimal CalculatePercentage(int presentDays, int totalDays)
    {
        if (totalDays == 0)
        {
            return 0;
        }

        return Math.Round((decimal)presentDays / totalDays * 100, 2);
    }

    public bool IsEligibleForRefund(decimal attendancePercentage) =>
        attendancePercentage >= Refund.EligibilityThreshold;

    public async Task<AttendanceSummary> GetSummaryAsync(string studentId, int courseId, int? year = null, int? month = null)
    {
        var records = await GetFilteredRecordsAsync(studentId, courseId, year, month);
        var student = await context.Users.FindAsync(studentId);
        var course = await context.Courses.FindAsync(courseId);

        return BuildSummary(
            studentId,
            student?.FullName ?? student?.Email ?? studentId,
            courseId,
            course?.CourseName ?? "",
            records);
    }

    public async Task<List<AttendanceSummary>> GetSummariesForCourseAsync(int courseId, int? year = null, int? month = null)
    {
        var studentIds = await context.CourseEnrollments
            .Where(e => e.CourseId == courseId)
            .Select(e => e.StudentId)
            .ToListAsync();

        var summaries = new List<AttendanceSummary>();
        foreach (var studentId in studentIds)
        {
            summaries.Add(await GetSummaryAsync(studentId, courseId, year, month));
        }

        return summaries.OrderByDescending(s => s.AttendancePercentage).ToList();
    }

    public async Task<List<AttendanceSummary>> GetSummariesForStudentAsync(
        string studentId, int? courseId = null, int? year = null, int? month = null)
    {
        var courseIds = courseId.HasValue
            ? [courseId.Value]
            : await context.CourseEnrollments
                .Where(e => e.StudentId == studentId)
                .Select(e => e.CourseId)
                .ToListAsync();

        var summaries = new List<AttendanceSummary>();
        foreach (var id in courseIds)
        {
            summaries.Add(await GetSummaryAsync(studentId, id, year, month));
        }

        return summaries;
    }

    private async Task<List<Attendance>> GetFilteredRecordsAsync(
        string studentId, int courseId, int? year, int? month)
    {
        var query = context.Attendances
            .Where(a => a.StudentId == studentId && a.CourseId == courseId);

        if (year.HasValue && month.HasValue)
        {
            query = query.Where(a => a.Date.Year == year.Value && a.Date.Month == month.Value);
        }
        else if (year.HasValue)
        {
            query = query.Where(a => a.Date.Year == year.Value);
        }

        return await query.ToListAsync();
    }

    private AttendanceSummary BuildSummary(
        string studentId,
        string studentName,
        int courseId,
        string courseName,
        List<Attendance> records)
    {
        var total = records.Count;
        var present = records.Count(r => r.Status == AttendanceStatus.Present);
        var absent = records.Count(r => r.Status == AttendanceStatus.Absent);
        var leave = records.Count(r => r.Status == AttendanceStatus.Leave);

        return new AttendanceSummary(
            studentId,
            studentName,
            courseId,
            courseName,
            total,
            present,
            absent,
            leave,
            CalculatePercentage(present, total));
    }
}

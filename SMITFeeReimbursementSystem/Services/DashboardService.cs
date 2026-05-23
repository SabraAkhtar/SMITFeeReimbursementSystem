using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using SMITFeeReimbursementSystem.Data;
using SMITFeeReimbursementSystem.Models;
using SMITFeeReimbursementSystem.ViewModels;

namespace SMITFeeReimbursementSystem.Services;

public class DashboardService(
    ApplicationDbContext context,
    RoleManager<IdentityRole> roleManager) : IDashboardService
{
    public async Task<DashboardViewModel> GetDashboardDataAsync()
    {
        // DbContext is not thread-safe — run sequentially, not in parallel
        var stats = await LoadStatsAsync();
        var charts = await LoadChartsAsync();
        var activity = await LoadRecentActivityAsync();

        return new DashboardViewModel
        {
            Stats = stats,
            Charts = charts,
            RecentActivity = activity
        };
    }

    private async Task<DashboardStatsViewModel> LoadStatsAsync()
    {
        var studentRoleId = await roleManager.Roles
            .Where(r => r.Name == AppRoles.Student)
            .Select(r => r.Id)
            .FirstOrDefaultAsync();

        var totalStudents = studentRoleId is null
            ? 0
            : await context.UserRoles.CountAsync(ur => ur.RoleId == studentRoleId);

        var paymentCounts = await context.Payments
            .GroupBy(p => p.Status)
            .Select(g => new { Status = g.Key, Count = g.Count() })
            .ToListAsync();

        var stats = new DashboardStatsViewModel
        {
            TotalStudents = totalStudents,
            TotalCourses = await context.Courses.CountAsync(),
            TotalPayments = await context.Payments.CountAsync(),
            PendingPayments = paymentCounts.FirstOrDefault(x => x.Status == PaymentStatus.Pending)?.Count ?? 0,
            ApprovedPayments = paymentCounts.FirstOrDefault(x => x.Status == PaymentStatus.Approved)?.Count ?? 0,
            RejectedPayments = paymentCounts.FirstOrDefault(x => x.Status == PaymentStatus.Rejected)?.Count ?? 0,
            TotalAttendanceRecords = await context.Attendances.CountAsync(),
            RefundEligibleStudents = await context.Refunds
                .CountAsync(r => r.AttendancePercentage >= Refund.EligibilityThreshold)
        };

        return stats;
    }

    private async Task<DashboardChartsViewModel> LoadChartsAsync()
    {
        var paymentStatus = await context.Payments
            .GroupBy(p => p.Status)
            .Select(g => new { Status = g.Key, Count = g.Count() })
            .ToListAsync();

        var enrollment = await context.CourseEnrollments
            .GroupBy(e => e.CourseId)
            .Select(g => new { CourseId = g.Key, Count = g.Count() })
            .ToListAsync();

        var courseIds = enrollment.Select(e => e.CourseId).ToList();
        var courseNames = await context.Courses
            .Where(c => courseIds.Contains(c.Id))
            .ToDictionaryAsync(c => c.Id, c => c.CourseName);

        var attendanceStats = await context.Attendances
            .GroupBy(a => a.CourseId)
            .Select(g => new
            {
                CourseId = g.Key,
                Total = g.Count(),
                Present = g.Count(a => a.Status == AttendanceStatus.Present)
            })
            .ToListAsync();

        var attendanceByCourse = attendanceStats
            .Select(a => (
                Name: courseNames.GetValueOrDefault(a.CourseId, $"Course {a.CourseId}"),
                Percentage: a.Total == 0 ? 0m : Math.Round((decimal)a.Present / a.Total * 100, 2)))
            .ToList();

        return new DashboardChartsViewModel
        {
            PaymentStatus = new ChartDatasetViewModel
            {
                Labels = ["Pending", "Approved", "Rejected"],
                Values =
                [
                    paymentStatus.FirstOrDefault(x => x.Status == PaymentStatus.Pending)?.Count ?? 0,
                    paymentStatus.FirstOrDefault(x => x.Status == PaymentStatus.Approved)?.Count ?? 0,
                    paymentStatus.FirstOrDefault(x => x.Status == PaymentStatus.Rejected)?.Count ?? 0
                ],
                Colors = ["#ffc107", "#198754", "#dc3545"]
            },
            AttendanceByCourse = new ChartDatasetViewModel
            {
                Labels = attendanceByCourse.Select(x => x.Name).ToList(),
                Values = attendanceByCourse.Select(x => x.Percentage).ToList(),
                Colors = ["#0d6efd", "#6610f2", "#20c997", "#fd7e14", "#6f42c1"]
            },
            EnrollmentByCourse = new ChartDatasetViewModel
            {
                Labels = enrollment.Select(e => courseNames.GetValueOrDefault(e.CourseId, $"Course {e.CourseId}")).ToList(),
                Values = enrollment.Select(e => (decimal)e.Count).ToList(),
                Colors = ["#0dcaf0", "#198754", "#ffc107", "#d63384", "#6c757d"]
            }
        };
    }

    private async Task<DashboardActivityViewModel> LoadRecentActivityAsync()
    {
        var latestPayments = await context.Payments
            .AsNoTracking()
            .OrderByDescending(p => p.SubmittedAt)
            .Take(5)
            .Select(p => new RecentPaymentViewModel
            {
                Id = p.Id,
                StudentName = p.Student.FullName ?? p.Student.Email ?? "",
                CourseName = p.Course.CourseName,
                Amount = p.Amount,
                Status = p.Status.ToString(),
                SubmittedAt = p.SubmittedAt
            })
            .ToListAsync();

        var latestEnrollments = await context.CourseEnrollments
            .AsNoTracking()
            .OrderByDescending(e => e.EnrolledAt)
            .Take(5)
            .Select(e => new RecentEnrollmentViewModel
            {
                StudentName = e.Student.FullName ?? e.Student.Email ?? "",
                CourseName = e.Course.CourseName,
                EnrolledAt = e.EnrolledAt
            })
            .ToListAsync();

        var recentAttendance = await context.Attendances
            .AsNoTracking()
            .OrderByDescending(a => a.CreatedAt)
            .Take(5)
            .Select(a => new RecentAttendanceViewModel
            {
                StudentName = a.Student.FullName ?? a.Student.Email ?? "",
                CourseName = a.Course.CourseName,
                Status = a.Status.ToString(),
                Date = a.Date
            })
            .ToListAsync();

        return new DashboardActivityViewModel
        {
            LatestPayments = latestPayments,
            LatestEnrollments = latestEnrollments,
            RecentAttendance = recentAttendance
        };
    }
}

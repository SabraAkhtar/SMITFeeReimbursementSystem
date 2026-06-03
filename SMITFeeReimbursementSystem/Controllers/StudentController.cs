using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SMITFeeReimbursementSystem.Data;
using SMITFeeReimbursementSystem.Models;
using SMITFeeReimbursementSystem.Services;

namespace SMITFeeReimbursementSystem.Controllers;

[Authorize(Roles = AppRoles.Student)]
public class StudentController(
    ApplicationDbContext context,
    UserManager<ApplicationUser> userManager,
    IAttendanceCalculationService attendanceCalculation) : Controller
{
    public async Task<IActionResult> Dashboard()
    {
        var user = await userManager.GetUserAsync(User);
        if (user is null) return Challenge();

        // Payments summary
        var payments = await context.Payments
            .Include(p => p.Course)
            .Include(p => p.Receipt)
            .Where(p => p.StudentId == user.Id)
            .OrderByDescending(p => p.SubmittedAt)
            .ToListAsync();

        // Refunds
        var refunds = await context.Refunds
            .Include(r => r.Course)
            .Where(r => r.StudentId == user.Id)
            .ToListAsync();

        // Enrollments
        var enrollments = await context.CourseEnrollments
            .Include(e => e.Course)
            .Where(e => e.StudentId == user.Id)
            .ToListAsync();

        // Attendance summaries for all enrolled courses
        var attendanceSummaries = new List<AttendanceSummary>();
        foreach (var enrollment in enrollments)
        {
            var summary = await attendanceCalculation.GetSummaryAsync(
                user.Id, enrollment.CourseId);
            attendanceSummaries.Add(summary);
        }

        ViewBag.User = user;
        ViewBag.Payments = payments;
        ViewBag.Refunds = refunds;
        ViewBag.Enrollments = enrollments;
        ViewBag.AttendanceSummaries = attendanceSummaries;
        ViewBag.EligibilityThreshold = Refund.EligibilityThreshold;

        return View();
    }
}

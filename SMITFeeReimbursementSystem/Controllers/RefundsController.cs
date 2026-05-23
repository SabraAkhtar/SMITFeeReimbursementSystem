using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SMITFeeReimbursementSystem.Data;
using SMITFeeReimbursementSystem.Models;
using SMITFeeReimbursementSystem.Services;
using SMITFeeReimbursementSystem.ViewModels;

namespace SMITFeeReimbursementSystem.Controllers;

[Authorize]
public class RefundsController(
    ApplicationDbContext context,
    UserManager<ApplicationUser> userManager,
    IRefundEligibilityService refundEligibility,
    IAttendanceCalculationService attendanceCalculation) : Controller
{
    [Authorize(Roles = AppRoles.Admin)]
    public async Task<IActionResult> Index(string? search, string? status)
    {
        await refundEligibility.SyncEligibleRefundsAsync();

        var query = context.Refunds
            .Include(r => r.Student)
            .Include(r => r.Course)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
        {
            query = query.Where(r =>
                (r.Student.FullName != null && r.Student.FullName.Contains(search)) ||
                (r.Student.Email != null && r.Student.Email.Contains(search)) ||
                r.Course.CourseName.Contains(search));
        }

        if (!string.IsNullOrWhiteSpace(status) && Enum.TryParse<RefundStatus>(status, true, out var refundStatus))
        {
            query = query.Where(r => r.RefundStatus == refundStatus);
        }

        var refunds = await query
            .OrderByDescending(r => r.AttendancePercentage)
            .Select(r => new RefundListItemViewModel
            {
                RefundId = r.RefundId,
                StudentName = r.Student.FullName ?? r.Student.Email ?? "",
                CourseName = r.Course.CourseName,
                AttendancePercentage = r.AttendancePercentage,
                RefundStatus = r.RefundStatus,
                IsEligible = r.AttendancePercentage >= Refund.EligibilityThreshold,
                CreatedAt = r.CreatedAt,
                AdminRemarks = r.AdminRemarks
            })
            .ToListAsync();

        ViewBag.Search = search;
        ViewBag.Status = status;
        ViewBag.EligibilityThreshold = Refund.EligibilityThreshold;
        return View(refunds);
    }

    [Authorize(Roles = AppRoles.Admin)]
    public async Task<IActionResult> Eligible(string? search)
    {
        await refundEligibility.SyncEligibleRefundsAsync();

        var eligible = await context.Refunds
            .Include(r => r.Student)
            .Include(r => r.Course)
            .Where(r => r.AttendancePercentage >= Refund.EligibilityThreshold)
            .ToListAsync();

        if (!string.IsNullOrWhiteSpace(search))
        {
            eligible = eligible.Where(r =>
                (r.Student.FullName?.Contains(search, StringComparison.OrdinalIgnoreCase) ?? false) ||
                (r.Student.Email?.Contains(search, StringComparison.OrdinalIgnoreCase) ?? false) ||
                r.Course.CourseName.Contains(search, StringComparison.OrdinalIgnoreCase)).ToList();
        }

        ViewBag.Search = search;
        ViewBag.EligibilityThreshold = Refund.EligibilityThreshold;
        return View(eligible);
    }

    [Authorize(Roles = AppRoles.Admin)]
    public async Task<IActionResult> Review(int id)
    {
        var refund = await context.Refunds
            .Include(r => r.Student)
            .Include(r => r.Course)
            .FirstOrDefaultAsync(r => r.RefundId == id);

        if (refund is null)
        {
            return NotFound();
        }

        return View(refund);
    }

    [HttpPost]
    [Authorize(Roles = AppRoles.Admin)]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Approve(int id, string? adminRemarks)
    {
        var refund = await GetRefundAsync(id);
        if (refund is null)
        {
            return NotFound();
        }

        if (refund.AttendancePercentage < Refund.EligibilityThreshold)
        {
            TempData["StatusMessage"] = $"Refund requires at least {Refund.EligibilityThreshold}% attendance.";
            return RedirectToAction(nameof(Review), new { id });
        }

        var admin = await userManager.GetUserAsync(User);
        refund.RefundStatus = RefundStatus.Approved;
        refund.AdminRemarks = adminRemarks;
        refund.ReviewedAt = DateTime.UtcNow;
        refund.ReviewedById = admin?.Id;

        await context.SaveChangesAsync();
        TempData["StatusMessage"] = "Refund approved successfully.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [Authorize(Roles = AppRoles.Admin)]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Reject(int id, string? adminRemarks)
    {
        var refund = await GetRefundAsync(id);
        if (refund is null)
        {
            return NotFound();
        }

        var admin = await userManager.GetUserAsync(User);
        refund.RefundStatus = RefundStatus.Rejected;
        refund.AdminRemarks = adminRemarks;
        refund.ReviewedAt = DateTime.UtcNow;
        refund.ReviewedById = admin?.Id;

        await context.SaveChangesAsync();
        TempData["StatusMessage"] = "Refund rejected.";
        return RedirectToAction(nameof(Index));
    }

    [Authorize(Roles = AppRoles.Student)]
    public async Task<IActionResult> MyRefunds(string? search)
    {
        var user = await userManager.GetUserAsync(User);
        if (user is null)
        {
            return Challenge();
        }

        await SyncStudentRefundAsync(user.Id);

        var query = context.Refunds
            .Include(r => r.Course)
            .Where(r => r.StudentId == user.Id);

        if (!string.IsNullOrWhiteSpace(search))
        {
            query = query.Where(r => r.Course.CourseName.Contains(search));
        }

        var refunds = await query.OrderByDescending(r => r.CreatedAt).ToListAsync();

        ViewBag.Search = search;
        ViewBag.EligibilityThreshold = Refund.EligibilityThreshold;

        foreach (var refund in refunds)
        {
            var summary = await attendanceCalculation.GetSummaryAsync(user.Id, refund.CourseId);
            ViewData[$"Summary_{refund.RefundId}"] = summary;
        }

        return View(refunds);
    }

    private async Task SyncStudentRefundAsync(string studentId)
    {
        var enrollments = await context.CourseEnrollments
            .Where(e => e.StudentId == studentId)
            .ToListAsync();

        foreach (var enrollment in enrollments)
        {
            var summary = await attendanceCalculation.GetSummaryAsync(
                enrollment.StudentId, enrollment.CourseId);

            if (!attendanceCalculation.IsEligibleForRefund(summary.AttendancePercentage))
            {
                continue;
            }

            var existing = await context.Refunds
                .FirstOrDefaultAsync(r =>
                    r.StudentId == enrollment.StudentId &&
                    r.CourseId == enrollment.CourseId);

            if (existing is null)
            {
                context.Refunds.Add(new Refund
                {
                    StudentId = enrollment.StudentId,
                    CourseId = enrollment.CourseId,
                    AttendancePercentage = summary.AttendancePercentage,
                    RefundStatus = RefundStatus.Pending
                });
            }
            else if (existing.RefundStatus == RefundStatus.Pending)
            {
                existing.AttendancePercentage = summary.AttendancePercentage;
            }
        }

        await context.SaveChangesAsync();
    }

    private async Task<Refund?> GetRefundAsync(int id) =>
        await context.Refunds.FirstOrDefaultAsync(r => r.RefundId == id);
}

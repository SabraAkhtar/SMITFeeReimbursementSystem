using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using SMITFeeReimbursementSystem.Data;
using SMITFeeReimbursementSystem.Models;
using SMITFeeReimbursementSystem.Services;
using SMITFeeReimbursementSystem.ViewModels;

namespace SMITFeeReimbursementSystem.Controllers;

[Authorize]
public class PaymentsController(
    ApplicationDbContext context,
    UserManager<ApplicationUser> userManager,
    IFileUploadService fileUploadService,
    IReceiptService receiptService,
    INotificationService notificationService) : Controller
{
    [Authorize(Roles = AppRoles.Student)]
    public async Task<IActionResult> MyPayments(string? search, string? status)
    {
        var user = await GetCurrentUserAsync();
        if (user is null)
        {
            return Challenge();
        }

        var query = context.Payments
            .Include(p => p.Course)
            .Include(p => p.Student)
            .Include(p => p.Receipt)
            .Where(p => p.StudentId == user.Id);

        query = ApplyFilters(query, search, status);

        var payments = await query
            .OrderByDescending(p => p.SubmittedAt)
            .Select(p => new PaymentListItemViewModel
            {
                Id = p.Id,
                StudentName = user.FullName ?? user.Email ?? "",
                CourseName = p.Course.CourseName,
                Amount = p.Amount,
                TransactionId = p.TransactionId,
                Status = p.Status,
                SubmittedAt = p.SubmittedAt,
                ReceiptNumber = p.Receipt != null ? p.Receipt.ReceiptNumber : null,
                AdminRemarks = p.AdminRemarks
            })
            .ToListAsync();

        ViewBag.Search = search;
        ViewBag.Status = status;
        return View(payments);
    }

    [Authorize(Roles = AppRoles.Student)]
    public async Task<IActionResult> Submit()
    {
        var user = await GetCurrentUserAsync();
        if (user is null) return Challenge();

        await PopulateCoursesAsync(user.Id);
        return View(new PaymentSubmitViewModel());
    }

    [HttpPost]
    [Authorize(Roles = AppRoles.Student)]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Submit(PaymentSubmitViewModel model)
    {
        var user = await GetCurrentUserAsync();
        if (user is null) return Challenge();

        await PopulateCoursesAsync(user.Id);

        if (!ModelState.IsValid) return View(model);

        var course = await context.Courses.FindAsync(model.CourseId);
        if (course is null)
        {
            ModelState.AddModelError(nameof(model.CourseId), "Invalid course selected.");
            return View(model);
        }

        // Auto-enroll student if not already enrolled
        var isEnrolled = await context.CourseEnrollments
            .AnyAsync(e => e.StudentId == user.Id && e.CourseId == model.CourseId);
        if (!isEnrolled)
        {
            context.CourseEnrollments.Add(new CourseEnrollment
            {
                StudentId = user.Id,
                CourseId = model.CourseId,
                EnrolledAt = DateTime.UtcNow
            });
            await context.SaveChangesAsync();
        }

        // Check if payment already submitted for this course
        var existingPayment = await context.Payments
            .AnyAsync(p => p.StudentId == user.Id && p.CourseId == model.CourseId
                        && p.Status != PaymentStatus.Rejected);
        if (existingPayment)
        {
            ModelState.AddModelError(nameof(model.CourseId), "You have already submitted a payment for this course.");
            return View(model);
        }

        try
        {
            var screenshotPath = await fileUploadService.SavePaymentScreenshotAsync(model.Screenshot!);

            var payment = new Payment
            {
                StudentId = user.Id,
                CourseId = model.CourseId,
                Amount = model.Amount > 0 ? model.Amount : course.FeeAmount,
                TransactionId = model.TransactionId.Trim(),
                ScreenshotPath = screenshotPath,
                Status = PaymentStatus.Pending,
                SubmittedAt = DateTime.UtcNow
            };

            context.Payments.Add(payment);
            await context.SaveChangesAsync();

            // Notify admin about new payment submission
            var studentName = user.FullName ?? user.Email ?? "A student";
            await notificationService.CreatePaymentSubmittedNotificationAsync(
                payment.Id, studentName, course.CourseName, payment.Amount);

            TempData["StatusMessage"] = "Payment submitted successfully. Awaiting admin approval.";
            return RedirectToAction(nameof(MyPayments));
        }
        catch (InvalidOperationException ex)
        {
            ModelState.AddModelError(nameof(model.Screenshot), ex.Message);
            return View(model);
        }
    }

    [Authorize(Roles = AppRoles.Admin)]
    public async Task<IActionResult> Index(string? search, string? status)
    {
        var query = context.Payments
            .Include(p => p.Student)
            .Include(p => p.Course)
            .Include(p => p.Receipt)
            .AsQueryable();

        query = ApplyFilters(query, search, status);

        var payments = await query
            .OrderByDescending(p => p.SubmittedAt)
            .Select(p => new PaymentListItemViewModel
            {
                Id = p.Id,
                StudentName = p.Student.FullName ?? p.Student.Email ?? "",
                CourseName = p.Course.CourseName,
                Amount = p.Amount,
                TransactionId = p.TransactionId,
                Status = p.Status,
                SubmittedAt = p.SubmittedAt,
                ReceiptNumber = p.Receipt != null ? p.Receipt.ReceiptNumber : null
            })
            .ToListAsync();

        ViewBag.Search = search;
        ViewBag.Status = status;
        return View(payments);
    }

    [Authorize(Roles = AppRoles.Admin)]
    public async Task<IActionResult> Review(int id)
    {
        var payment = await context.Payments
            .Include(p => p.Student)
            .Include(p => p.Course)
            .Include(p => p.Receipt)
            .FirstOrDefaultAsync(p => p.Id == id);

        if (payment is null)
        {
            return NotFound();
        }

        return View(new PaymentReviewViewModel
        {
            Id = payment.Id,
            StudentName = payment.Student.FullName ?? payment.Student.Email ?? "",
            StudentEmail = payment.Student.Email ?? "",
            CourseName = payment.Course.CourseName,
            Amount = payment.Amount,
            TransactionId = payment.TransactionId,
            ScreenshotPath = payment.ScreenshotPath,
            Status = payment.Status,
            SubmittedAt = payment.SubmittedAt,
            AdminRemarks = payment.AdminRemarks
        });
    }

    [HttpPost]
    [Authorize(Roles = AppRoles.Admin)]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Approve(int id, string? adminRemarks)
    {
        var payment = await GetPaymentForReviewAsync(id);
        if (payment is null)
        {
            return NotFound();
        }

        if (payment.Status != PaymentStatus.Pending)
        {
            TempData["StatusMessage"] = "Only pending payments can be approved.";
            return RedirectToAction(nameof(Review), new { id });
        }

        var admin = await GetCurrentUserAsync();
        payment.Status = PaymentStatus.Approved;
        payment.AdminRemarks = adminRemarks;
        payment.ReviewedAt = DateTime.UtcNow;
        payment.ReviewedById = admin?.Id;

        await context.SaveChangesAsync();
        await receiptService.GenerateReceiptAsync(id);

        // Notify student their payment was approved
        var course = await context.Courses.FindAsync(payment.CourseId);
        await notificationService.CreatePaymentApprovedNotificationAsync(
            id, payment.StudentId, course?.CourseName ?? "your course", payment.Amount);

        TempData["StatusMessage"] = $"Payment approved and PDF receipt generated for {payment.Student?.FullName ?? "student"}.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [Authorize(Roles = AppRoles.Admin)]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Reject(int id, string? adminRemarks)
    {
        var payment = await GetPaymentForReviewAsync(id);
        if (payment is null)
        {
            return NotFound();
        }

        if (payment.Status != PaymentStatus.Pending)
        {
            TempData["StatusMessage"] = "Only pending payments can be rejected.";
            return RedirectToAction(nameof(Review), new { id });
        }

        var admin = await GetCurrentUserAsync();
        payment.Status = PaymentStatus.Rejected;
        payment.AdminRemarks = adminRemarks;
        payment.ReviewedAt = DateTime.UtcNow;
        payment.ReviewedById = admin?.Id;

        await context.SaveChangesAsync();

        // Notify student their payment was rejected
        var course = await context.Courses.FindAsync(payment.CourseId);
        await notificationService.CreatePaymentRejectedNotificationAsync(
            id, payment.StudentId, course?.CourseName ?? "your course", adminRemarks);

        TempData["StatusMessage"] = "Payment rejected.";
        return RedirectToAction(nameof(Index));
    }

    [Authorize]
    public async Task<IActionResult> Receipt(int id)
    {
        var user = await GetCurrentUserAsync();
        if (user is null)
        {
            return Challenge();
        }

        var receipt = await context.Receipts
            .Include(r => r.Payment)
            .Include(r => r.Student)
            .Include(r => r.Course)
            .FirstOrDefaultAsync(r => r.PaymentId == id);

        if (receipt is null)
        {
            return NotFound();
        }

        var isAdmin = await userManager.IsInRoleAsync(user, AppRoles.Admin);
        if (!isAdmin && receipt.StudentId != user.Id)
        {
            return Forbid();
        }

        return View(receipt);
    }

    [Authorize]
    public async Task<IActionResult> DownloadPdf(int id)
    {
        var user = await GetCurrentUserAsync();
        if (user is null)
        {
            return Challenge();
        }

        var receipt = await receiptService.GetReceiptByPaymentIdAsync(id);
        if (receipt is null)
        {
            return NotFound();
        }

        var isAdmin = await userManager.IsInRoleAsync(user, AppRoles.Admin);
        if (!isAdmin && receipt.StudentId != user.Id)
        {
            return Forbid();
        }

        var pdf = await receiptService.GetReceiptPdfAsync(id);
        if (pdf is null || pdf.Length == 0)
        {
            return NotFound();
        }

        return File(pdf, "application/pdf", $"{receipt.ReceiptNumber}.pdf");
    }

    private async Task<Payment?> GetPaymentForReviewAsync(int id)
    {
        return await context.Payments
            .Include(p => p.Student)
            .Include(p => p.Receipt)
            .FirstOrDefaultAsync(p => p.Id == id);
    }

    private async Task<ApplicationUser?> GetCurrentUserAsync() =>
        await userManager.GetUserAsync(User);

    private async Task PopulateCoursesAsync(string studentId)
    {
        // Show all available courses (student auto-enrolls on payment submit)
        var allCourses = await context.Courses
            .OrderBy(c => c.CourseName)
            .ToListAsync();

        ViewBag.CourseId = new SelectList(allCourses, "Id", "CourseName");
    }

    private async Task PopulateEnrolledCoursesAsync(string studentId)
    {
        var courses = await context.CourseEnrollments
            .Where(e => e.StudentId == studentId)
            .Include(e => e.Course)
            .Select(e => e.Course)
            .OrderBy(c => c.CourseName)
            .ToListAsync();

        ViewBag.CourseId = new SelectList(courses, "Id", "CourseName");
    }

    private static IQueryable<Payment> ApplyFilters(IQueryable<Payment> query, string? search, string? status)
    {
        if (!string.IsNullOrWhiteSpace(search))
        {
            query = query.Where(p =>
                p.TransactionId.Contains(search) ||
                (p.Student.FullName != null && p.Student.FullName.Contains(search)) ||
                (p.Student.Email != null && p.Student.Email.Contains(search)) ||
                p.Course.CourseName.Contains(search));
        }

        if (!string.IsNullOrWhiteSpace(status) && Enum.TryParse<PaymentStatus>(status, true, out var paymentStatus))
        {
            query = query.Where(p => p.Status == paymentStatus);
        }

        return query;
    }
}

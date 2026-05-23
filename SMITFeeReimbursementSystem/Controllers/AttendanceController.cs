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
public class AttendanceController(
    ApplicationDbContext context,
    UserManager<ApplicationUser> userManager,
    IAttendanceCalculationService attendanceCalculation) : Controller
{
    [Authorize(Policy = "RequireTeacherOrAdmin")]
    public async Task<IActionResult> Index(int? courseId, string? studentId, string? search, DateOnly? fromDate, DateOnly? toDate)
    {
        await PopulateCourseFilterAsync(courseId);
        await PopulateStudentFilterAsync(studentId);

        var query = context.Attendances
            .Include(a => a.Student)
            .Include(a => a.Course)
            .AsQueryable();

        if (courseId.HasValue)
        {
            query = query.Where(a => a.CourseId == courseId.Value);
        }

        if (!string.IsNullOrEmpty(studentId))
        {
            query = query.Where(a => a.StudentId == studentId);
        }

        if (fromDate.HasValue)
        {
            query = query.Where(a => a.Date >= fromDate.Value);
        }

        if (toDate.HasValue)
        {
            query = query.Where(a => a.Date <= toDate.Value);
        }

        if (!string.IsNullOrWhiteSpace(search))
        {
            query = query.Where(a =>
                (a.Student.FullName != null && a.Student.FullName.Contains(search)) ||
                (a.Student.Email != null && a.Student.Email.Contains(search)) ||
                a.Course.CourseName.Contains(search));
        }

        var records = await query
            .OrderByDescending(a => a.Date)
            .Select(a => new AttendanceRecordViewModel
            {
                AttendanceId = a.AttendanceId,
                StudentName = a.Student.FullName ?? a.Student.Email ?? "",
                CourseName = a.Course.CourseName,
                Date = a.Date,
                Status = a.Status
            })
            .ToListAsync();

        ViewBag.Search = search;
        ViewBag.FromDate = fromDate?.ToString("yyyy-MM-dd");
        ViewBag.ToDate = toDate?.ToString("yyyy-MM-dd");
        return View(records);
    }

    [Authorize(Policy = "RequireTeacherOrAdmin")]
    public async Task<IActionResult> Mark(int? courseId, DateOnly? date)
    {
        var model = new MarkAttendanceViewModel
        {
            CourseId = courseId ?? 0,
            Date = date ?? DateOnly.FromDateTime(DateTime.Today)
        };

        await PopulateCourseFilterAsync(courseId);
        if (courseId.HasValue && courseId.Value > 0)
        {
            model.Students = await BuildStudentRowsAsync(courseId.Value, model.Date);
        }

        return View(model);
    }

    [HttpPost]
    [Authorize(Policy = "RequireTeacherOrAdmin")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Mark(MarkAttendanceViewModel model)
    {
        await PopulateCourseFilterAsync(model.CourseId);

        if (model.CourseId <= 0)
        {
            ModelState.AddModelError(nameof(model.CourseId), "Please select a course.");
            return View(model);
        }

        if (!ModelState.IsValid)
        {
            model.Students = await BuildStudentRowsAsync(model.CourseId, model.Date);
            return View(model);
        }

        var marker = await userManager.GetUserAsync(User);
        foreach (var row in model.Students)
        {
            var existing = await context.Attendances
                .FirstOrDefaultAsync(a =>
                    a.StudentId == row.StudentId &&
                    a.CourseId == model.CourseId &&
                    a.Date == model.Date);

            if (existing is not null)
            {
                existing.Status = row.Status;
                existing.MarkedById = marker?.Id;
            }
            else
            {
                context.Attendances.Add(new Attendance
                {
                    StudentId = row.StudentId,
                    CourseId = model.CourseId,
                    Date = model.Date,
                    Status = row.Status,
                    MarkedById = marker?.Id
                });
            }
        }

        await context.SaveChangesAsync();
        TempData["StatusMessage"] = $"Attendance saved for {model.Date:dd MMM yyyy}.";
        return RedirectToAction(nameof(Index), new { courseId = model.CourseId });
    }

    [Authorize(Policy = "RequireTeacherOrAdmin")]
    public async Task<IActionResult> MonthlyReport(int? courseId, int? year, int? month)
    {
        year ??= DateTime.Today.Year;
        month ??= DateTime.Today.Month;

        await PopulateCourseFilterAsync(courseId);

        List<AttendanceSummary> summaries;
        if (courseId.HasValue && courseId.Value > 0)
        {
            summaries = await attendanceCalculation.GetSummariesForCourseAsync(courseId.Value, year, month);
        }
        else
        {
            summaries = [];
        }

        ViewBag.Year = year;
        ViewBag.Month = month;
        ViewBag.MonthName = new DateTime(year.Value, month.Value, 1).ToString("MMMM yyyy");
        return View(summaries);
    }

    [Authorize(Roles = AppRoles.Student)]
    public async Task<IActionResult> MyAttendance(int? courseId, int? year, int? month)
    {
        var user = await userManager.GetUserAsync(User);
        if (user is null)
        {
            return Challenge();
        }

        year ??= DateTime.Today.Year;
        month ??= DateTime.Today.Month;

        var summaries = await attendanceCalculation.GetSummariesForStudentAsync(user.Id, courseId, year, month);
        var enrolledCourses = await context.CourseEnrollments
            .Where(e => e.StudentId == user.Id)
            .Include(e => e.Course)
            .Select(e => e.Course)
            .ToListAsync();

        ViewBag.CourseId = new SelectList(enrolledCourses, "Id", "CourseName", courseId);
        ViewBag.Year = year;
        ViewBag.Month = month;
        ViewBag.MonthName = new DateTime(year.Value, month.Value, 1).ToString("MMMM yyyy");
        ViewBag.EligibilityThreshold = Refund.EligibilityThreshold;

        return View(summaries);
    }

    private async Task<List<StudentAttendanceRow>> BuildStudentRowsAsync(int courseId, DateOnly date)
    {
        var enrollments = await context.CourseEnrollments
            .Where(e => e.CourseId == courseId)
            .Include(e => e.Student)
            .ToListAsync();

        var existing = await context.Attendances
            .Where(a => a.CourseId == courseId && a.Date == date)
            .ToDictionaryAsync(a => a.StudentId, a => a.Status);

        return enrollments.Select(e => new StudentAttendanceRow
        {
            StudentId = e.StudentId,
            StudentName = e.Student.FullName ?? e.Student.Email ?? e.StudentId,
            Status = existing.TryGetValue(e.StudentId, out var status) ? status : AttendanceStatus.Present,
            HasExistingRecord = existing.ContainsKey(e.StudentId)
        }).ToList();
    }

    private async Task PopulateCourseFilterAsync(int? selectedId)
    {
        var courses = await context.Courses.OrderBy(c => c.CourseName).ToListAsync();
        ViewBag.CourseId = new SelectList(courses, "Id", "CourseName", selectedId);
    }

    private async Task PopulateStudentFilterAsync(string? selectedId)
    {
        var students = await userManager.GetUsersInRoleAsync(AppRoles.Student);
        var items = students
            .OrderBy(s => s.FullName ?? s.Email)
            .Select(s => new { s.Id, Name = s.FullName ?? s.Email ?? s.Id });
        ViewBag.StudentId = new SelectList(items, "Id", "Name", selectedId);
    }
}

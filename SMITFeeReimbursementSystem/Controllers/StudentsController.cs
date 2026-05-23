using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SMITFeeReimbursementSystem.Data;
using SMITFeeReimbursementSystem.Models;
using SMITFeeReimbursementSystem.ViewModels;

namespace SMITFeeReimbursementSystem.Controllers;

[Authorize(Roles = AppRoles.Admin)]
public class StudentsController(
    ApplicationDbContext context,
    UserManager<ApplicationUser> userManager) : Controller
{
    public async Task<IActionResult> Index(string? search, int page = 1)
    {
        const int pageSize = 10;
        var admin = await userManager.GetUserAsync(User);
        ViewBag.AdminName = admin?.FullName ?? admin?.Email ?? "Administrator";

        var students = await userManager.GetUsersInRoleAsync(AppRoles.Student);
        var studentIds = students.Select(s => s.Id).ToList();

        var enrollmentCounts = await context.CourseEnrollments
            .Where(e => studentIds.Contains(e.StudentId))
            .GroupBy(e => e.StudentId)
            .Select(g => new { StudentId = g.Key, Count = g.Count() })
            .ToDictionaryAsync(x => x.StudentId, x => x.Count);

        var items = students
            .Select(s => new StudentListItemViewModel
            {
                Id = s.Id,
                FullName = s.FullName ?? "—",
                RollNumber = s.RollNumber ?? "—",
                Email = s.Email ?? "",
                Phone = s.PhoneNumber,
                EnrolledCourses = enrollmentCounts.GetValueOrDefault(s.Id, 0)
            })
            .AsEnumerable();

        if (!string.IsNullOrWhiteSpace(search))
        {
            items = items.Where(s =>
                s.FullName.Contains(search, StringComparison.OrdinalIgnoreCase) ||
                s.Email.Contains(search, StringComparison.OrdinalIgnoreCase) ||
                s.RollNumber.Contains(search, StringComparison.OrdinalIgnoreCase));
        }

        var ordered = items.OrderBy(s => s.FullName).ToList();
        var totalCount = ordered.Count;
        var paged = ordered.Skip((page - 1) * pageSize).Take(pageSize).ToList();

        ViewBag.Search = search;
        ViewBag.CurrentPage = page;
        ViewBag.TotalPages = (int)Math.Ceiling(totalCount / (double)pageSize);
        ViewBag.TotalCount = totalCount;
        return View(paged);
    }

    public async Task<IActionResult> Details(string id)
    {
        var student = await userManager.FindByIdAsync(id);
        if (student is null) return NotFound();

        var isStudent = await userManager.IsInRoleAsync(student, AppRoles.Student);
        if (!isStudent) return NotFound();

        var enrollments = await context.CourseEnrollments
            .Where(e => e.StudentId == id)
            .Include(e => e.Course)
            .ToListAsync();

        var payments = await context.Payments
            .Where(p => p.StudentId == id)
            .Include(p => p.Course)
            .Include(p => p.Receipt)
            .OrderByDescending(p => p.SubmittedAt)
            .ToListAsync();

        var refunds = await context.Refunds
            .Where(r => r.StudentId == id)
            .Include(r => r.Course)
            .OrderByDescending(r => r.CreatedAt)
            .ToListAsync();

        var model = new StudentDetailsViewModel
        {
            Id = student.Id,
            FullName = student.FullName ?? "—",
            RollNumber = student.RollNumber ?? "—",
            Email = student.Email ?? "",
            Phone = student.PhoneNumber,
            Enrollments = enrollments,
            Payments = payments,
            Refunds = refunds
        };

        return View(model);
    }

    public async Task<IActionResult> Edit(string id)
    {
        var student = await userManager.FindByIdAsync(id);
        if (student is null) return NotFound();

        var isStudent = await userManager.IsInRoleAsync(student, AppRoles.Student);
        if (!isStudent) return NotFound();

        return View(new AdminEditStudentViewModel
        {
            Id = student.Id,
            FullName = student.FullName ?? "",
            RollNumber = student.RollNumber ?? "",
            Email = student.Email ?? "",
            Phone = student.PhoneNumber
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(string id, AdminEditStudentViewModel model)
    {
        if (id != model.Id) return NotFound();
        if (!ModelState.IsValid) return View(model);

        var student = await userManager.FindByIdAsync(id);
        if (student is null) return NotFound();

        student.FullName = model.FullName;
        student.RollNumber = model.RollNumber;
        student.PhoneNumber = model.Phone;

        var result = await userManager.UpdateAsync(student);
        if (!result.Succeeded)
        {
            foreach (var error in result.Errors)
                ModelState.AddModelError(string.Empty, error.Description);
            return View(model);
        }

        TempData["StatusMessage"] = "Student updated successfully.";
        return RedirectToAction(nameof(Details), new { id });
    }

    public async Task<IActionResult> Delete(string id)
    {
        var student = await userManager.FindByIdAsync(id);
        if (student is null) return NotFound();

        var isStudent = await userManager.IsInRoleAsync(student, AppRoles.Student);
        if (!isStudent) return NotFound();

        return View(new StudentListItemViewModel
        {
            Id = student.Id,
            FullName = student.FullName ?? "—",
            RollNumber = student.RollNumber ?? "—",
            Email = student.Email ?? "",
            Phone = student.PhoneNumber
        });
    }

    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(string id)
    {
        var student = await userManager.FindByIdAsync(id);
        if (student is null) return NotFound();

        var result = await userManager.DeleteAsync(student);
        if (!result.Succeeded)
        {
            TempData["StatusMessage"] = "Error: Could not delete student.";
            return RedirectToAction(nameof(Index));
        }

        TempData["StatusMessage"] = "Student deleted successfully.";
        return RedirectToAction(nameof(Index));
    }
}

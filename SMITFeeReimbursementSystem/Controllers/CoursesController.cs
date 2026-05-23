using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SMITFeeReimbursementSystem.Data;
using SMITFeeReimbursementSystem.Models;
using SMITFeeReimbursementSystem.ViewModels;

namespace SMITFeeReimbursementSystem.Controllers;

[Authorize(Roles = AppRoles.Admin)]
public class CoursesController(
    ApplicationDbContext context,
    UserManager<ApplicationUser> userManager) : Controller
{
    public async Task<IActionResult> Index(string? search)
    {
        var query = context.Courses
            .Include(c => c.Enrollments)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
        {
            query = query.Where(c =>
                c.CourseName.Contains(search) ||
                (c.Description != null && c.Description.Contains(search)) ||
                c.Duration.Contains(search));
        }

        var courses = await query
            .OrderByDescending(c => c.CreatedAt)
            .Select(c => new CourseListItemViewModel
            {
                Id = c.Id,
                CourseName = c.CourseName,
                FeeAmount = c.FeeAmount,
                Duration = c.Duration,
                Description = c.Description,
                EnrolledCount = c.Enrollments.Count
            })
            .ToListAsync();

        ViewBag.Search = search;
        return View(courses);
    }

    public IActionResult Create() => View(new CourseViewModel());

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CourseViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        context.Courses.Add(new Course
        {
            CourseName = model.CourseName,
            FeeAmount = model.FeeAmount,
            Duration = model.Duration,
            Description = model.Description
        });
        await context.SaveChangesAsync();
        TempData["StatusMessage"] = "Course created successfully.";
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Edit(int id)
    {
        var course = await context.Courses.FindAsync(id);
        if (course is null)
        {
            return NotFound();
        }

        return View(new CourseViewModel
        {
            Id = course.Id,
            CourseName = course.CourseName,
            FeeAmount = course.FeeAmount,
            Duration = course.Duration,
            Description = course.Description
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, CourseViewModel model)
    {
        if (id != model.Id)
        {
            return NotFound();
        }

        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var course = await context.Courses.FindAsync(id);
        if (course is null)
        {
            return NotFound();
        }

        course.CourseName = model.CourseName;
        course.FeeAmount = model.FeeAmount;
        course.Duration = model.Duration;
        course.Description = model.Description;
        await context.SaveChangesAsync();
        TempData["StatusMessage"] = "Course updated successfully.";
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Details(int id)
    {
        var course = await context.Courses
            .Include(c => c.Enrollments)
            .ThenInclude(e => e.Student)
            .FirstOrDefaultAsync(c => c.Id == id);

        if (course is null)
        {
            return NotFound();
        }

        return View(course);
    }

    public async Task<IActionResult> Delete(int id)
    {
        var course = await context.Courses.FindAsync(id);
        if (course is null)
        {
            return NotFound();
        }

        return View(course);
    }

    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var course = await context.Courses.FindAsync(id);
        if (course is null)
        {
            return NotFound();
        }

        context.Courses.Remove(course);
        await context.SaveChangesAsync();
        TempData["StatusMessage"] = "Course deleted successfully.";
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> AssignStudents(int id)
    {
        var course = await context.Courses.FindAsync(id);
        if (course is null)
        {
            return NotFound();
        }

        var enrolledIds = await context.CourseEnrollments
            .Where(e => e.CourseId == id)
            .Select(e => e.StudentId)
            .ToListAsync();

        var students = await GetStudentUsersAsync();

        var model = new AssignStudentsViewModel
        {
            CourseId = id,
            CourseName = course.CourseName,
            SelectedStudentIds = enrolledIds,
            AvailableStudents = students.Select(s => new StudentOption
            {
                Id = s.Id,
                Name = s.FullName ?? s.Email ?? s.UserName ?? s.Id,
                IsEnrolled = enrolledIds.Contains(s.Id)
            }).ToList()
        };

        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AssignStudents(AssignStudentsViewModel model)
    {
        var course = await context.Courses.FindAsync(model.CourseId);
        if (course is null)
        {
            return NotFound();
        }

        var existing = await context.CourseEnrollments
            .Where(e => e.CourseId == model.CourseId)
            .ToListAsync();

        context.CourseEnrollments.RemoveRange(existing);

        if (model.SelectedStudentIds.Count > 0)
        {
            var enrollments = model.SelectedStudentIds.Distinct().Select(studentId => new CourseEnrollment
            {
                CourseId = model.CourseId,
                StudentId = studentId,
                EnrolledAt = DateTime.UtcNow
            });
            context.CourseEnrollments.AddRange(enrollments);
        }

        await context.SaveChangesAsync();
        TempData["StatusMessage"] = "Students assigned to course successfully.";
        return RedirectToAction(nameof(Details), new { id = model.CourseId });
    }

    private async Task<List<ApplicationUser>> GetStudentUsersAsync()
    {
        var students = await userManager.GetUsersInRoleAsync(AppRoles.Student);
        return students.OrderBy(s => s.FullName ?? s.Email).ToList();
    }
}

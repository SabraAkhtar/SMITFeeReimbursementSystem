using System.Diagnostics;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SMITFeeReimbursementSystem.Data;
using SMITFeeReimbursementSystem.Models;

namespace SMITFeeReimbursementSystem.Controllers;

public class HomeController(
    ApplicationDbContext context,
    UserManager<ApplicationUser> userManager) : Controller
{
    public async Task<IActionResult> Index()
    {
        // Check if logged-in student has any rejected payments
        if (User.Identity?.IsAuthenticated == true && User.IsInRole(AppRoles.Student))
        {
            var user = await userManager.GetUserAsync(User);
            if (user is not null)
            {
                var rejectedPayments = await context.Payments
                    .Include(p => p.Course)
                    .Where(p => p.StudentId == user.Id && p.Status == PaymentStatus.Rejected)
                    .OrderByDescending(p => p.ReviewedAt)
                    .ToListAsync();

                ViewBag.RejectedPayments = rejectedPayments;
            }
        }

        return View();
    }

    public IActionResult Privacy() => View();

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}

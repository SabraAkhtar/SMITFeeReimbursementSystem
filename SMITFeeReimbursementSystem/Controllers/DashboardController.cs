using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using SMITFeeReimbursementSystem.Models;
using SMITFeeReimbursementSystem.Services;

namespace SMITFeeReimbursementSystem.Controllers;

[Authorize(Roles = AppRoles.Admin)]
public class DashboardController(
    IDashboardService dashboardService,
    UserManager<ApplicationUser> userManager) : Controller
{
    public async Task<IActionResult> Index()
    {
        var user = await userManager.GetUserAsync(User);
        ViewBag.AdminName = user?.FullName ?? user?.Email ?? "Administrator";

        var model = await dashboardService.GetDashboardDataAsync();
        return View(model);
    }
}

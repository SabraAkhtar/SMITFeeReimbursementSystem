using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SMITFeeReimbursementSystem.Models;

namespace SMITFeeReimbursementSystem.Controllers;

[Authorize(Roles = AppRoles.Admin)]
public class AdminController : Controller
{
    public IActionResult Dashboard() =>
        RedirectToAction(nameof(DashboardController.Index), "Dashboard");
}

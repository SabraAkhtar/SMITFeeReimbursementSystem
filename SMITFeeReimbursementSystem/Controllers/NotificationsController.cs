using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using SMITFeeReimbursementSystem.Models;
using SMITFeeReimbursementSystem.Services;

namespace SMITFeeReimbursementSystem.Controllers;

[Authorize]
public class NotificationsController(
    INotificationService notificationService,
    UserManager<ApplicationUser> userManager) : Controller
{
    /// <summary>Mark a single notification as read, then redirect to its action URL.</summary>
    public async Task<IActionResult> Open(int id, string? returnUrl)
    {
        await notificationService.MarkAsReadAsync(id);

        if (!string.IsNullOrWhiteSpace(returnUrl) && Url.IsLocalUrl(returnUrl))
            return Redirect(returnUrl);

        return User.IsInRole(AppRoles.Admin)
            ? RedirectToAction("Index", "Payments")
            : RedirectToAction("MyPayments", "Payments");
    }

    /// <summary>Mark all notifications as read (AJAX).</summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> MarkAllRead()
    {
        string? userId = null;
        if (!User.IsInRole(AppRoles.Admin))
        {
            var user = await userManager.GetUserAsync(User);
            userId = user?.Id;
        }

        await notificationService.MarkAllAsReadAsync(userId);
        return Json(new { success = true });
    }
}

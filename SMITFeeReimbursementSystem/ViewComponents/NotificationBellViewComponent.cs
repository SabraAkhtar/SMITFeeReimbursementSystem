using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using SMITFeeReimbursementSystem.Models;
using SMITFeeReimbursementSystem.Services;

namespace SMITFeeReimbursementSystem.ViewComponents;

public class NotificationBellViewComponent(
    INotificationService notificationService,
    UserManager<ApplicationUser> userManager) : ViewComponent
{
    public async Task<IViewComponentResult> InvokeAsync()
    {
        var isAdmin = UserClaimsPrincipal.IsInRole(AppRoles.Admin);

        if (isAdmin)
        {
            var summary = await notificationService.GetPaymentSummaryAsync();
            var notifications = await notificationService.GetRecentAsync(8, userId: null);
            ViewBag.UnreadCount = await notificationService.GetUnreadCountAsync(userId: null);
            ViewBag.Summary = summary;
            ViewBag.IsAdmin = true;
            return View(notifications);
        }
        else
        {
            // Student
            var user = await userManager.GetUserAsync(UserClaimsPrincipal);
            var userId = user?.Id;
            var notifications = await notificationService.GetRecentAsync(8, userId: userId);
            ViewBag.UnreadCount = await notificationService.GetUnreadCountAsync(userId: userId);
            ViewBag.Summary = new NotificationSummary(0, 0, 0, 0);
            ViewBag.IsAdmin = false;
            return View(notifications);
        }
    }
}

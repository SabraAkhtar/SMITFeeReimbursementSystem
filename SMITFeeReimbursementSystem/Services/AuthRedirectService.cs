using Microsoft.AspNetCore.Identity;
using SMITFeeReimbursementSystem.Models;

namespace SMITFeeReimbursementSystem.Services;

public class AuthRedirectService(UserManager<ApplicationUser> userManager) : IAuthRedirectService
{
    public async Task<string> GetHomePathForUserAsync(ApplicationUser user)
    {
        if (await userManager.IsInRoleAsync(user, AppRoles.Admin))
            return "/Dashboard";

        if (await userManager.IsInRoleAsync(user, AppRoles.Teacher))
            return "/Attendance/Mark";

        if (await userManager.IsInRoleAsync(user, AppRoles.Student))
            return "/Payments/MyPayments";

        return "/";
    }
}

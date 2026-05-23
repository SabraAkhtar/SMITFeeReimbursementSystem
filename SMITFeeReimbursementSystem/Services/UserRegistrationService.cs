using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using SMITFeeReimbursementSystem.Models;

namespace SMITFeeReimbursementSystem.Services;

public class UserRegistrationService(UserManager<ApplicationUser> userManager) : IUserRegistrationService
{
    public async Task<bool> IsFirstUserAsync() =>
        !await userManager.Users.AnyAsync();

    public async Task<string> ResolveRoleForNewUserAsync(string? requestedRole)
    {
        if (!await userManager.Users.AnyAsync())
        {
            return AppRoles.Admin;
        }

        return AppRoles.Registerable.Contains(requestedRole)
            ? requestedRole!
            : AppRoles.Student;
    }
}

using Microsoft.AspNetCore.Identity;
using SMITFeeReimbursementSystem.Data;
using SMITFeeReimbursementSystem.Models;

namespace SMITFeeReimbursementSystem.Extensions;

public static class IdentityServiceExtensions
{
    public static IServiceCollection AddApplicationIdentity(this IServiceCollection services)
    {
        services
            .AddIdentity<ApplicationUser, IdentityRole>(options =>
            {
                options.SignIn.RequireConfirmedAccount = false;
                options.User.RequireUniqueEmail = true;

                options.Password.RequireDigit = true;
                options.Password.RequireLowercase = true;
                options.Password.RequireUppercase = true;
                options.Password.RequireNonAlphanumeric = false;
                options.Password.RequiredLength = 6;

                options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(5);
                options.Lockout.MaxFailedAccessAttempts = 5;
                options.Lockout.AllowedForNewUsers = true;
            })
            .AddEntityFrameworkStores<ApplicationDbContext>()
            .AddDefaultTokenProviders();

        services.ConfigureApplicationCookie(options =>
        {
            options.Cookie.HttpOnly = true;
            options.ExpireTimeSpan = TimeSpan.FromDays(14);
            options.SlidingExpiration = true;
            options.LoginPath = "/Account/Login";
            options.LogoutPath = "/Account/Logout";
            options.AccessDeniedPath = "/Account/AccessDenied";
        });

        services.AddAuthorization(options =>
        {
            options.AddPolicy("RequireAdmin", policy => policy.RequireRole(AppRoles.Admin));
            options.AddPolicy("RequireTeacher", policy => policy.RequireRole(AppRoles.Teacher));
            options.AddPolicy("RequireStudent", policy => policy.RequireRole(AppRoles.Student));
            options.AddPolicy("RequireTeacherOrAdmin", policy =>
                policy.RequireRole(AppRoles.Admin, AppRoles.Teacher));
        });

        return services;
    }
}

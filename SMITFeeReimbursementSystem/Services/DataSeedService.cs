using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using SMITFeeReimbursementSystem.Data;
using SMITFeeReimbursementSystem.Models;

namespace SMITFeeReimbursementSystem.Services;

public class DataSeedService(
    ApplicationDbContext context,
    RoleManager<IdentityRole> roleManager,
    UserManager<ApplicationUser> userManager,
    ILogger<DataSeedService> logger) : IDataSeedService
{
    public async Task SeedAsync()
    {
        if (context.Database.IsSqlite())
        {
            await context.Database.EnsureCreatedAsync();
        }
        else
        {
            await context.Database.MigrateAsync();
        }

        await SeedRolesAsync();
        await SeedCoursesAsync();
        await SeedAdminAsync();
    }

    private async Task SeedAdminAsync()
    {
        const string adminEmail = "subrahkhan3@gmail.com";
        const string adminPassword = "SubraAkhtar12345";

        var existing = await userManager.FindByEmailAsync(adminEmail);
        if (existing is not null)
        {
            // Make sure this user is Admin
            if (!await userManager.IsInRoleAsync(existing, AppRoles.Admin))
            {
                await userManager.AddToRoleAsync(existing, AppRoles.Admin);
                logger.LogInformation("Admin role assigned to existing user '{Email}'.", adminEmail);
            }
            return;
        }

        var admin = new ApplicationUser
        {
            UserName = adminEmail,
            Email = adminEmail,
            EmailConfirmed = true,
            FullName = "Sabra Akhtar"
        };

        var result = await userManager.CreateAsync(admin, adminPassword);
        if (result.Succeeded)
        {
            await userManager.AddToRoleAsync(admin, AppRoles.Admin);
            logger.LogInformation("Default admin account created: {Email}", adminEmail);
        }
        else
        {
            logger.LogWarning("Failed to create admin: {Errors}",
                string.Join(", ", result.Errors.Select(e => e.Description)));
        }
    }

    private async Task SeedRolesAsync()
    {
        foreach (var roleName in AppRoles.All)
        {
            if (await roleManager.RoleExistsAsync(roleName))
                continue;

            var result = await roleManager.CreateAsync(new IdentityRole(roleName));
            if (result.Succeeded)
                logger.LogInformation("Identity role '{Role}' created.", roleName);
            else
                logger.LogWarning("Failed to create role '{Role}': {Errors}", roleName,
                    string.Join(", ", result.Errors.Select(e => e.Description)));
        }
    }

    private async Task SeedCoursesAsync()
    {
        // Only seed if no courses exist yet
        if (await context.Courses.AnyAsync())
            return;

        var courses = new List<Course>
        {
            new Course
            {
                CourseName = "Web & App Development",
                FeeAmount = 3000m,
                Duration = "6 Months",
                Description = "Full-stack web and mobile app development using modern frameworks like React, Node.js, and Flutter.",
                CreatedAt = DateTime.UtcNow
            },
            new Course
            {
                CourseName = "Agentic AI",
                FeeAmount = 3000m,
                Duration = "4 Months",
                Description = "Build intelligent AI agents using LLMs, LangChain, and autonomous workflow automation.",
                CreatedAt = DateTime.UtcNow
            },
            new Course
            {
                CourseName = "Digital Marketing",
                FeeAmount = 3000m,
                Duration = "3 Months",
                Description = "SEO, social media marketing, Google Ads, content strategy, and analytics for modern businesses.",
                CreatedAt = DateTime.UtcNow
            },
            new Course
            {
                CourseName = "English Communication",
                FeeAmount = 3000m,
                Duration = "3 Months",
                Description = "Professional English speaking, writing, and presentation skills for career growth.",
                CreatedAt = DateTime.UtcNow
            },
            new Course
            {
                CourseName = "Cyber Security",
                FeeAmount = 3000m,
                Duration = "5 Months",
                Description = "Ethical hacking, network security, penetration testing, and cybersecurity fundamentals.",
                CreatedAt = DateTime.UtcNow
            }
        };

        context.Courses.AddRange(courses);
        await context.SaveChangesAsync();
        logger.LogInformation("Seeded {Count} default SMIT courses.", courses.Count);
    }
}

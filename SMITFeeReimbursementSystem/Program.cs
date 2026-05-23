using Microsoft.EntityFrameworkCore;
using SMITFeeReimbursementSystem.Data;
using SMITFeeReimbursementSystem.Extensions;
using SMITFeeReimbursementSystem.Services;

var builder = WebApplication.CreateBuilder(args);

// Database: SQLite for production/deploy, SQL Server for local dev
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

if (builder.Environment.IsProduction() || connectionString.StartsWith("Data Source="))
{
    builder.Services.AddDbContext<ApplicationDbContext>(options =>
        options.UseSqlite(connectionString));
}
else
{
    builder.Services.AddDbContext<ApplicationDbContext>(options =>
        options.UseSqlServer(connectionString));
}

builder.Services.AddDatabaseDeveloperPageExceptionFilter();
builder.Services.AddApplicationIdentity();
builder.Services.AddScoped<IDataSeedService, DataSeedService>();
builder.Services.AddScoped<IUserRegistrationService, UserRegistrationService>();
builder.Services.AddScoped<IAuthRedirectService, AuthRedirectService>();
builder.Services.AddScoped<IFileUploadService, FileUploadService>();
builder.Services.AddScoped<IReceiptService, ReceiptService>();
builder.Services.AddScoped<IAttendanceCalculationService, AttendanceCalculationService>();
builder.Services.AddScoped<IRefundEligibilityService, RefundEligibilityService>();
builder.Services.AddScoped<IDashboardService, DashboardService>();
builder.Services.AddControllersWithViews();

// Port from environment variable (Railway sets this)
var port = Environment.GetEnvironmentVariable("PORT");
if (!string.IsNullOrEmpty(port))
{
    builder.WebHost.UseUrls($"http://0.0.0.0:{port}");
}

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var seedService = scope.ServiceProvider.GetRequiredService<IDataSeedService>();
    await seedService.SeedAsync();
}

if (app.Environment.IsDevelopment())
{
    app.UseMigrationsEndPoint();
}
else
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
    app.UseHttpsRedirection();
}

app.UseStaticFiles();
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();

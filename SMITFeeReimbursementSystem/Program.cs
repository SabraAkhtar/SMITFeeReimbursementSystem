using Microsoft.EntityFrameworkCore;
using SMITFeeReimbursementSystem.Data;
using SMITFeeReimbursementSystem.Extensions;
using SMITFeeReimbursementSystem.Services;

var builder = WebApplication.CreateBuilder(args);

// ---- Ensure wwwroot exists ----
var wwwrootPath = Path.Combine(builder.Environment.ContentRootPath, "wwwroot");
if (!Directory.Exists(wwwrootPath))
{
    Directory.CreateDirectory(wwwrootPath);
    Directory.CreateDirectory(Path.Combine(wwwrootPath, "css"));
    Directory.CreateDirectory(Path.Combine(wwwrootPath, "js"));
    Directory.CreateDirectory(Path.Combine(wwwrootPath, "uploads", "payments"));
    Directory.CreateDirectory(Path.Combine(wwwrootPath, "uploads", "receipts"));
}
builder.WebHost.UseWebRoot(wwwrootPath);

// ---- Database: SQLite ----
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? "Data Source=app.db";
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlite(connectionString));

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

// ---- PORT for Railway ----
var port = Environment.GetEnvironmentVariable("PORT");
if (!string.IsNullOrEmpty(port))
{
    builder.WebHost.UseUrls($"http://0.0.0.0:{port}");
}

var app = builder.Build();

// ---- Seed database ----
using (var scope = app.Services.CreateScope())
{
    var seedService = scope.ServiceProvider.GetRequiredService<IDataSeedService>();
    await seedService.SeedAsync();
}

// ---- Middleware pipeline ----
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
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

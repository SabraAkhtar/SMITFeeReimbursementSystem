# SMIT Fee Reimbursement System

ASP.NET Core MVC (.NET 10) web application with SQL Server, Entity Framework Core (Code First), and ASP.NET Core Identity.

## Prerequisites

- [.NET 10 SDK](https://dotnet.microsoft.com/download)
- SQL Server or SQL Server LocalDB

## Project structure

```
SMITFeeReimbursementSystem/
├── Controllers/     # MVC controllers (Home, Account)
├── Data/            # DbContext and EF Core migrations
├── Models/          # Domain and Identity models
├── Services/        # Application services (data seeding)
├── ViewModels/      # View models for forms
├── Views/           # Razor views
└── wwwroot/         # Static files (Bootstrap 5, jQuery)
```

## Database

Update the connection string in `appsettings.json`:

```json
"ConnectionStrings": {
  "DefaultConnection": "Server=(localdb)\\mssqllocaldb;Database=SMITFeeReimbursementDb;Trusted_Connection=True;MultipleActiveResultSets=true;TrustServerCertificate=True"
}
```

Migrations are applied automatically on startup. To add a new migration manually:

```bash
cd SMITFeeReimbursementSystem
dotnet ef migrations add MigrationName --output-dir Data/Migrations
dotnet ef database update
```

## Roles

| Role    | Description              |
|---------|--------------------------|
| Admin   | System administrator     |
| Teacher | Teaching staff           |
| Student | Student users (default)  |

**Startup:** Roles `Admin`, `Teacher`, and `Student` are seeded automatically when the app starts.

**First registration:** The first user to register is automatically assigned the **Admin** role. Later users choose **Teacher** or **Student** during registration.

**Admin dashboard:** Admins are redirected to `/Admin/Dashboard` after login. The dashboard is restricted to the Admin role only.

## Course & Payment modules

### Courses (Admin only)
- CRUD at `/Courses`
- Fields: name, fee (default 3000), duration, description
- Assign students to courses

### Payments
- **Students:** Submit payment with screenshot at `/Payments/Submit` (enrolled courses only)
- **Admin:** Review, approve, reject at `/Payments`
- On approval: receipt generated at `/Payments/Receipt/{id}`

### Attendance (Admin & Teacher)
- Mark daily attendance by course/date at `/Attendance/Mark`
- Statuses: Present, Absent, Leave
- Monthly report with auto % calculation: `(Present / Total) × 100`

### Refunds (Admin)
- Auto-eligibility when attendance ≥ **80%**
- Approve/reject at `/Refunds`
- Students view status at `/Refunds/MyRefunds`

### PDF Receipts (QuestPDF)
- Auto-generated on payment approval
- Fields: Receipt #, Student, Roll #, Course, Amount, Transaction ID, Payment Date, Approved By
- Download PDF: `/Payments/DownloadPdf/{paymentId}`
- Print-friendly receipt view at `/Payments/Receipt/{paymentId}`
- Stored in database + `wwwroot/uploads/receipts/`

## Security summary

| Action | Admin | Teacher | Student |
|--------|-------|---------|---------|
| Approve payments / refunds | Yes | No | No |
| Mark attendance | Yes | Yes | No |
| Submit payments | No | No | Yes |
| View own data | Yes | No | Yes |

## Run the application

```bash
cd SMITFeeReimbursementSystem
dotnet run
```

Open `https://localhost:7xxx` (see console output for the exact URL).

## Authentication

- **Login:** `/Account/Login`
- **Register:** `/Account/Register`
- **Logout:** POST to `/Account/Logout`

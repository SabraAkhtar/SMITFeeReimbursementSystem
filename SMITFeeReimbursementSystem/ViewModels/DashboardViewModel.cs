namespace SMITFeeReimbursementSystem.ViewModels;

public class DashboardViewModel
{
    public DashboardStatsViewModel Stats { get; set; } = new();
    public DashboardChartsViewModel Charts { get; set; } = new();
    public DashboardActivityViewModel RecentActivity { get; set; } = new();
}

public class DashboardStatsViewModel
{
    public int TotalStudents { get; set; }
    public int TotalCourses { get; set; }
    public int TotalPayments { get; set; }
    public int PendingPayments { get; set; }
    public int ApprovedPayments { get; set; }
    public int RejectedPayments { get; set; }
    public int TotalAttendanceRecords { get; set; }
    public int RefundEligibleStudents { get; set; }
}

public class DashboardChartsViewModel
{
    public ChartDatasetViewModel PaymentStatus { get; set; } = new();
    public ChartDatasetViewModel AttendanceByCourse { get; set; } = new();
    public ChartDatasetViewModel EnrollmentByCourse { get; set; } = new();
}

public class ChartDatasetViewModel
{
    public List<string> Labels { get; set; } = [];
    public List<decimal> Values { get; set; } = [];
    public List<string>? Colors { get; set; }
}

public class DashboardActivityViewModel
{
    public List<RecentPaymentViewModel> LatestPayments { get; set; } = [];
    public List<RecentEnrollmentViewModel> LatestEnrollments { get; set; } = [];
    public List<RecentAttendanceViewModel> RecentAttendance { get; set; } = [];
}

public class RecentPaymentViewModel
{
    public int Id { get; set; }
    public string StudentName { get; set; } = string.Empty;
    public string CourseName { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public string Status { get; set; } = string.Empty;
    public DateTime SubmittedAt { get; set; }
}

public class RecentEnrollmentViewModel
{
    public string StudentName { get; set; } = string.Empty;
    public string CourseName { get; set; } = string.Empty;
    public DateTime EnrolledAt { get; set; }
}

public class RecentAttendanceViewModel
{
    public string StudentName { get; set; } = string.Empty;
    public string CourseName { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public DateOnly Date { get; set; }
}

using SMITFeeReimbursementSystem.Models;

namespace SMITFeeReimbursementSystem.Services;

public interface INotificationService
{
    /// <summary>Create a notification for admins when a student submits a payment.</summary>
    Task CreatePaymentSubmittedNotificationAsync(int paymentId, string studentName, string courseName, decimal amount);

    /// <summary>Create a notification for a specific student when their payment is approved.</summary>
    Task CreatePaymentApprovedNotificationAsync(int paymentId, string studentId, string courseName, decimal amount);

    /// <summary>Create a notification for a specific student when their payment is rejected.</summary>
    Task CreatePaymentRejectedNotificationAsync(int paymentId, string studentId, string courseName, string? remarks);

    /// <summary>Get unread notification count (for navbar badge) — admin sees all, student sees own.</summary>
    Task<int> GetUnreadCountAsync(string? userId = null);

    /// <summary>Get recent notifications for the dropdown (latest N) — admin sees all, student sees own.</summary>
    Task<List<Notification>> GetRecentAsync(int count = 10, string? userId = null);

    /// <summary>Mark a single notification as read.</summary>
    Task MarkAsReadAsync(int id);

    /// <summary>Mark all notifications as read for a user (null = admin global).</summary>
    Task MarkAllAsReadAsync(string? userId = null);

    /// <summary>Get live payment summary counts (new, pending, approved) for the badge tooltip.</summary>
    Task<NotificationSummary> GetPaymentSummaryAsync();
}

public record NotificationSummary(int NewSubmissions, int Pending, int Approved, int Rejected);

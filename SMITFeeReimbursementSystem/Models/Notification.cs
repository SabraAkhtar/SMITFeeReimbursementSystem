namespace SMITFeeReimbursementSystem.Models;

public class Notification
{
    public int Id { get; set; }

    /// <summary>Short title shown in dropdown</summary>
    public string Title { get; set; } = string.Empty;

    /// <summary>Detailed message body</summary>
    public string Message { get; set; } = string.Empty;

    /// <summary>Optional link to navigate when clicked</summary>
    public string? ActionUrl { get; set; }

    public NotificationType Type { get; set; } = NotificationType.PaymentSubmitted;

    public bool IsRead { get; set; } = false;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>null = admin notification (global), non-null = student-specific notification</summary>
    public string? ForUserId { get; set; }

    /// <summary>The payment this notification relates to (if any)</summary>
    public int? PaymentId { get; set; }
    public Payment? Payment { get; set; }
}

public enum NotificationType
{
    PaymentSubmitted,
    PaymentApproved,
    PaymentRejected
}

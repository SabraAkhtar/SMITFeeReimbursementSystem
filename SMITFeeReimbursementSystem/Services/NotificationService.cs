using Microsoft.EntityFrameworkCore;
using SMITFeeReimbursementSystem.Data;
using SMITFeeReimbursementSystem.Models;

namespace SMITFeeReimbursementSystem.Services;

public class NotificationService(ApplicationDbContext context) : INotificationService
{
    // ── Admin notification: new payment submitted ──────────────────────────
    public async Task CreatePaymentSubmittedNotificationAsync(
        int paymentId, string studentName, string courseName, decimal amount)
    {
        context.Notifications.Add(new Notification
        {
            Title = "New Payment Submitted",
            Message = $"{studentName} submitted fee for {courseName} — Rs. {amount:N0}",
            ActionUrl = $"/Payments/Review/{paymentId}",
            Type = NotificationType.PaymentSubmitted,
            PaymentId = paymentId,
            ForUserId = null   // null = admin
        });
        await context.SaveChangesAsync();
    }

    // ── Student notification: payment approved ─────────────────────────────
    public async Task CreatePaymentApprovedNotificationAsync(
        int paymentId, string studentId, string courseName, decimal amount)
    {
        context.Notifications.Add(new Notification
        {
            Title = "Payment Approved ✓",
            Message = $"Your fee payment for {courseName} (Rs. {amount:N0}) has been approved. Download your receipt now.",
            ActionUrl = $"/Payments/Receipt/{paymentId}",
            Type = NotificationType.PaymentApproved,
            PaymentId = paymentId,
            ForUserId = studentId
        });
        await context.SaveChangesAsync();
    }

    // ── Student notification: payment rejected ─────────────────────────────
    public async Task CreatePaymentRejectedNotificationAsync(
        int paymentId, string studentId, string courseName, string? remarks)
    {
        var msg = $"Your fee payment for {courseName} was rejected.";
        if (!string.IsNullOrWhiteSpace(remarks))
            msg += $" Reason: {remarks}";

        context.Notifications.Add(new Notification
        {
            Title = "Payment Rejected",
            Message = msg,
            ActionUrl = $"/Payments/MyPayments",
            Type = NotificationType.PaymentRejected,
            PaymentId = paymentId,
            ForUserId = studentId
        });
        await context.SaveChangesAsync();
    }

    // ── Unread count ───────────────────────────────────────────────────────
    public async Task<int> GetUnreadCountAsync(string? userId = null)
    {
        var query = context.Notifications.Where(n => !n.IsRead);
        query = userId is null
            ? query.Where(n => n.ForUserId == null)   // admin
            : query.Where(n => n.ForUserId == userId); // student
        return await query.CountAsync();
    }

    // ── Recent list ────────────────────────────────────────────────────────
    public async Task<List<Notification>> GetRecentAsync(int count = 10, string? userId = null)
    {
        var query = context.Notifications.AsQueryable();
        query = userId is null
            ? query.Where(n => n.ForUserId == null)
            : query.Where(n => n.ForUserId == userId);
        return await query
            .OrderByDescending(n => n.CreatedAt)
            .Take(count)
            .ToListAsync();
    }

    // ── Mark read ──────────────────────────────────────────────────────────
    public async Task MarkAsReadAsync(int id)
    {
        var n = await context.Notifications.FindAsync(id);
        if (n is not null) { n.IsRead = true; await context.SaveChangesAsync(); }
    }

    public async Task MarkAllAsReadAsync(string? userId = null)
    {
        var query = context.Notifications.Where(n => !n.IsRead);
        var list = userId is null
            ? await query.Where(n => n.ForUserId == null).ToListAsync()
            : await query.Where(n => n.ForUserId == userId).ToListAsync();
        foreach (var n in list) n.IsRead = true;
        await context.SaveChangesAsync();
    }

    // ── Payment summary (admin only) ───────────────────────────────────────
    public async Task<NotificationSummary> GetPaymentSummaryAsync()
    {
        var counts = await context.Payments
            .GroupBy(p => p.Status)
            .Select(g => new { Status = g.Key, Count = g.Count() })
            .ToListAsync();

        var newSubmissions = await context.Notifications
            .CountAsync(n => !n.IsRead && n.Type == NotificationType.PaymentSubmitted && n.ForUserId == null);

        return new NotificationSummary(
            NewSubmissions: newSubmissions,
            Pending: counts.FirstOrDefault(x => x.Status == PaymentStatus.Pending)?.Count ?? 0,
            Approved: counts.FirstOrDefault(x => x.Status == PaymentStatus.Approved)?.Count ?? 0,
            Rejected: counts.FirstOrDefault(x => x.Status == PaymentStatus.Rejected)?.Count ?? 0
        );
    }
}

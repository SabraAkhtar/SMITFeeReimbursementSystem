using Microsoft.EntityFrameworkCore;
using SMITFeeReimbursementSystem.Data;
using SMITFeeReimbursementSystem.Models;

namespace SMITFeeReimbursementSystem.Services;

public class RefundEligibilityService(
    ApplicationDbContext context,
    IAttendanceCalculationService attendanceCalculation) : IRefundEligibilityService
{
    public async Task<List<Refund>> SyncEligibleRefundsAsync()
    {
        var enrollments = await context.CourseEnrollments
            .Include(e => e.Student)
            .Include(e => e.Course)
            .ToListAsync();

        foreach (var enrollment in enrollments)
        {
            var summary = await attendanceCalculation.GetSummaryAsync(
                enrollment.StudentId, enrollment.CourseId);

            if (!attendanceCalculation.IsEligibleForRefund(summary.AttendancePercentage))
            {
                continue;
            }

            var existing = await context.Refunds
                .FirstOrDefaultAsync(r =>
                    r.StudentId == enrollment.StudentId &&
                    r.CourseId == enrollment.CourseId);

            if (existing is null)
            {
                context.Refunds.Add(new Refund
                {
                    StudentId = enrollment.StudentId,
                    CourseId = enrollment.CourseId,
                    AttendancePercentage = summary.AttendancePercentage,
                    RefundStatus = RefundStatus.Pending
                });
            }
            else if (existing.RefundStatus == RefundStatus.Pending)
            {
                existing.AttendancePercentage = summary.AttendancePercentage;
            }
        }

        await context.SaveChangesAsync();
        return await GetEligibleRefundsAsync();
    }

    public async Task<List<Refund>> GetEligibleRefundsAsync()
    {
        return await context.Refunds
            .Include(r => r.Student)
            .Include(r => r.Course)
            .Where(r => r.AttendancePercentage >= Refund.EligibilityThreshold)
            .OrderByDescending(r => r.AttendancePercentage)
            .ToListAsync();
    }
}

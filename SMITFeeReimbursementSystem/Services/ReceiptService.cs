using Microsoft.EntityFrameworkCore;
using SMITFeeReimbursementSystem.Data;
using SMITFeeReimbursementSystem.Models;

namespace SMITFeeReimbursementSystem.Services;

public class ReceiptService(
    ApplicationDbContext context,
    IWebHostEnvironment environment) : IReceiptService
{
    public async Task<Receipt> GenerateReceiptAsync(int paymentId)
    {
        var payment = await context.Payments
            .Include(p => p.Student)
            .Include(p => p.Course)
            .Include(p => p.ReviewedBy)
            .Include(p => p.Receipt)
            .FirstOrDefaultAsync(p => p.Id == paymentId)
            ?? throw new InvalidOperationException("Payment not found.");

        if (payment.Status != PaymentStatus.Approved)
        {
            throw new InvalidOperationException("Receipt can only be generated for approved payments.");
        }

        var receipt = payment.Receipt ?? new Receipt { PaymentId = payment.Id };

        if (payment.Receipt is null)
        {
            var count = await context.Receipts.CountAsync();
            receipt.ReceiptNumber = $"RCP-{DateTime.UtcNow:yyyyMMdd}-{(count + 1):D4}";
            context.Receipts.Add(receipt);
        }

        receipt.StudentId = payment.StudentId;
        receipt.CourseId = payment.CourseId;
        receipt.StudentName = payment.Student.FullName ?? payment.Student.Email ?? "Student";
        receipt.RollNumber = payment.Student.RollNumber ?? "N/A";
        receipt.CourseName = payment.Course.CourseName;
        receipt.Amount = payment.Amount;
        receipt.TransactionId = payment.TransactionId;
        receipt.PaymentDate = payment.ReviewedAt ?? payment.SubmittedAt;
        receipt.ApprovedById = payment.ReviewedById;
        receipt.ApprovedByName = payment.ReviewedBy?.FullName ?? payment.ReviewedBy?.Email ?? "Administrator";
        receipt.GeneratedAt = DateTime.UtcNow;

        var pdfBytes = PdfReceiptGenerator.Generate(receipt);
        receipt.PdfFilePath = await SavePdfAsync(receipt.ReceiptNumber, pdfBytes);

        await context.SaveChangesAsync();
        return receipt;
    }

    public async Task<byte[]?> GetReceiptPdfAsync(int paymentId)
    {
        var receipt = await GetReceiptByPaymentIdAsync(paymentId);
        if (receipt is null)
        {
            return null;
        }

        if (!string.IsNullOrEmpty(receipt.PdfFilePath))
        {
            var fullPath = Path.Combine(environment.WebRootPath, receipt.PdfFilePath.TrimStart('/').Replace('/', Path.DirectorySeparatorChar));
            if (System.IO.File.Exists(fullPath))
            {
                return await System.IO.File.ReadAllBytesAsync(fullPath);
            }
        }

        return PdfReceiptGenerator.Generate(receipt);
    }

    public async Task<Receipt?> GetReceiptByPaymentIdAsync(int paymentId) =>
        await context.Receipts
            .AsNoTracking()
            .Include(r => r.Student)
            .Include(r => r.Course)
            .Include(r => r.Payment)
            .FirstOrDefaultAsync(r => r.PaymentId == paymentId);

    private async Task<string> SavePdfAsync(string receiptNumber, byte[] pdfBytes)
    {
        var folder = Path.Combine(environment.WebRootPath, "uploads", "receipts");
        Directory.CreateDirectory(folder);

        var safeName = $"{receiptNumber.Replace("/", "-")}.pdf";
        var fullPath = Path.Combine(folder, safeName);
        await System.IO.File.WriteAllBytesAsync(fullPath, pdfBytes);

        return $"/uploads/receipts/{safeName}";
    }
}

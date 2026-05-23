using SMITFeeReimbursementSystem.Models;

namespace SMITFeeReimbursementSystem.Services;

public interface IReceiptService
{
    Task<Receipt> GenerateReceiptAsync(int paymentId);
    Task<byte[]?> GetReceiptPdfAsync(int paymentId);
    Task<Receipt?> GetReceiptByPaymentIdAsync(int paymentId);
}

namespace SMITFeeReimbursementSystem.Services;

public interface IFileUploadService
{
    Task<string> SavePaymentScreenshotAsync(IFormFile file);
}

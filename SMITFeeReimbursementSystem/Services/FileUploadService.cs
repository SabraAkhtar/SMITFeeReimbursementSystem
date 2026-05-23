namespace SMITFeeReimbursementSystem.Services;

public class FileUploadService(IWebHostEnvironment environment) : IFileUploadService
{
    private static readonly string[] AllowedExtensions = [".jpg", ".jpeg", ".png", ".gif", ".webp"];
    private const long MaxFileSize = 5 * 1024 * 1024;

    public async Task<string> SavePaymentScreenshotAsync(IFormFile file)
    {
        if (file.Length == 0)
        {
            throw new InvalidOperationException("Screenshot file is required.");
        }

        if (file.Length > MaxFileSize)
        {
            throw new InvalidOperationException("Screenshot must be 5 MB or smaller.");
        }

        var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
        if (!AllowedExtensions.Contains(extension))
        {
            throw new InvalidOperationException("Only image files (jpg, png, gif, webp) are allowed.");
        }

        var uploadsFolder = Path.Combine(environment.WebRootPath, "uploads", "payments");
        Directory.CreateDirectory(uploadsFolder);

        var fileName = $"{Guid.NewGuid():N}{extension}";
        var filePath = Path.Combine(uploadsFolder, fileName);

        await using var stream = new FileStream(filePath, FileMode.Create);
        await file.CopyToAsync(stream);

        return $"/uploads/payments/{fileName}";
    }
}

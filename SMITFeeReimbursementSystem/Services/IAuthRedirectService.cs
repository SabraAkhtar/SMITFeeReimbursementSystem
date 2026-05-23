using SMITFeeReimbursementSystem.Models;

namespace SMITFeeReimbursementSystem.Services;

public interface IAuthRedirectService
{
    Task<string> GetHomePathForUserAsync(ApplicationUser user);
}

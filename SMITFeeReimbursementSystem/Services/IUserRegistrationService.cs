namespace SMITFeeReimbursementSystem.Services;

public interface IUserRegistrationService
{
    Task<bool> IsFirstUserAsync();
    Task<string> ResolveRoleForNewUserAsync(string? requestedRole);
}

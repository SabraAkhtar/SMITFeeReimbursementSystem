using SMITFeeReimbursementSystem.Models;

namespace SMITFeeReimbursementSystem.Services;

public interface IRefundEligibilityService
{
    Task<List<Refund>> SyncEligibleRefundsAsync();
    Task<List<Refund>> GetEligibleRefundsAsync();
}

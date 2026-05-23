using SMITFeeReimbursementSystem.ViewModels;

namespace SMITFeeReimbursementSystem.Services;

public interface IDashboardService
{
    Task<DashboardViewModel> GetDashboardDataAsync();
}

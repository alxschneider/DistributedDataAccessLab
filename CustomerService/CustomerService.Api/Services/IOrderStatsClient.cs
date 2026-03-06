namespace CustomerService.Api.Services;

public interface IOrderStatsClient
{
    Task<object?> GetBuyerDashboardAsync(int buyerId);
    Task<object?> GetSellerDashboardAsync(int sellerId);
}

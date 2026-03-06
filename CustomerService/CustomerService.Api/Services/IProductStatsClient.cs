namespace CustomerService.Api.Services;

public interface IProductStatsClient
{
    Task<object?> GetSellerProductsDashboardAsync(int sellerId);
}

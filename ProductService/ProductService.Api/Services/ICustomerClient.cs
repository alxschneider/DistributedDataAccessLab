namespace ProductService.Api.Services;

public interface ICustomerClient
{
    Task<bool> SellerExistsAsync(int sellerId);
}

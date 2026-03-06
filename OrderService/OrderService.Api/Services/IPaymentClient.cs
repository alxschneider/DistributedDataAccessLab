namespace OrderService.Api.Services;

public interface IPaymentClient
{
    Task<bool> RequestRefundAsync(int orderId);
}

namespace PaymentService.Api.Services;

public interface IOrderClient
{
    Task<bool> UpdateOrderStatusAsync(int orderId, string status);
}

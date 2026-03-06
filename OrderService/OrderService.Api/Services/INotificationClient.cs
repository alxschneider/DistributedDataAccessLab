namespace OrderService.Api.Services;

public interface INotificationClient
{
    Task SendNotificationAsync(int userId, string type, string title, string message);
}

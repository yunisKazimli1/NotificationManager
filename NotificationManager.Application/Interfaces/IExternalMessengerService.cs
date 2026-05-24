namespace NotificationManager.Application.Interfaces
{
    public interface IExternalMessengerService
    {
        Task SendAsync(string message);
    }
}

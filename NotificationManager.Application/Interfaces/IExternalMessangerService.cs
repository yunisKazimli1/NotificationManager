namespace NotificationManager.Application.Interfaces
{
    public interface IExternalMessangerService
    {
        Task SendAsync(string message);
    }
}

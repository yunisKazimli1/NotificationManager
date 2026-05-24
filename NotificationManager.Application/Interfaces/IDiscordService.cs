namespace NotificationManager.Application.Interfaces
{
    public interface IDiscordService
    {
        Task SendAsync(string message);
    }
}

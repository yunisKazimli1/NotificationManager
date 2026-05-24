using NotificationManager.Application.Dtos;

namespace NotificationManager.Application.Interfaces
{
    public interface INotificationService
    {
        public Task SendNotificationAsync(NotificationDto notificationDto);
    }
}

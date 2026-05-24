using NotificationManager.Application.Dtos;

namespace NotificationManager.Application.Interfaces
{
    public interface IAiMessageGenerator
    {
        Task<string> GenerateMessageAsync(NotificationDto dto);
    }
}

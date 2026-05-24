using Microsoft.Extensions.Logging;
using NotificationManager.Application.Dtos;
using NotificationManager.Application.Interfaces;
using NotificationManager.Domain.Enums;
using NotificationManager.Domain.Utilities.Exceptions;

namespace NotificationManager.Application.Implementations
{
    public class NotificationService(
        ILogger<NotificationService> _logger,
        IRateLimiter _rateLimiter,
        IAiMessageGenerator _ai,
        IExternalMessangerService _discord) : INotificationService
    {
        public async Task SendNotificationAsync(NotificationDto dto)
        {
            _logger.LogInformation("Processing notification with level {Level}", dto.NotificationLevel);

            if (!ShouldProcess(dto.NotificationLevel))
            {
                _logger.LogInformation("Notification ignored (level too low)");
                return;
            }

            if (!_rateLimiter.Allow())
            {
                _logger.LogWarning("Rate limit reached (10/min). Notification dropped.");

                throw new TooManyRequestsLocallyException();//expected TooManyRequestsException, because of business rule
            }

            var message = await _ai.GenerateMessageAsync(dto);

            await _discord.SendAsync(message);

            _logger.LogInformation("Notification successfully sent to Discord");
        }

        private bool ShouldProcess(NotificationLevel level)
        {
            return level >= NotificationLevel.Warning;
        }
    }
}

using Microsoft.AspNetCore.Mvc;
using NotificationManager.Application.Dtos;
using NotificationManager.Application.Interfaces;

namespace NotificationManager.Api.Controllers
{
    [ApiController]
    [Route("api/Notification")]
    public class NotificationController : ControllerBase
    {
        private readonly ILogger<NotificationController> _logger;
        private readonly INotificationService _notificationService;
        public NotificationController(ILogger<NotificationController> logger, INotificationService notificationService)
        {
            _logger = logger;
            _notificationService = notificationService;
        }

        [HttpPost]
        public async Task<IActionResult> Send([FromBody]NotificationDto notificationDto)
        {
            _logger.LogInformation(
                "Received notification. Title: {Title}, Level: {Level}, Message: {Message}",
                notificationDto.Title,
                notificationDto.NotificationLevel,
                notificationDto.Message
            );

            await _notificationService.SendNotificationAsync(notificationDto);

            return Accepted();
        }
    }
}

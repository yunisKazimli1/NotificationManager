using NotificationManager.Domain.Enums;
using System.ComponentModel.DataAnnotations;

namespace NotificationManager.Application.Dtos
{
    public class NotificationDto
    {
        [Required]
        public required string Title { get; set; }

        [Required]
        public required string Message { get; set; }

        [Required]
        public NotificationLevel NotificationLevel { get; set; }
    }
}

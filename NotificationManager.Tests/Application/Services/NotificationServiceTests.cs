using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using NotificationManager.Application.Dtos;
using NotificationManager.Application.Implementations;
using NotificationManager.Application.Interfaces;
using NotificationManager.Domain.Enums;
using NotificationManager.Domain.Utilities.Exceptions;
using Xunit;

namespace NotificationManager.Tests.Application.Services
{
    public class NotificationServiceTests
    {
        private readonly Mock<ILogger<NotificationService>> _logger = new();
        private readonly Mock<IRateLimiter> _rateLimiter = new();
        private readonly Mock<IAiMessageGenerator> _ai = new();
        private readonly Mock<IDiscordService> _discord = new();

        private readonly NotificationService _service;

        public NotificationServiceTests()
        {
            _service = new NotificationService(
                _logger.Object,
                _rateLimiter.Object,
                _ai.Object,
                _discord.Object);
        }

        [Fact]
        public async Task Should_ignore_notification_when_level_is_Info()
        {
            var dto = new NotificationDto
            {
                Title = "Test",
                Message = "Hello",
                NotificationLevel = NotificationLevel.Info
            };

            await _service.SendNotificationAsync(dto);

            _ai.Verify(x => x.GenerateMessageAsync(It.IsAny<NotificationDto>()), Times.Never);
            _discord.Verify(x => x.SendAsync(It.IsAny<string>()), Times.Never);
            _rateLimiter.Verify(x => x.Allow(), Times.Never);
        }

        [Fact]
        public async Task Should_process_warning_and_send_to_discord()
        {
            var dto = new NotificationDto
            {
                Title = "CPU High",
                Message = "CPU 95%",
                NotificationLevel = NotificationLevel.Warning
            };

            _rateLimiter.Setup(x => x.Allow()).Returns(true);
            _ai.Setup(x => x.GenerateMessageAsync(dto)).ReturnsAsync("ALERT: CPU high");

            await _service.SendNotificationAsync(dto);

            _rateLimiter.Verify(x => x.Allow(), Times.Once);
            _ai.Verify(x => x.GenerateMessageAsync(dto), Times.Once);
            _discord.Verify(x => x.SendAsync("ALERT: CPU high"), Times.Once);
        }

        [Fact]
        public async Task Should_throw_exception_when_rate_limit_exceeded()
        {
            var dto = new NotificationDto
            {
                Title = "Test",
                Message = "Test",
                NotificationLevel = NotificationLevel.Warning
            };

            _rateLimiter.Setup(x => x.Allow()).Returns(false);

            Func<Task> act = async () => await _service.SendNotificationAsync(dto);

            await act.Should()
                .ThrowAsync<TooManyRequestsLocallyException>();

            _ai.Verify(x => x.GenerateMessageAsync(It.IsAny<NotificationDto>()), Times.Never);
            _discord.Verify(x => x.SendAsync(It.IsAny<string>()), Times.Never);
        }
    }
}
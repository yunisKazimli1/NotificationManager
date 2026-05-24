using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using Moq.Protected;
using NotificationManager.Infrastructure.Implementation;
using System.Net;
using Xunit;

namespace NotificationManager.Tests.Infrastructure.Discord
{
    public class DiscordServiceTests
    {
        private DiscordService CreateSut(
            HttpResponseMessage response,
            string? webhookUrl = "https://discord.com/webhook")
        {
            var handlerMock = new Mock<HttpMessageHandler>();

            handlerMock
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(response);

            var httpClient = new HttpClient(handlerMock.Object);

            var configMock = new Mock<IConfiguration>();
            configMock.Setup(x => x["Discord:WebhookUrl"])
                      .Returns(webhookUrl);

            var loggerMock = new Mock<ILogger<DiscordService>>();

            return new DiscordService(
                httpClient,
                configMock.Object,
                loggerMock.Object);
        }

        [Fact]
        public async Task Should_not_send_when_webhook_missing()
        {
            var sut = CreateSut(
                new HttpResponseMessage(HttpStatusCode.OK),
                webhookUrl: null);

            await sut.SendAsync("test message");

            // no exception expected
        }

        [Fact]
        public async Task Should_send_message_successfully()
        {
            var response = new HttpResponseMessage(HttpStatusCode.OK);

            var sut = CreateSut(response);

            await sut.SendAsync("hello discord");

            response.StatusCode.Should().Be(HttpStatusCode.OK);
        }

        [Fact]
        public async Task Should_handle_failed_response()
        {
            var response = new HttpResponseMessage(HttpStatusCode.BadRequest)
            {
                Content = new StringContent("Invalid webhook")
            };

            var sut = CreateSut(response);

            await sut.SendAsync("hello discord");

            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }
    }
}
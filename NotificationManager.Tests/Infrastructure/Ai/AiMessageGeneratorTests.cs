using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using Moq.Protected;
using NotificationManager.Application.Dtos;
using NotificationManager.Domain.Enums;
using NotificationManager.Domain.Utilities.Exceptions;
using NotificationManager.Infrastructure.Implementation;
using System.Net;
using System.Text;
using Xunit;

namespace NotificationManager.Tests.Infrastructure.Ai
{
    public class AiMessageGeneratorTests
    {
        private AiMessageGenerator CreateSut(HttpResponseMessage response)
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
            configMock.Setup(x => x["OpenAI:ApiKey"])
                      .Returns("test-key");

            var loggerMock = new Mock<ILogger<AiMessageGenerator>>();

            return new AiMessageGenerator(httpClient, configMock.Object, loggerMock.Object);
        }

        [Fact]
        public async Task Should_return_message_when_api_succeeds()
        {
            var json = """
            {
                "choices": [
                    {
                        "message": {
                            "content": "Test alert message"
                        }
                    }
                ]
            }
            """;

            var response = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(json, Encoding.UTF8, "application/json")
            };

            var sut = CreateSut(response);

            var dto = new NotificationDto
            {
                Title = "CPU",
                Message = "High usage",
                NotificationLevel = NotificationLevel.Warning
            };

            var result = await sut.GenerateMessageAsync(dto);

            result.Should().Be("Test alert message");
        }

        [Fact]
        public async Task Should_throw_TooManyRequestsException_when_429()
        {
            var response = new HttpResponseMessage(HttpStatusCode.TooManyRequests);

            var sut = CreateSut(response);

            var dto = new NotificationDto
            {
                Title = "CPU",
                Message = "High usage",
                NotificationLevel = NotificationLevel.Warning
            };

            Func<Task> act = async () => await sut.GenerateMessageAsync(dto);

            await act.Should()
                .ThrowAsync<TooManyRequestsException>();
        }

        [Fact]
        public async Task Should_throw_when_api_key_missing()
        {
            var handlerMock = new Mock<HttpMessageHandler>();

            var httpClient = new HttpClient(handlerMock.Object);

            var configMock = new Mock<IConfiguration>();
            configMock.Setup(x => x["OpenAI:ApiKey"])
                      .Returns((string)null);

            var loggerMock = new Mock<ILogger<AiMessageGenerator>>();

            var sut = new AiMessageGenerator(httpClient, configMock.Object, loggerMock.Object);

            var dto = new NotificationDto
            {
                Title = "Test",
                Message = "Test",
                NotificationLevel = NotificationLevel.Warning
            };

            Func<Task> act = async () => await sut.GenerateMessageAsync(dto);

            await act.Should()
                .ThrowAsync<Exception>()
                .WithMessage("Missing OpenRouter/OpenAI API key");
        }
    }
}
using FluentAssertions;
using Moq;
using NotificationManager.Application.Dtos;
using NotificationManager.Domain.Enums;
using System.Net;
using System.Net.Http.Json;
using Xunit;

public class NotificationApiTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _client;
    private readonly CustomWebApplicationFactory _factory;

    public NotificationApiTests(CustomWebApplicationFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task Send_ShouldReturnAccepted_WhenRequestIsValid()
    {
        // Arrange
        var dto = new NotificationDto
        {
            Title = "Test Title",
            Message = "Test Message",
            NotificationLevel = NotificationManager.Domain.Enums.NotificationLevel.Info
        };

        _factory.NotificationServiceMock
            .Setup(x => x.SendNotificationAsync(It.IsAny<NotificationDto>()))
            .Returns(Task.CompletedTask);

        // Act
        var response = await _client.PostAsJsonAsync("/api/Notification", dto);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Accepted);

        _factory.NotificationServiceMock.Verify(
            x => x.SendNotificationAsync(It.Is<NotificationDto>(d =>
                d.Title == "Test Title" &&
                d.Message == "Test Message"
            )),
            Times.Once
        );
    }

    [Fact]
    public async Task Send_ShouldReturn429_After10Requests()
    {
        // Arrange
        var dto = new NotificationDto
        {
            Title = "Rate limit test",
            Message = "Testing rate limiter",
            NotificationLevel = NotificationLevel.Warning
        };

        HttpResponseMessage response = null!;

        // Act - send 11 requests quickly
        for (int i = 1; i <= 11; i++)
        {
            response = await _client.PostAsJsonAsync("/api/Notification", dto);
        }

        // Assert

        response.StatusCode.Should().Be(HttpStatusCode.TooManyRequests);
    }

    [Fact]
    public async Task Send_ShouldReturn500_WhenUnexpectedExceptionOccurs()
    {
        // Arrange
        var dto = new NotificationDto
        {
            Title = "Exception test",
            Message = "Force failure",
            NotificationLevel = NotificationLevel.Warning
        };

        _factory.DiscordServiceMock
            .Setup(x => x.SendAsync(It.IsAny<string>()))
            .ThrowsAsync(new Exception("Discord failure"));

        // Act
        var response = await _client.PostAsJsonAsync("/api/Notification", dto);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.InternalServerError);

        var content = await response.Content.ReadAsStringAsync();
        response.StatusCode.Should().Be(HttpStatusCode.InternalServerError);
    }

    [Fact]
    public async Task Send_ShouldReturnBadRequest_WhenRequestBodyIsInvalid()
    {
        // Arrange
        var invalidDto = new NotificationDto
        {
            Title = null,   // invalid
            Message = null  // invalid
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/Notification", invalidDto);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Health_ShouldReturn200Ok()
    {
        // Act
        var response = await _client.GetAsync("/health");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }
}
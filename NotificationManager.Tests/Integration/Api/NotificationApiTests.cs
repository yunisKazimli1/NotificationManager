using FluentAssertions;
using Moq;
using NotificationManager.Application.Dtos;
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
}
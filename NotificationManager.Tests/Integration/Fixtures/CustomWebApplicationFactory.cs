using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Moq;
using NotificationManager.Application.Interfaces;

namespace NotificationManager.Tests.Integration.Fixtures
{
    public class CustomWebApplicationFactory : WebApplicationFactory<Program>
    {
        public Mock<IExternalMessengerService> DiscordServiceMock { get; } = new Mock<IExternalMessengerService>();

        public Mock<INotificationService> NotificationServiceMock { get; } = new Mock<INotificationService>();

        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.ConfigureServices(services =>
            {
                // Remove real service registration
                services.RemoveAll(typeof(IExternalMessengerService));

                // Register mock
                services.AddScoped(_ => DiscordServiceMock.Object);
            });
        }
    }
}
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
        public Mock<IExternalMessangerService> DiscordServiceMock { get; } = new Mock<IExternalMessangerService>();

        public Mock<INotificationService> NotificationServiceMock { get; } = new Mock<INotificationService>();

        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.ConfigureServices(services =>
            {
                // Remove real service registration
                //services.RemoveAll(typeof(INotificationService));
                services.RemoveAll(typeof(IExternalMessangerService));

                // Register mock
                //services.AddScoped(_ => NotificationServiceMock.Object);
                services.AddScoped(_ => DiscordServiceMock.Object);
            });
        }
    }
}
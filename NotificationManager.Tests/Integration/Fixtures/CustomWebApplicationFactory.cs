using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.VisualStudio.TestPlatform.TestHost;
using Moq;
using NotificationManager.Application.Interfaces;

public class CustomWebApplicationFactory : WebApplicationFactory<Program>
{
    public Mock<IDiscordService> DiscordServiceMock { get; } = new Mock<IDiscordService>();

    public Mock<INotificationService> NotificationServiceMock { get; } = new Mock<INotificationService>();

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureServices(services =>
        {
            // Remove real service registration
            services.RemoveAll(typeof(INotificationService));
            services.RemoveAll(typeof(IDiscordService));

            // Register mock
            services.AddScoped(_ => NotificationServiceMock.Object);
            services.AddScoped(_ => DiscordServiceMock.Object);
        });
    }
}
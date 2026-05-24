using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.VisualStudio.TestPlatform.TestHost;
using Moq;
using NotificationManager.Application.Interfaces;

public class CustomWebApplicationFactory : WebApplicationFactory<Program>
{
    public Mock<INotificationService> NotificationServiceMock { get; }
        = new Mock<INotificationService>();

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureServices(services =>
        {
            // Remove real service registration
            services.RemoveAll(typeof(INotificationService));

            // Register mock
            services.AddScoped(_ => NotificationServiceMock.Object);
        });
    }
}
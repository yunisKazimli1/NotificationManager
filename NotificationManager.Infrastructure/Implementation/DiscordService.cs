using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using NotificationManager.Application.Interfaces;
using System.Net.Http.Json;

namespace NotificationManager.Infrastructure.Implementation
{
    public class DiscordService(
        HttpClient _httpClient,
        IConfiguration _configuration,
        ILogger<DiscordService> _logger) : IExternalMessengerService
    {

        public async Task SendAsync(string message)
        {
            var webhookUrl = _configuration["Discord:WebhookUrl"];

            if (string.IsNullOrWhiteSpace(webhookUrl))
            {
                throw new Exception("Discord webhook URL is missing");
            }

            _logger.LogInformation("Sending message to Discord");

            var payload = new
            {
                content = message
            };

            var response = await _httpClient.PostAsJsonAsync(webhookUrl, payload);

            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadAsStringAsync();

                throw new Exception($"Failed to send Discord message. Status: {response.StatusCode}, Error: {error}");
            }
            else
            {
                _logger.LogInformation("Discord message sent successfully");
            }
        }
    }
}

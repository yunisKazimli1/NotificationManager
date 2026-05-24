using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using NotificationManager.Application.Dtos;
using NotificationManager.Application.Interfaces;
using NotificationManager.Domain.Utilities.Exceptions;
using NotificationManager.Infrastructure.Dtos;
using System.Net.Http.Headers;
using System.Net.Http.Json;

namespace NotificationManager.Infrastructure.Implementation
{
    public class AiMessageGenerator(
        HttpClient httpClient,
        IConfiguration configuration,
        ILogger<AiMessageGenerator> logger) : IAiMessageGenerator
    {
        private readonly HttpClient _httpClient = httpClient;
        private readonly IConfiguration _configuration = configuration;
        private readonly ILogger<AiMessageGenerator> _logger = logger;

        public async Task<string> GenerateMessageAsync(NotificationDto dto)
        {
            _logger.LogInformation("Generating AI message for level {Level}", dto.NotificationLevel);

            var apiKey = _configuration["OpenAI:ApiKey"]?.Trim();

            if (string.IsNullOrWhiteSpace(apiKey))
                throw new Exception("Missing OpenRouter/OpenAI API key");

            var request = new HttpRequestMessage(
                HttpMethod.Post,
                "https://openrouter.ai/api/v1/chat/completions");

            request.Headers.Authorization =
                new AuthenticationHeaderValue("Bearer", apiKey);

            request.Headers.Add("HTTP-Referer", "http://localhost");
            request.Headers.Add("X-Title", "NotificationManager");

            var body = new
            {
                model = "openai/gpt-4o-mini",
                messages = new[]
                {
                    new { role = "system", content = "You generate alert messages based on system notifications." },
                    new { role = "user", content = BuildPrompt(dto) }
                }
            };

            request.Content = JsonContent.Create(body);

            var response = await _httpClient.SendAsync(request);

            _logger.LogInformation(
                "AI response: {StatusCode} {ReasonPhrase}",
                response.StatusCode,
                response.ReasonPhrase);

            if (response.StatusCode == System.Net.HttpStatusCode.TooManyRequests)
                throw new Exception("OpenAi router api usage limit has been exceeded");

            response.EnsureSuccessStatusCode();

            var result = await response.Content.ReadFromJsonAsync<AiResponse>();

            return result?.Choices?.FirstOrDefault()?.Message?.Content
                   ?? "No message generated";
        }

        private string BuildPrompt(NotificationDto dto)
        {
            return $"""
                A system notification occurred.

                Title: {dto.Title}
                Message: {dto.Message}
                Level: {dto.NotificationLevel}

                Generate a clear Discord alert message.
                """;
        }
    }
}
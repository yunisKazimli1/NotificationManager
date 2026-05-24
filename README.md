# NotificationManager

A .NET 8 Web API that receives system notifications via HTTP, analyzes them using an AI model, and forwards critical alerts to Discord via webhook. Includes a built-in sliding-window rate limiter capped at 10 outbound messages per minute.

---

## Table of Contents

- [Architecture](#architecture)
- [How It Works](#how-it-works)
- [Project Structure](#project-structure)
- [Configuration](#configuration)
- [Running the Project](#running-the-project)
- [API Reference](#api-reference)
- [Testing](#testing)
- [Design Decisions](#design-decisions)

---

## Architecture

The solution follows **Clean Architecture** with four layers, each with a clearly defined responsibility:

```
NotificationManager (API)
    └── NotificationManager.Application
            └── NotificationManager.Domain
NotificationManager.Infrastructure
    └── NotificationManager.Application (interfaces only)
```

| Layer | Project | Responsibility |
|---|---|---|
| **API** | `NotificationManager` | Controllers, middleware, DI wiring |
| **Application** | `NotificationManager.Application` | Business logic, interfaces, DTOs |
| **Domain** | `NotificationManager.Domain` | Enums, custom exceptions |
| **Infrastructure** | `NotificationManager.Infrastructure` | HTTP clients for Discord and OpenRouter AI |

Dependencies only point inward. Infrastructure knows about Application interfaces; Application knows nothing about Infrastructure implementations. This allows swapping Discord for Slack, or OpenRouter for any other AI provider, without touching business logic.

---

## How It Works

1. A client sends a `POST /api/Notification` request with a title, message, and notification level.
2. The `NotificationController` passes the payload to `NotificationService`.
3. The service checks whether the level is `Warning`, `Error`, or `Critical`. If the level is `Info`, the notification is silently ignored.
4. If the level qualifies, the rate limiter is consulted. If 10 or more messages have already been forwarded in the last 60 seconds, a `429 Too Many Requests` response is returned.
5. Otherwise, the payload is sent to the AI generator (`AiMessageGenerator`), which calls the OpenRouter API (using `openai/gpt-4o-mini`) and asks it to produce a clear, human-readable Discord alert message.
6. The generated message is forwarded to Discord via webhook (`DiscordService`).
7. If Discord returns an error, an exception is thrown and the global exception middleware returns a `500` response with a trace ID.

### Notification Levels

| Level | Value | Forwarded to Discord? |
|---|---|---|
| Info | 0 | No |
| Warning | 1 | Yes |
| Error | 2 | Yes |
| Critical | 3 | Yes |

### Rate Limiting

The rate limiter uses a sliding window backed by a `Queue<DateTime>`. On each call it evicts timestamps older than 60 seconds, then checks whether the count has reached 10. If so, it returns `false` and the service throws `TooManyRequestsLocallyException`, which the middleware maps to HTTP 429. The limiter is registered as a `Singleton` so the counter is shared across all requests.

---

## Project Structure

```
NotificationManager.sln
│
├── NotificationManager/                        # API layer
│   ├── Controllers/
│   │   └── NotificationController.cs
│   ├── Middleware/
│   │   ├── ExceptionHandlingMiddleware.cs
│   │   └── Extensions/
│   │       └── ExceptionHandlingMiddlewareExtensions.cs
│   ├── Program.cs
│   └── appsettings.json
│
├── NotificationManager.Application/            # Business logic
│   ├── Dtos/
│   │   └── NotificationDto.cs
│   ├── Implementations/
│   │   ├── NotificationService.cs
│   │   └── RateLimiter.cs
│   └── Interfaces/
│       ├── INotificationService.cs
│       ├── IRateLimiter.cs
│       ├── IAiMessageGenerator.cs
│       └── IExternalMessengerService.cs
│
├── NotificationManager.Domain/                 # Core types
│   ├── Enums/
│   │   └── NotificationLevel.cs
│   └── Utilities/Exceptions/
│       └── TooManyRequestsLocallyException.cs
│
├── NotificationManager.Infrastructure/         # External integrations
│   ├── Dtos/
│   │   └── AiResponse.cs
│   └── Implementation/
│       ├── AiMessageGenerator.cs
│       └── DiscordService.cs
│
└── NotificationManager.Tests/                  # Unit and integration tests
    ├── Application/
    │   ├── RateLimit/
    │   │   └── RateLimiterTests.cs
    │   └── Services/
    │       └── NotificationServiceTests.cs
    ├── Infrastructure/
    │   ├── Ai/
    │   │   └── AiMessageGeneratorTests.cs
    │   └── Discord/
    │       └── DiscordServiceTests.cs
    └── Integration/
        ├── Api/
        │   └── NotificationApiTests.cs
        └── Fixtures/
            └── CustomWebApplicationFactory.cs
```

---

## Configuration

The application requires two configuration values in `appsettings.json`:

```json
{
  "OpenAI": {
    "ApiKey": "<your-openrouter-api-key>"
  },
  "Discord": {
    "WebhookUrl": "<your-discord-webhook-url>"
  }
}
```

> **Note on credentials in appsettings.json:** For this task, the API key and Discord webhook URL are committed directly to `appsettings.json` for convenience — this makes it immediately runnable without any additional setup steps. In a real production project, secrets would never be stored in source control. The correct approach would be to use environment variables, `dotnet user-secrets` for local development, and a secrets manager such as Azure Key Vault or AWS Secrets Manager in production.

The `OpenAI:ApiKey` value is an [OpenRouter](https://openrouter.ai) key. OpenRouter is used as a proxy for OpenAI-compatible models, and this project uses `openai/gpt-4o-mini`.

---

## Running the Project

**Prerequisites:** .NET 8 SDK

```bash
git clone https://github.com/yunisKazimli1/NotificationManager.git
cd NotificationManager
dotnet run --project NotificationManager
```

The API will start at `https://localhost:7xxx`. Swagger UI is available at `/swagger` in Development mode.

A health check endpoint is available at `/health`.

---

## API Reference

### POST /api/Notification

Receives a notification payload. If the level is `Warning` or higher, an AI-generated alert is forwarded to Discord.

**Request body:**

```json
{
  "title": "High CPU usage",
  "message": "CPU has been above 90% for 5 minutes on server-01",
  "notificationLevel": 1
}
```

| Field | Type | Required | Description |
|---|---|---|---|
| `title` | string | Yes | Short summary of the notification |
| `message` | string | Yes | Full notification body |
| `notificationLevel` | int | Yes | 0 = Info, 1 = Warning, 2 = Error, 3 = Critical |

**Responses:**

| Status | When |
|---|---|
| `202 Accepted` | Notification received and processed successfully |
| `400 Bad Request` | Missing required fields |
| `429 Too Many Requests` | Rate limit of 10/min exceeded |
| `500 Internal Server Error` | Unexpected error (AI or Discord call failed) |

---

## Testing

```bash
dotnet test
```

The test suite covers:

- **Unit tests** — `NotificationService`, `RateLimiter`, `AiMessageGenerator`, `DiscordService`, each tested in isolation using Moq
- **Integration tests** — full HTTP pipeline via `WebApplicationFactory`, covering the happy path, rate limiting (11 consecutive requests), bad request validation, and the health check endpoint

Infrastructure HTTP calls (OpenRouter and Discord) are mocked at the `HttpMessageHandler` level using `Moq.Protected`, so no real network calls are made in tests.

---

## Design Decisions

**`IExternalMessengerService` instead of `IDiscordService`**
The interface is named generically so the application layer has no direct dependency on Discord. Replacing the Discord implementation with Slack, Teams, or any other messenger requires only a new `Infrastructure` class and a one-line DI change in `Program.cs` — the rest of the codebase is unaffected.

**Rate limiter as `Singleton`**
The `RateLimiter` must be registered as a `Singleton` because it maintains shared in-memory state (the timestamp queue). A `Scoped` registration would create a new instance per request, meaning the counter would reset on every call and never actually limit anything.

**Global exception middleware**
Rather than wrapping every service call in try/catch blocks, exception handling is centralized in `ExceptionHandlingMiddleware`. This keeps controllers and services clean, and maps domain-specific exceptions to the correct HTTP status codes in one place.

**No FluentValidation**
The project has only one DTO (`NotificationDto`) with two required string fields. Adding FluentValidation for this would introduce a dependency and configuration overhead that is not justified at this scale. The built-in `[Required]` data annotations on the DTO, combined with ASP.NET Core's automatic model state validation via `[ApiController]`, are sufficient. In a project with multiple complex DTOs or conditional validation rules, FluentValidation would be the right choice.

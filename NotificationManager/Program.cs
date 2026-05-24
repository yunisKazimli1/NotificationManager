using Microsoft.AspNetCore.Diagnostics;
using NotificationManager.Application.Implementations;
using NotificationManager.Application.Interfaces;
using NotificationManager.Domain.Utilities.Exceptions;
using NotificationManager.Infrastructure.Implementation;
using System.ComponentModel.DataAnnotations;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddHttpClient();
builder.Services.AddHealthChecks();
builder.Services.AddScoped<INotificationService, NotificationService>();
builder.Services.AddScoped<IRateLimiter, RateLimiter>();
builder.Services.AddHttpClient<IDiscordService, DiscordService>();
builder.Services.AddHttpClient<IAiMessageGenerator, AiMessageGenerator>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.MapHealthChecks("/health");

app.UseAuthorization();

app.MapControllers();

app.UseExceptionHandler(errorApp =>
{
    errorApp.Run(async context =>
    {
        var exception = context.Features
            .Get<IExceptionHandlerFeature>()?
            .Error;

        var logger = context.RequestServices
            .GetRequiredService<ILogger<Program>>();

        logger.LogError(exception, "Unhandled exception occurred");

        context.Response.ContentType = "application/json";

        context.Response.StatusCode = exception switch
        {
            TooManyRequestsException => StatusCodes.Status429TooManyRequests,
            _ => StatusCodes.Status500InternalServerError
        };

        await context.Response.WriteAsJsonAsync(new
        {
            message = exception switch 
            { 
                TooManyRequestsLocallyException => "Rate limit exceeded. Please try again later.",
                _ => "An unexpected error occurred."
            },
            traceId = context.TraceIdentifier
        });
    });
});

app.Run();

public partial class Program { }

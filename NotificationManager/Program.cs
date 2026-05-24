using NotificationManager.Api.Middleware.Extensions;
using NotificationManager.Application.Implementations;
using NotificationManager.Application.Interfaces;
using NotificationManager.Infrastructure.Implementation;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddHttpClient();
builder.Services.AddHealthChecks();
builder.Services.AddScoped<INotificationService, NotificationService>();
builder.Services.AddSingleton<IRateLimiter, RateLimiter>();
builder.Services.AddHttpClient<IExternalMessengerService, DiscordService>();
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

app.UseGlobalExceptionHandling();

app.Run();

public partial class Program { }

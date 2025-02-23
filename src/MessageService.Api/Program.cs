using MessageService.Dal;
using MessageService.Api;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Http;
using System.Net.WebSockets;
using System.Text.Json;
using System.Text;
using System.Collections.Concurrent;
using System.Diagnostics;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

string connectionString = "Host=db;Port=5432;Database=messagesdb;Username=user;Password=password";

builder.Services.AddSingleton<IMessageRepository>(sp => new MessageRepository(connectionString));
builder.Services.AddControllers();
builder.Services.AddSingleton<WebSocketHandler>();

builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.WithOrigins("http://localhost:8081")
              .AllowAnyMethod() 
              .AllowAnyHeader(); 
    });
});

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Message Service API",
        Version = "v1",
        Description = "Simple message service with WebSocket",
        Contact = new OpenApiContact
        {
            Name = "Dmitriy Balakshin",
            Email = "nazerudb@gmail.com",
            Url = new Uri("https://github.com/nazeru")
        }
    });
});

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI(options =>
{
    options.SwaggerEndpoint("/swagger/v1/swagger.json", "Message Service API v1");
    options.RoutePrefix = "swagger"; 
});

var logger = app.Services.GetRequiredService<ILogger<Program>>();
app.Use(async (context, next) =>
{
    var stopwatch = Stopwatch.StartNew();
    await next();
    stopwatch.Stop();

    var logMessage = $"{context.Connection.RemoteIpAddress} - [{DateTime.UtcNow:dd/MMM/yyyy:HH:mm:ss zzz}] " +
                     $"\"{context.Request.Method} {context.Request.Path} {context.Request.Protocol}\" " +
                     $"{context.Response.StatusCode} {stopwatch.ElapsedMilliseconds}ms";

    logger.LogInformation(logMessage);
});

app.UseCors();

app.UseRouting();
app.UseEndpoints(endpoints =>
{
    endpoints.MapControllers();
});
app.UseWebSockets();

app.Use(async (context, next) =>
{
    if (context.Request.Path == "/ws")
    {
        if (context.WebSockets.IsWebSocketRequest)
        {
            using var webSocket = await context.WebSockets.AcceptWebSocketAsync();
            var webSocketHandler = app.Services.GetRequiredService<WebSocketHandler>();
            logger.LogInformation($"{context.Connection.RemoteIpAddress} - WebSocket-connection successfully");
            await webSocketHandler.HandleConnection(webSocket);
        }
        else
        {
            context.Response.StatusCode = 400;
            logger.LogWarning($"{context.Connection.RemoteIpAddress} - Error: WebSocket-connection with HTTP");
        }
    }
    else
    {
        await next(context);
    }

});

logger.LogInformation("API is running!");
app.Run();

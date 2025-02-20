using Application;
using Application.Common.Abstractions;
using Infrastructure;
using Infrastructure.Communications;
using Serilog;
using WebApi;

var builder = WebApplication.CreateBuilder(args);

// Configure Services

builder.Host
    .UseSerilog((context, loggerContext)
        => loggerContext.ReadFrom.Configuration(context.Configuration));

// Add services to the container.
builder.Services.AddApplicationServices();
builder.Services.AddInfrastructureServices(builder.Configuration);
builder.Services.AddWebServices(builder.Configuration);


// Configure the HTTP request pipeline.

var app = builder.Build();

// Set the service provider
ServiceLocator.ServiceProvider = app.Services;

// Configure pipeline
app.UseApplicationPipeline();

app.MapHub<NotificationHub>("/notificationHub").RequireAuthorization();

app.Map("/", () => Results.Redirect("/api"));

app.MapEndpoints();

app.Run();

public partial class Program { }

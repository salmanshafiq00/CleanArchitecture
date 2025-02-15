using Application.Common.Abstractions.Communication;
using Microsoft.Extensions.DependencyInjection;

namespace Infrastructure.Communications;

internal static class NotificationServiceExtentension
{
    public static IServiceCollection AddNotificationServices(
        this IServiceCollection services)
    {
        services.AddSignalR(options =>
        {
            options.EnableDetailedErrors = true;
            options.MaximumReceiveMessageSize = 102400; // 100KB
            options.ClientTimeoutInterval = TimeSpan.FromSeconds(30);
            options.KeepAliveInterval = TimeSpan.FromSeconds(15);
        });

        //services.AddHostedService<NotificationProcessingService>();
        services.AddScoped<INotificationService, NotificationService>();

        return services;
    }
}

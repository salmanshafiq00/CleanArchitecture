using Application.Common.Abstractions.Communication;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System.Net.Mail;

namespace Infrastructure.Communications;

internal static class EmailServiceExtension
{
    public static IServiceCollection AddEmailServices(
            this IServiceCollection services,
            IConfiguration configuration)
    {
        // Configure SMTP client
        var smtpClient = new SmtpClient(
            configuration["Email:SmtpServer"],
            int.Parse(configuration["Email:Port"]))
        {
            EnableSsl = bool.Parse(configuration["Email:EnableSsl"]),
            Credentials = new System.Net.NetworkCredential(
                configuration["Email:Username"],
                configuration["Email:Password"])
        };

        // Configure FluentEmail
        services
            .AddFluentEmail(configuration["Email:DefaultFromEmail"], configuration["Email:DisplayName"])
            .AddRazorRenderer()
            .AddSmtpSender(smtpClient);

        // Register Email Service
        services.AddTransient<IEmailService, EmailService>();

        return services;
    }
}

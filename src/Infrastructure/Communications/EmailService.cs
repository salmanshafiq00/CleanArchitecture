using Application.Common.Abstractions.Communication;
using FluentEmail.Core;
using Microsoft.Extensions.Configuration;

namespace Infrastructure.Communications;

public class EmailService(
    IFluentEmail email, IConfiguration configuration) : IEmailService
{
    private readonly IFluentEmail _email = email;
    private readonly string _displayName = configuration["Email:DisplayName"]!;
    private readonly string _defaultFromEmail = configuration["Email:DefaultFromEmail"]!;

    public async Task<bool> SendEmailAsync(
        string to,
        string subject,
        string body,
        string? from = null)
    {
        var response = await _email
            .SetFrom(from ?? _defaultFromEmail, _displayName)
            .To(to)
            .Subject(subject)
            .Body(body)
            .SendAsync();

        return response.Successful;
    }

    public async Task<bool> SendHtmlEmailAsync(
        string to,
        string subject,
        string htmlBody,
        string? from = null)
    {
        var response = await _email
            .SetFrom(from ?? _defaultFromEmail, _displayName)
            .To(to)
            .Subject(subject)
            .Body(htmlBody, true)
            .SendAsync();

        return response.Successful;
    }

    public async Task<bool> SendTemplateEmailAsync<TModel>(
        string to,
        string subject,
        TModel templateModel,
        string? templatePath = null,
        string? from = null)
    {
        if (!File.Exists(templatePath))
            throw new FileNotFoundException($"Template not found");

        var templateContent = await File.ReadAllTextAsync(templatePath);

        var response = await _email
            .SetFrom(from ?? _defaultFromEmail, _displayName)
            .To(to)
            .Subject(subject)
            .UsingTemplate(templateContent, templateModel)
            .SendAsync();

        return response.Successful;
    }

    public async Task<bool> SendEmailWithAttachmentAsync(
        string to,
        string subject,
        string body,
        string attachmentPath,
        string? from = null,
        bool isHtml = false)
    {
        if (!File.Exists(attachmentPath))
            throw new FileNotFoundException($"Attachment {attachmentPath} not found");

        var response = await _email
            .SetFrom(from ?? _defaultFromEmail, _displayName)
            .To(to)
            .Subject(subject)
            .Body(body, isHtml)
            .AttachFromFilename(attachmentPath)
            .SendAsync();

        return response.Successful;
    }

    public async Task<bool> SendBulkEmailAsync(
        string[] to,
        string subject,
        string body,
        string? from = null,
        bool isHtml = false)
    {
        var email = _email
            .SetFrom(from ?? _defaultFromEmail, _displayName)
            .Subject(subject)
            .Body(body, isHtml);

        foreach (var recipient in to)
        {
            email.To(recipient);
        }

        var response = await email.SendAsync();
        return response.Successful;
    }
}

namespace Application.Common.Abstractions.Communication;

public interface IEmailService
{
    /// <summary>
    /// Send a simple text email
    /// </summary>
    Task<bool> SendEmailAsync(
        string to,
        string subject,
        string body,
        string? from = null);

    /// <summary>
    /// Send an HTML email
    /// </summary>
    Task<bool> SendHtmlEmailAsync(
        string to,
        string subject,
        string htmlBody,
        string? from = null);

    /// <summary>
    /// Send an email using a Razor template
    /// </summary>
    Task<bool> SendTemplateEmailAsync<TModel>(
        string to,
        string subject,
        TModel templateModel,
        string? templatePath = null,
        string? from = null);

    /// <summary>
    /// Send an email with attachment
    /// </summary>
    Task<bool> SendEmailWithAttachmentAsync(
        string to,
        string subject,
        string body,
        string attachmentPath,
        string? from = null,
        bool isHtml = false);

    /// <summary>
    /// Send a bulk email to multiple recipients
    /// </summary>
    Task<bool> SendBulkEmailAsync(
        string[] to,
        string subject,
        string body,
        string? from = null,
        bool isHtml = false);
}

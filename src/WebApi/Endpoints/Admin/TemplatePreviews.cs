using FluentEmail.Core.Interfaces;

namespace WebApi.Endpoints.Admin;

public class TemplatePreviews : EndpointGroupBase
{
    public override void Map(WebApplication app)
    {
        var group = app.MapGroup(this);

        group.MapGet("PreviewForgotPassword", PreviewForgotPassword)
            .AllowAnonymous()
            .WithName("PreviewForgotPassword")
            .Produces<IResult>(StatusCodes.Status200OK);

        group.MapGet("ResetPasswordConfirmation", ResetPasswordConfirmation)
            .AllowAnonymous()
            .WithName("ResetPasswordConfirmation")
            .Produces<IResult>(StatusCodes.Status200OK);

    }

    public async Task<IResult> PreviewForgotPassword(ITemplateRenderer razorRenderer)
    {
        var model = new
        {
            ReceiverName = "John",
            ResetLink = "https://your-app.com/reset-password?token=abc123"
        };

        var templatePath = Path.Combine(Directory.GetCurrentDirectory(), "Templates", "ForgotPassword", "ForgotPassword.cshtml");

        var templateContent = File.ReadAllText(templatePath);

        var htmlContent = await razorRenderer.ParseAsync(templateContent, model);

        return Results.Content(htmlContent, "text/html");
    }

    public async Task<IResult> ResetPasswordConfirmation(ITemplateRenderer razorRenderer)
    {
        var model = new
        {
            ReceiverName = "John",
            SiteLink = "https://your-app.com/"
        };

        var templatePath = Path.Combine(Directory.GetCurrentDirectory(), "Templates", "ForgotPassword", "ResetPasswordConfirmation.cshtml");

        var templateContent = File.ReadAllText(templatePath);

        var htmlContent = await razorRenderer.ParseAsync(templateContent, model);

        return Results.Content(htmlContent, "text/html");
    }
}

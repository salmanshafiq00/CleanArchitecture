using Infrastructure.BackgroundJobs;
using HealthChecks.UI.Client;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.Extensions.FileProviders;
using Serilog;
using WebApi.Extensions;
using WebApi.Middlewares;
using Infrastructure.Identity;
using Infrastructure.Persistence;

namespace WebApi;

internal static class ApplicationBuilderExtensions
{
    public static IApplicationBuilder UseApplicationPipeline(
        this WebApplication app)
    {
        if (app.Environment.IsDevelopment())
        {
             //app.IdentityInitialiseDatabaseAsync();
             //app.AppInitialiseDatabaseAsync();

            app.UseSwaggerUi(settings =>
            {
                settings.Path = "/api";
                settings.DocumentPath = "/api/specification.json";
            });
        }
        else
        {
            app.UseHsts();
        }

        app.UseExceptionHandler(options => { });
        app.UseMiddleware<RequestContextLoggingMiddleware>();
        app.UseSerilogRequestLogging();

        app.UseCors(ApiConstants.AllowOriginPolicy);

        app.UseHttpsRedirection();

        app.UseStaticFiles();
        app.ConfigureResourcesFolder();

        app.UseAuthentication();
        app.UseRouting();
        app.UseAuthorization();

        app.UseHealthChecks("/api/health", new HealthCheckOptions
        {
            ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
        });

        app.UseHangfire();

        return app;
    }

    private static void ConfigureResourcesFolder(this IApplicationBuilder app)
    {
        var resourcePath = Path.Combine(Directory.GetCurrentDirectory(), "Resources");
        Directory.CreateDirectory(resourcePath);

        app.UseStaticFiles(new StaticFileOptions
        {
            FileProvider = new PhysicalFileProvider(resourcePath),
            RequestPath = "/Resources",
            //OnPrepareResponse = ctx =>
            //{
            //    var user = ctx.Context.User;
            //    if (user?.Identity?.IsAuthenticated != true)
            //    {
            //        ctx.Context.Response.StatusCode = StatusCodes.Status401Unauthorized;
            //        ctx.Context.Response.ContentLength = 0;
            //        ctx.Context.Response.Body = Stream.Null;
            //    }
            //}
        });
    }

}

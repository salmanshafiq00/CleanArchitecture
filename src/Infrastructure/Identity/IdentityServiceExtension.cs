using Application.Common.Abstractions.Identity;
using Domain.Constants;
using Infrastructure.Identity.Entities;
using Infrastructure.Identity.OptionsSetup;
using Infrastructure.Identity.Permissions;
using Infrastructure.Identity.Services;
using Infrastructure.Persistence;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Infrastructure.Identity;

internal static class IdentityServiceExtension
{
    public static IServiceCollection AddIdentityService(this IServiceCollection services, IConfiguration configuration)
    {
        var identityConString = configuration.GetConnectionString("IdentityConnection");

        Guard.Against.Null(identityConString, message: $"Connection string 'IdentityConnection' not found.");

        services.AddDbContext<IdentityContext>(options => options.UseSqlServer(identityConString));

        services.AddIdentityCore<AppUser>()
        .AddRoles<IdentityRole>()
        .AddEntityFrameworkStores<IdentityContext>()
        .AddApiEndpoints();

        // Configure reset token lifespan here
        services.Configure<DataProtectionTokenProviderOptions>(options =>
        {
            options.TokenLifespan = TimeSpan.FromMinutes(2);
        });

        services.AddScoped<IdentityDbContextInitialiser>();
        services.AddScoped<IIdentityService, IdentityService>();
        services.AddTransient<ICustomAuthorizationService, CustomAuthorizationService>();
        services.AddTransient<IIdentityRoleService, IdentityRoleService>();
        services.AddTransient<IAuthService, AuthService>();
        services.AddTransient<ITokenProviderService, TokenProviderService>();

        services.AddAuthentication(options =>
        {
            options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
        })
        .AddJwtBearer();

        services.ConfigureOptions<JwtOptionsSetup>();
        services.ConfigureOptions<JwtBearerOptionsSetup>();

        services.AddAuthorizationBuilder()
            .AddPolicy(Policies.CanPurge, policy => policy.RequireRole(Roles.Administrator));

        services.AddScoped<IAuthorizationHandler, PermissionAuthorizationHandler>(); // Handles dynamic permission checks
        // For dynamically create policy if not exist
        services.AddSingleton<IAuthorizationPolicyProvider, PermissionPolicyProvider>(); // Dynamically provides policies

        return services;
    }
}

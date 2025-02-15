using System.Security.Claims;
using Domain.Constants;
using Infrastructure.Identity.Entities;
using Infrastructure.Identity.Permissions;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using PermissionConstant = Application.Common.Security.Permissions;
namespace Infrastructure.Identity;

public static class IdentityInitialiserExtensions
{
    public static async Task IdentityInitialiseDatabaseAsync(this WebApplication app)
    {
        using var scope = app.Services.CreateScope();

        var identityInitialiser = scope.ServiceProvider.GetRequiredService<IdentityDbContextInitialiser>();

        await identityInitialiser.InitialiseAsync();

        await identityInitialiser.SeedAsync();
    }
}

internal sealed class IdentityDbContextInitialiser(
        ILogger<IdentityDbContextInitialiser> logger,
        IdentityContext context,
        UserManager<AppUser> userManager,
        RoleManager<IdentityRole> roleManager)
{
    public async Task InitialiseAsync()
    {
        try
        {
            await context.Database.MigrateAsync();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An error occurred while initialising the database.");
            throw;
        }
    }

    public async Task SeedAsync()
    {
        try
        {
            await SeedDefaultIdentityAsync();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An error occurred while seeding the database.");
            throw;
        }
    }

    private async Task SeedDefaultIdentityAsync()
    {
        // Default roles
        var administratorRole = new IdentityRole(Roles.Administrator);

        if (roleManager.Roles.All(r => r.Name != administratorRole.Name))
        {
            await roleManager.CreateAsync(administratorRole);
        }

        // Get Permission
        var features = PermissionConstant.GetAllNestedModule(typeof(PermissionConstant.Admin));
        features.AddRange(PermissionConstant.GetAllNestedModule(typeof(PermissionConstant.CommonSetup)));

        var permissions = PermissionConstant.GetPermissionsByfeatures(features);

        // Default Permissions
        foreach (var permission in permissions)
        {
            await roleManager.AddClaimAsync(administratorRole, new Claim(CustomClaimTypes.Permission, permission));
        }

        // Default users
        var administrator = new AppUser { UserName = "salman@localhost", Email = "salman@localhost" };

        if (userManager.Users.All(u => u.UserName != administrator.UserName))
        {
            await userManager.CreateAsync(administrator, "Salman@123");
            if (!string.IsNullOrWhiteSpace(administratorRole.Name))
            {
                await userManager.AddToRolesAsync(administrator, [administratorRole.Name]);
            }
        }
    }
}

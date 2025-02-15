using Domain.Admin;
using Domain.Common;
using Domain.Constants;
using Infrastructure.Identity;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;

namespace Infrastructure.Persistence;


public static class ApplicationInitialiserExtensions
{
    public static async Task AppInitialiseDatabaseAsync(this WebApplication app)
    {
        using var scope = app.Services.CreateScope();

        var appInitialiser = scope.ServiceProvider.GetRequiredService<ApplicationDbContextInitialiser>();

        await appInitialiser.InitialiseAsync();

        await appInitialiser.SeedAsync();
    }
}

internal sealed class ApplicationDbContextInitialiser(
    ApplicationDbContext appDbContext,
    IdentityContext identityContext,
    ILogger<ApplicationDbContextInitialiser> logger)
{

    public async Task InitialiseAsync()
    {
        try
        {
            await appDbContext.Database.MigrateAsync();
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
            await SeedDefaultDataAsync();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An error occurred while seeding the database.");
            throw;
        }
    }

    private async Task SeedDefaultDataAsync()
    {
        using var transaction = await appDbContext.Database.BeginTransactionAsync();

        try
        {
            Lookup menuTypeLookup = new() { Name = "Menu Type", Code = "menu", DevCode = 101, Status = true };
            await appDbContext.Lookups.AddAsync(menuTypeLookup);
            await appDbContext.SaveChangesAsync();  // Save to get the ID

            List<LookupDetail> lookupDetails = [
                new() { Name = "Module", Code = "module", LookupId = menuTypeLookup.Id, DevCode = 10101, Status = true },
        new() { Name = "Sub Menu", Code = "smenu", LookupId = menuTypeLookup.Id, DevCode = 10102, Status = true },
        new() { Name = "Menu", Code = "menu", LookupId = menuTypeLookup.Id, DevCode = 10103, Status = true }
            ];

            await appDbContext.LookupDetails.AddRangeAsync(lookupDetails);
            await appDbContext.SaveChangesAsync();  // Save to get IDs

            var moduleId = lookupDetails.First(x => x.DevCode == 10101).Id;
            var menuId = lookupDetails.First(x => x.DevCode == 10103).Id;

            List<AppMenu> appModules = [
                new() { Label = "Admin", RouterLink = "/admin", ParentId = null, IsActive = true, OrderNo = 1, Visible = true, MenuTypeId = moduleId },
        new() { Label = "Common Setup", RouterLink = "/setup", ParentId = null, IsActive = true, OrderNo = 2, Visible = true, MenuTypeId = moduleId }
            ];

            await appDbContext.AddRangeAsync(appModules);
            await appDbContext.SaveChangesAsync();  // Save to get IDs

            var adminModuleId = appModules.First(x => x.Label == "Admin").Id;
            var commonSetupModuleId = appModules.First(x => x.Label == "Common Setup").Id;

            List<AppMenu> appmunes = [
                new() { Label = "Dashboard", RouterLink = "/", ParentId = null, IsActive = true, OrderNo = 0, Visible = true, MenuTypeId = menuId },
        new() { Label = "Users", RouterLink = "/admin/users", ParentId = adminModuleId, IsActive = true, OrderNo = 1, Visible = true, MenuTypeId = menuId },
        new() { Label = "Roles", RouterLink = "/admin/roles", ParentId = adminModuleId, IsActive = true, OrderNo = 2, Visible = true, MenuTypeId = menuId },
        new() { Label = "App Menu", RouterLink = "/admin/app-menus", ParentId = adminModuleId, IsActive = true, OrderNo = 3, Visible = true, MenuTypeId = menuId },
        new() { Label = "App Page", RouterLink = "/admin/app-pages", ParentId = adminModuleId, IsActive = true, OrderNo = 4, Visible = true, MenuTypeId = menuId },
        new() { Label = "Lookup", RouterLink = "/setup/lookups", ParentId = commonSetupModuleId, IsActive = true, OrderNo = 1, Visible = true, MenuTypeId = menuId },
        new() { Label = "Lookup Detail", RouterLink = "/setup/lookup-details", ParentId = commonSetupModuleId, IsActive = true, OrderNo = 2, Visible = true, MenuTypeId = menuId }
            ];

            await appDbContext.AddRangeAsync(appmunes);
            await appDbContext.SaveChangesAsync(); // Save everything

            var adminRole = await identityContext.Roles
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.Name == Roles.Administrator);

            appDbContext.RoleMenus.AddRange(appmunes.Select(x => new RoleMenu { RoleId = adminRole.Id, AppMenuId = x.Id }));
            await appDbContext.SaveChangesAsync();

            // Commit transaction if everything is successful
            await transaction.CommitAsync();
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }

    }
}

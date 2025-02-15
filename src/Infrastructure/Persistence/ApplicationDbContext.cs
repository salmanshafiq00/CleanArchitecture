using System.Reflection;
using Application.Common.Abstractions;
using Domain.Admin;
using Domain.Common;
using Microsoft.EntityFrameworkCore.Storage;

namespace Infrastructure.Persistence;

public sealed class ApplicationDbContext : DbContext, IApplicationDbContext
{
    private IDbContextTransaction? _currentTransaction;
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

    #region Admin

    public DbSet<AppMenu> AppMenus => Set<AppMenu>();
    public DbSet<RoleMenu> RoleMenus => Set<RoleMenu>();
    public DbSet<AppNotification> AppNotifications => Set<AppNotification>();
    #endregion

    #region Common
    public DbSet<Lookup> Lookups => Set<Lookup>();

    public DbSet<LookupDetail> LookupDetails => Set<LookupDetail>();

    #endregion

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());

        // Apply configurations specific to ApplicationDbContext
        //builder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly, x => x.Namespace!.EndsWith("Persistence.Configurations"));
    }

    public async Task BeginTransactionAsync()
    {
        if (_currentTransaction != null)
            throw new InvalidOperationException("A transaction is already in progress.");

        _currentTransaction = await Database.BeginTransactionAsync();
    }

    public async Task CommitTransactionAsync()
    {
        if (_currentTransaction == null)
            throw new InvalidOperationException("No transaction in progress.");

        try
        {
            _currentTransaction?.Commit();
        }
        finally
        {
            await DisposeTransactionAsync();
        }
    }

    private async Task DisposeTransactionAsync()
    {
        if (_currentTransaction != null)
        {
            await _currentTransaction.DisposeAsync();
            _currentTransaction = null;
        }
    }
    public async Task RollbackTransactionAsync()
    {
        try
        {
            _currentTransaction?.Rollback();
            // Log rollback
            Console.WriteLine("Transaction rolled back.");
        }
        finally
        {
            await DisposeTransactionAsync();
        }
    }


}

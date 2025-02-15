using Dapper;
using Application.Common.Abstractions;
using Application.Common.DapperQueries;
using Infrastructure.Persistence.TypeHandlers;
using Infrastructure.Persistence.Interceptors;
using Infrastructure.Persistence.Outbox;
using Infrastructure.Persistence.Services;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Infrastructure.Persistence;

internal static class PersistenceServiceExtension
{
    private const string DefaultConnection = nameof(DefaultConnection);

    public static void AddPersistenceServices(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString(DefaultConnection);
        Guard.Against.Null(connectionString, message: $"Connection string '{nameof(DefaultConnection)}' not found.");


        services.AddScoped<ISaveChangesInterceptor, AuditableEntityInterceptor>();
        services.AddScoped<ISaveChangesInterceptor, InsertOutboxMessagesInterceptor>();
        services.AddScoped<ISqlConnectionFactory>(_ => new SqlConnectionFactory(connectionString));
        services.AddDbContext<ApplicationDbContext>((sp, options) =>
        {
            options.AddInterceptors(sp.GetServices<ISaveChangesInterceptor>());
            options.UseSqlServer(connectionString);
        });
        services.AddScoped<IApplicationDbContext>(provider
            => provider.GetRequiredService<ApplicationDbContext>());
        services.AddScoped<ApplicationDbContextInitialiser>();

        AddDapperTypeHandlers();
        services.AddScoped<ICommonQueryService, CommonQueryService>();

    }

    public static void AddDapperTypeHandlers()
    {
        SqlMapper.AddTypeHandler(new DapperSqlDateOnlyTypeHandler());
        SqlMapper.AddTypeHandler(new DapperSqlNullableDateOnlyTypeHandler());
        SqlMapper.AddTypeHandler(new DapperSqlTimeOnlyTypeHandler());
        SqlMapper.AddTypeHandler(new DapperSqlNullableTimeOnlyTypeHandler());
    }

}

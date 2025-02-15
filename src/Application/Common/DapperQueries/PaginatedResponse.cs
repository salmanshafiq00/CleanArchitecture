using System.Data;
using Application.Common.Abstractions;
using Application.Common.Exceptions;
using Microsoft.Extensions.DependencyInjection;

namespace Application.Common.DapperQueries;

public class PaginatedResponse<TEntity>
    where TEntity : class
{
    [JsonInclude]
    public IReadOnlyCollection<TEntity> Items { get; init; } = [];
    public int Offset { get; init; }
    public int Next { get; init; }
    public int TotalCount { get; init; }
    public bool HasPreviousPage => Offset > 0;
    public bool HasNextPage => Offset + Next < TotalCount;
    public int CurrentPosition => Math.Min(Offset + Next, TotalCount);

    public PaginatedResponse() { }

    public PaginatedResponse(IReadOnlyCollection<TEntity> items, int count, int offset, int next)
    {
        Items = items;
        TotalCount = count;
        Offset = offset;
        Next = next;
    }

    public static async Task<PaginatedResponse<TEntity>> CreateAsync(
        IDbConnection connection,
        string sql,
        int offset,
        int next,
        object? parameters = default,
        string? orderBy = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(connection);
        ArgumentException.ThrowIfNullOrWhiteSpace(sql);

        // Use SQL parameters to prevent SQL injection
        var param = new DynamicParameters(parameters);
        param.Add("@Offset", offset);
        param.Add("@Next", next);

        // Optimize the count query
        var countSql = $"""
            WITH ItemCount AS (
                SELECT COUNT(*) AS Total 
                FROM ({sql}) AS CountQuery
            )
            SELECT Total FROM ItemCount
            """;

        // Execute queries in parallel for better performance
        var logger = ServiceLocator.ServiceProvider
            .GetRequiredService<ILogger<PaginatedResponse<TEntity>>>();

        try
        {
            logger.LogDebug("Executing paginated query with parameters: Offset {Offset}, Next {Next}",
                offset, next);

            var finalSql = $"""
                {sql}
                ORDER BY {orderBy ?? "(SELECT NULL)"}
                OFFSET @Offset ROWS 
                FETCH NEXT @Next ROWS ONLY
                """;

            // Create and start both tasks
            var itemsTask = connection.QueryAsync<TEntity>(finalSql, param);
            var countTask = connection.ExecuteScalarAsync<int>(countSql, param);

            // Wait for both tasks to complete
            await Task.WhenAll(itemsTask, countTask);

            // Get results
            var items = await itemsTask;
            var count = await countTask;

            return new PaginatedResponse<TEntity>(
                items?.AsList() ?? [],
                count,
                offset,
                next);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error executing paginated query");
            throw new DapperQueryException("Failed to execute paginated query", ex);
        }
    }
}

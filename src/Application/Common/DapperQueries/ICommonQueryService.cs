using Domain.Common;

namespace Application.Common.DapperQueries;

public interface ICommonQueryService
{
    /// <summary>
    /// Checks whether a record exists in the specified database table based on the given filter criteria.
    /// </summary>
    /// <param name="tableName">The name of the database table to query.</param>
    /// <param name="equalFilters">
    /// An array of column names for which equality checks will be applied.
    /// The provided <paramref name="param"/> object should contain values matching these columns.
    /// </param>
    /// <param name="param">
    /// An optional object containing the values to be matched against the columns in <paramref name="equalFilters"/>.
    /// Properties of this object must match the column names.
    /// </param>
    /// <param name="notEqualFilters">
    /// An optional array of column names for which inequality checks will be applied.
    /// These columns will be checked to ensure their values in the database do not match those in <paramref name="param"/>.
    /// </param>
    /// <returns>
    /// A task that resolves to a <see cref="bool"/> indicating whether any record satisfies the specified criteria.
    /// Returns <c>true</c> if a matching record exists, otherwise <c>false</c>.
    /// </returns>
    /// <remarks>
    /// This method is typically used to ensure the uniqueness of specific column values, such as enforcing 
    /// unique constraints on names, codes, or other identifiers within a database table.
    /// </remarks>
    Task<bool> IsExistAsync(string tableName, string[] equalFilters, object? param = null, string[]? notEqualFilters = null);

    Task<Guid?> GetLookupDetailIdAsync(int lookupDetailDevCode, CancellationToken cancellationToken = default);
    Task<List<LookupDetail>> GetLookupDetailsAsync(int lookupDevCode, CancellationToken cancellationToken = default);
}

using System.Data;
using System.Text;
using Microsoft.Extensions.DependencyInjection;
using static EasyPOS.Application.Common.DapperQueries.Constants;
using static EasyPOS.Application.Common.DapperQueries.SqlConstants;

namespace EasyPOS.Application.Common.DapperQueries;

public class PaginatedResponse_140924<TEntity>
    where TEntity : class
{
    [JsonInclude]
    public IReadOnlyCollection<TEntity> Items { get; init; }
    public int PageNumber { get; init; }
    public int TotalPages { get; init; }
    public int TotalCount { get; init; }
    public bool HasPreviousPage => PageNumber > 1;
    public bool HasNextPage => PageNumber < TotalPages;

    [JsonInclude]
    public Dictionary<string, object> OptionsDataSources { get; set; } = [];


    public PaginatedResponse_140924() { }

    public PaginatedResponse_140924(
        IReadOnlyCollection<TEntity> items,
        int count,
        int pageNumber,
        int pageSize)
    {
        PageNumber = pageNumber;
        TotalPages = (int)Math.Ceiling(count / (double)pageSize);
        TotalCount = count;
        Items = items;
    }

    public static async Task<PaginatedResponse_140924<TEntity>> CreateAsync(
         IDbConnection connection,
         string sql,
         DataGridModel gridModel,
         object? parameters = default,
         string? orderBy = null,
         string? groupBy = null,
         string? having = null)
    {
        var logger = ServiceLocator.ServiceProvider
            .GetRequiredService<ILogger<PaginatedResponse_140924<TEntity>>>();

        #region Filter (WHERE)

        SetWhereCluaseIfNotExist(ref sql);

        SetGlobalFilterSql(gridModel, ref sql);

        SetFilterSql(ref sql, gridModel);

        #endregion

        #region Group By

        if (groupBy is not null)
        {
            sql = $"""
            {sql}
            {groupBy} 
            """;
        }

        #endregion

        #region Sorting (ORDER BY)

        string defaultOrderBy = orderBy ?? $"{S.ORDERBY} (SELECT NULL)";

        string sqlOrderBy = GetOrderBySql(gridModel);

        if (string.IsNullOrEmpty(sqlOrderBy))
        {
            sqlOrderBy = defaultOrderBy;
        }

        #endregion

        #region Pagination  (FETCH NEXT)

        var paginatedSql = $"""
            {sql}
            {sqlOrderBy} 
            OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY;
            """;


        #endregion

        #region Parameters

        var param = new DynamicParameters(parameters);
        param.Add(nameof(gridModel.Offset), gridModel.Offset);
        param.Add(nameof(gridModel.PageSize), gridModel.PageSize);

        if (!string.IsNullOrEmpty(gridModel.GlobalFilterValue))
        {
            param.Add(nameof(gridModel.GlobalFilterValue), $"%{gridModel.GlobalFilterValue.ToLower()}%");
        }

        #endregion

        logger?.LogInformation("Executing SQL: {Sql}", paginatedSql);

        var items = await connection
            .QueryAsync<TEntity>(paginatedSql, param);

        var count = await connection
            .ExecuteScalarAsync<int>($"{S.SELECT} {S.COUNT}(*) {S.FROM} ({sql}) as CountQuery", param);

        //SetFiltersToGridModel(gridModel, dataFields);

        return new PaginatedResponse_140924<TEntity>(
            items.AsList(),
            count,
            gridModel.Offset / gridModel.PageSize + 1,
            gridModel.PageSize
            );
    }


    // Functions -----------

    #region Filtering Functions (WHERE)

    private static bool HasWhereClause(string sql)
    {
        return sql.IndexOf($"{S.WHERE}", StringComparison.OrdinalIgnoreCase) >= 0;
    }

    private static void SetWhereCluaseIfNotExist(ref string sql)
    {
        if (HasWhereClause(sql))
        {
            return;
        }
        string defaultWhereSql = $"{S.WHERE} 1 = 1";

        sql = $"""
            {sql} 
            {defaultWhereSql} 
            """;
    }

    private static void SetGlobalFilterSql(
        DataGridModel gridModel,
        ref string sql)
    {

        if (string.IsNullOrEmpty(gridModel.GlobalFilterValue)
               || string.IsNullOrWhiteSpace(gridModel.GlobalFilterValue))
        {
            return;
        }

        StringBuilder globalFilter = new();

        bool isFirst = true;
        foreach (var field in gridModel.GlobalFilterFields)
        {
            if (isFirst)
            {
                globalFilter.Append('(');
                isFirst = false;
            }
            else
            {
                globalFilter.Append(" OR ");
            }

            if (field.FieldType == TFieldType.TDate)
            {
                globalFilter.Append($"{S.CONV}(NVARCHAR(10), {field.DbField}, 103) {S.LIKE} @GlobalFilterValue");
            }
            else
            {
                globalFilter.Append($"{S.LOWER}({field.DbField}) {S.LIKE} @GlobalFilterValue");
            }
        }

        if (!isFirst) globalFilter.Append(')');

        sql = $"""
            {sql} 
            {S.AND} {globalFilter}
            """;
    }

    private static void SetFilterSql(
        ref string sql,
        DataGridModel gridModel)
    {
        if (gridModel.Filters.Count == 0
            || !gridModel.Filters.Any(x => string.IsNullOrEmpty(x.Value)))
        {
            return;
        }

        StringBuilder filterBuilder = new();

        foreach (var filter in gridModel.Filters.Where(x => !string.IsNullOrEmpty(x.Value)))
        {
            string sqlOperator = string.IsNullOrEmpty(filter.Operator) ? $" {S.AND}" : filter.Operator.ToUpper();

            //var dataField = GetDataField(dataFields, filter.FieldName);

            //if (dataField is null) continue;

            string condition = string.Empty;

            if (filter?.FieldType == TFieldType.TString)
            {
                condition = filter.MatchMode switch
                {
                    MatchMode.STARTS_WITH => $"{S.LOWER}({filter.DbField}) {S.LIKE} {S.LOWER}('{filter.Value}%')",
                    MatchMode.ENDS_WITH => $"{S.LOWER}({filter.DbField}) {S.LIKE} {S.LOWER}('%{filter.Value}')",
                    MatchMode.CONTAINS => $"{S.LOWER}({filter.DbField}) {S.LIKE} {S.LOWER}('%{filter.Value}%')",
                    MatchMode.NOT_CONTAINS => $"{S.LOWER}({filter.DbField}) {S.LIKE} {S.LOWER}('%{filter.Value}%')",
                    MatchMode.EQUALS => $"{S.LOWER}({filter.DbField}) = {S.LOWER}('{filter.Value}')",
                    MatchMode.NOT_EQUALS => $"{S.LOWER}({filter.DbField}) != {S.LOWER}('{filter.Value}')",
                    _ => throw new InvalidOperationException($"Unknown MatchMode: {filter.MatchMode}")
                };
            }
            else if (filter?.FieldType == TFieldType.TSelect)
            {
                condition = filter.MatchMode switch
                {
                    MatchMode.EQUALS => $"{filter.DbField} = {filter.Value.ToUpper()}",
                    MatchMode.NOT_EQUALS => $"{filter.DbField} != {filter.Value.ToUpper()}",
                    _ => throw new InvalidOperationException($"Unknown MatchMode: {filter.MatchMode}")
                };

            }
            else if (filter?.FieldType == TFieldType.TMultiSelect)
            {
                condition = filter.MatchMode switch
                {
                    MatchMode.IN => $"{filter.DbField} {S.IN} ({filter.Value.ToUpper()})",
                    MatchMode.NOTIN => $"{filter.DbField} {S.NOT} {S.IN} ({filter.Value.ToUpper()})",
                    _ => throw new InvalidOperationException($"Unknown MatchMode: {filter.MatchMode}")
                };

            }
            else if (filter?.FieldType == TFieldType.TDate)
            {
                condition = filter.MatchMode switch
                {
                    MatchMode.DATE_IS => $"{S.CONV}(DATE, {filter.DbField}) = '{filter.Value}'",
                    MatchMode.DATE_IS_NOT => $"{S.CONV}(DATE, {filter.DbField}) <> '{filter.Value}'",
                    MatchMode.DATE_BEFORE => $"{S.CONV}(DATE, {filter.DbField}) < '{filter.Value}'",
                    MatchMode.DATE_AFTER => $"{S.CONV}(DATE, {filter.DbField}) > '{filter.Value}'",
                    MatchMode.DATE_IS_OR_BEFORE => $"{S.CONV}(DATE, {filter.DbField}) <= '{filter.Value}'",
                    MatchMode.DATE_IS_OR_AFTER => $"{S.CONV}(DATE, {filter.DbField}) >= '{filter.Value}'",
                    _ => throw new InvalidOperationException($"Unknown MatchMode: {filter.MatchMode}")
                };
            }

            if (!string.IsNullOrEmpty(condition))
            {
                filterBuilder.Append(sqlOperator);
                filterBuilder.Append(" ");
                filterBuilder.Append(condition);
            }
        }

        sql = filterBuilder.Length > 0
            ? $"""
                {sql} 
                {filterBuilder.ToString()}
             """
            : sql;
    }
    #endregion

    #region Sorting Functions (ORDER BY)

    private static string GetOrderBySql(DataGridModel gridModel)
    {
        if (string.IsNullOrEmpty(gridModel.SortField) || gridModel.SortField is null)
        {
            return string.Empty;
        }

        return gridModel.SortOrder == -1
            ? $"ORDER BY {gridModel.SortField} DESC"
            : $"ORDER BY {gridModel.SortField} ASC";
    }

    private static bool HasOrderByClause(string sql)
    {
        return sql.Contains($"{S.ORDERBY}", StringComparison.OrdinalIgnoreCase);
    }


    #endregion



}

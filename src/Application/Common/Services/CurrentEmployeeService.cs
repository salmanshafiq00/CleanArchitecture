using Application.Common.Abstractions;
using Application.Common.Abstractions.Identity;
using Application.Common.Models;

namespace Application.Common.Services;

public sealed class CurrentEmployeeService(
    IUser user, ISqlConnectionFactory sqlConnectionFactory) : ICurrentEmployee
{
    private readonly IUser _user = user;
    private readonly ISqlConnectionFactory _sqlConnectionFactory = sqlConnectionFactory;

    public Task<CurrentEmployee?> GetCurrentEmployeeAsync()
    {
        throw new NotImplementedException();
    }

    public async Task<Guid?> GetCurrentEmployeeIdAsync()
    {
        var userId = _user.Id;

        if (string.IsNullOrEmpty(userId))
        {
            return default; // No user is logged in
        }

        var connection = _sqlConnectionFactory.GetOpenConnection();

        var sql = $"""
            SELECT 
                e.Id
            FROM Employees AS e
            WHERE e.UserId = @UserId
        """;

        return await connection.QueryFirstOrDefaultAsync<Guid?>(sql, new { UserId = userId });
    }

    public async Task<CurrentEmployee> GetEmployeeIdAsync()
    {
        var userId = _user.Id;

        if (string.IsNullOrEmpty(userId))
        {
            return default; // No user is logged in
        }

        var connection = _sqlConnectionFactory.GetOpenConnection();

        var sql = $"""
            SELECT 
                e.Id, 
                e.FirstName, 
                e.LastName, 
                e.Code, 
                e.PhotoUrl, 
                e.UserId
            FROM Employees AS e
            WHERE e.UserId = @UserId
        """;

        return await connection.QueryFirstOrDefaultAsync<CurrentEmployee>(sql, new { UserId = userId });
    }
}

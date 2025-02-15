using System.Security.Claims;
using Dapper;
using Application.Common.Abstractions;
using Application.Common.Abstractions.Identity;

namespace WebApi.Services;

public class CurrentUser(IHttpContextAccessor httpContextAccessor, ISqlConnectionFactory sqlConnectionFactory) : IUser
{
    private readonly IHttpContextAccessor _httpContextAccessor = httpContextAccessor;
    private readonly ISqlConnectionFactory _sqlConnectionFactory = sqlConnectionFactory;

    public string? Id => _httpContextAccessor.HttpContext?.User?.FindFirstValue(ClaimTypes.NameIdentifier);

    public async Task<Guid?> GetCurrentEmployeeIdAsync()
    {
        var userId = Id;

        if (string.IsNullOrEmpty(userId))
        {
            return null; // No user is logged in
        }

        var connection = _sqlConnectionFactory.GetOpenConnection();

        // Query to fetch the EmployeeId for the current user
        var sql = $"""
            SELECT e.Id 
            FROM Employees AS e
            WHERE e.UserId = @UserId
        """;

        // Use Dapper to execute the query
        return await connection.QueryFirstOrDefaultAsync<Guid?>(sql, new { UserId = userId });
    }
}

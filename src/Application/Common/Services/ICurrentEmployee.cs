using Application.Common.Models;

namespace Application.Common.Services;

public interface ICurrentEmployee
{
    /// <summary>
    /// Retrieves the current employee associated with the logged-in user.
    /// Returns null if no employee is found.
    /// </summary>
    Task<CurrentEmployee?> GetCurrentEmployeeAsync();

    /// <summary>
    /// Retrieves the ID of the current employee associated with the logged-in user.
    /// Returns null if no employee is found.
    /// </summary>
    Task<Guid?> GetCurrentEmployeeIdAsync();
}

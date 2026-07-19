namespace Maliev.ComplianceService.Application.Interfaces;

/// <summary>
/// Service for retrieving employee information from the Employee Service.
/// </summary>
public interface IEmployeeService
{
    /// <summary>
    /// Retrieves the full name of an employee.
    /// </summary>
    /// <param name="employeeId">The unique identifier of the employee.</param>
    /// <param name="cancellationToken">Optional cancellation token.</param>
    /// <returns>The employee's full name, or null if not found.</returns>
    Task<string> GetEmployeeNameAsync(Guid employeeId, CancellationToken cancellationToken = default);
}

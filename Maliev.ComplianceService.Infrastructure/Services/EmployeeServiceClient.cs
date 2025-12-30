using Maliev.ComplianceService.Application.Interfaces;
using Microsoft.Extensions.Logging;

namespace Maliev.ComplianceService.Infrastructure.Services;

/// <summary>
/// Client for interacting with the Employee Service.
/// </summary>
public class EmployeeServiceClient : IEmployeeService
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<EmployeeServiceClient> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="EmployeeServiceClient"/> class.
    /// </summary>
    /// <param name="httpClient">The HTTP client.</param>
    /// <param name="logger">The logger.</param>
    public EmployeeServiceClient(HttpClient httpClient, ILogger<EmployeeServiceClient> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<string> GetEmployeeNameAsync(Guid employeeId, CancellationToken cancellationToken = default)
    {
        try
        {
            // In a real implementation, we would call the Employee Service
            // var response = await _httpClient.GetAsync($"/api/employees/{employeeId}", cancellationToken);
            // ...
            
            // For now, return a placeholder as per FR-031 if it fails or if we want to stub it
            return "(name unavailable)";
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to retrieve employee name for {EmployeeId}", employeeId);
            return "(name unavailable)";
        }
    }
}

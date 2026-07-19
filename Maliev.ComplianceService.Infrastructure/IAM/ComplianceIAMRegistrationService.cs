using Maliev.Aspire.ServiceDefaults.IAM;
using Maliev.ComplianceService.Domain.Authorization;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Maliev.ComplianceService.Infrastructure.IAM;

/// <summary>
/// Background service that registers compliance-related permissions with the IAM service on startup.
/// </summary>
public class ComplianceIAMRegistrationService : IAMRegistrationService
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ComplianceIAMRegistrationService"/> class.
    /// </summary>
    /// <param name="configuration">The application configuration.</param>
    /// <param name="logger">Logger instance.</param>
    public ComplianceIAMRegistrationService(
        IConfiguration configuration,
        ILogger<ComplianceIAMRegistrationService> logger)
        : base(configuration, logger, "compliance")
    {
    }

    /// <inheritdoc/>
    protected override IEnumerable<PermissionRegistration> GetPermissions()
    {
        return CompliancePermissions.All.Select(p => new PermissionRegistration
        {
            PermissionId = p.Key,
            Description = p.Value
        });
    }

    /// <inheritdoc/>
    protected override IEnumerable<RoleRegistration> GetPredefinedRoles()
    {
        return Enumerable.Empty<RoleRegistration>();
    }
}


using Maliev.Aspire.ServiceDefaults.IAM;
using Maliev.ComplianceService.Domain.Authorization;
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
    public ComplianceIAMRegistrationService(IHttpClientFactory httpClientFactory, ILogger<ComplianceIAMRegistrationService> logger)
        : base(httpClientFactory, logger, "ComplianceService")
    {
    }

    /// <inheritdoc/>
    protected override IEnumerable<PermissionRegistration> GetPermissions()
    {
        return new[]
        {
            new PermissionRegistration { PermissionId = CompliancePermissions.Manage, Description = "Manage work authorizations and compliance documents" },
            new PermissionRegistration { PermissionId = CompliancePermissions.Reports, Description = "View compliance reports and audits" }
        };
    }

    /// <inheritdoc/>
    protected override IEnumerable<RoleRegistration> GetPredefinedRoles()
    {
        return Enumerable.Empty<RoleRegistration>();
    }
}
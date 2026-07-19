namespace Maliev.ComplianceService.Domain.Enums;

/// <summary>
/// Provides extension methods for the ComplianceStatus enum.
/// </summary>
public static class ComplianceStatusExtensions
{
    /// <summary>
    /// Calculates the compliance status based on an optional expiration date.
    /// </summary>
    /// <param name="expirationDate">The expiration date of the work authorization.</param>
    /// <returns>The calculated compliance status.</returns>
    public static ComplianceStatus CalculateStatus(DateTime? expirationDate)
    {
        if (!expirationDate.HasValue)
            return ComplianceStatus.Compliant;

        var daysUntilExpiration = (expirationDate.Value - DateTime.UtcNow).Days;

        if (daysUntilExpiration < 0)
            return ComplianceStatus.Expired;
        if (daysUntilExpiration <= 90)
            return ComplianceStatus.ExpiringSoon;

        return ComplianceStatus.Compliant;
    }
}

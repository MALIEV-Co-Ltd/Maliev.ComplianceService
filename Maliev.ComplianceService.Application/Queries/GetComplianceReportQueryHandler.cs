using Maliev.ComplianceService.Application.DTOs;
using Maliev.ComplianceService.Application.Interfaces;
using Maliev.ComplianceService.Application.Mappers;
using Maliev.ComplianceService.Domain.Enums;
using MediatR;
using Microsoft.Extensions.Caching.Distributed;
using System.Text.Json;

namespace Maliev.ComplianceService.Application.Queries.GetComplianceReport;

/// <summary>
/// Handles the GetComplianceReportQuery.
/// </summary>
public class GetComplianceReportQueryHandler : IRequestHandler<GetComplianceReportQuery, ComplianceReportResponse>
{
    private readonly IWorkAuthorizationRepository _repository;
    private readonly IComplianceAlertRepository _alertRepository;
    private readonly IDistributedCache _cache;

    /// <summary>
    /// Initializes a new instance of the <see cref="GetComplianceReportQueryHandler"/> class.
    /// </summary>
    /// <param name="repository">The work authorization repository.</param>
    /// <param name="alertRepository">The compliance alert repository.</param>
    /// <param name="cache">The distributed cache.</param>
    public GetComplianceReportQueryHandler(
        IWorkAuthorizationRepository repository,
        IComplianceAlertRepository alertRepository,
        IDistributedCache cache)
    {
        _repository = repository;
        _alertRepository = alertRepository;
        _cache = cache;
    }

    /// <summary>
    /// Handles the query to generate a compliance report.
    /// </summary>
    /// <param name="query">The query.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A compliance report response.</returns>
    public async Task<ComplianceReportResponse> Handle(GetComplianceReportQuery query, CancellationToken cancellationToken)
    {
        var cacheKey = $"compliance_report_{query.DepartmentId ?? Guid.Empty}";

        // Try to get from cache (simplified for now, usually we would check if cache is available)
        if (_cache != null)
        {
            var cached = await _cache.GetStringAsync(cacheKey, cancellationToken);
            if (!string.IsNullOrEmpty(cached))
            {
                return JsonSerializer.Deserialize<ComplianceReportResponse>(cached)!;
            }
        }

        // Optimized aggregation using the new repository methods
        var stats = await _repository.GetComplianceStatsAsync(query.DepartmentId, cancellationToken);
        var typeBreakdown = await _repository.GetTypeBreakdownAsync(query.DepartmentId, cancellationToken);

        var activeAlerts = await _alertRepository.GetAlertsAsync(isResolved: false, cancellationToken: cancellationToken);

        int compliantCount = stats.TryGetValue(ComplianceStatus.Compliant, out var c) ? c : 0;
        int expiringSoonCount = stats.TryGetValue(ComplianceStatus.ExpiringSoon, out var es) ? es : 0;
        int expiredCount = stats.TryGetValue(ComplianceStatus.Expired, out var e) ? e : 0;
        int pendingCount = stats.TryGetValue(ComplianceStatus.PendingVerification, out var p) ? p : 0;
        int nonCompliantCount = stats.TryGetValue(ComplianceStatus.NonCompliant, out var nc) ? nc : 0;

        int totalTracked = compliantCount + expiringSoonCount + expiredCount + pendingCount + nonCompliantCount;

        var report = new ComplianceReportResponse
        {
            ReportDate = DateTime.UtcNow,
            TotalEmployees = 0, // This would normally come from Employee Service
            RequiresAuthorization = totalTracked,
            Compliant = compliantCount,
            ExpiringSoon = expiringSoonCount,
            Expired = expiredCount,
            ComplianceRate = totalTracked > 0
                ? Math.Round((decimal)compliantCount / totalTracked * 100, 2)
                : 100,
            ByAuthorizationType = typeBreakdown.ToList(),
            Alerts = activeAlerts.Take(10).Select(a => DtoMapper.ToAlertDto(a)).ToList()
        };

        // Cache the result for 15 minutes
        if (_cache != null)
        {
            var options = new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(15)
            };
            await _cache.SetStringAsync(cacheKey, JsonSerializer.Serialize(report), options, cancellationToken);
        }

        return report;
    }
}

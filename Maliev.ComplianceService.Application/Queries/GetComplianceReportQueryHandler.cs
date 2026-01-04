using System.Text.Json;
using Maliev.ComplianceService.Application.DTOs;
using Maliev.ComplianceService.Application.Interfaces;
using Maliev.ComplianceService.Application.Mappers;
using Maliev.ComplianceService.Domain.Enums;
using MediatR;
using Microsoft.Extensions.Caching.Distributed;

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

        // In a real implementation, we would use more efficient aggregation queries
        // For now, let's use the repository methods we have
        
        var compliant = await _repository.GetByComplianceStatusAsync(ComplianceStatus.Compliant, cancellationToken);
        var expiring = await _repository.GetByComplianceStatusAsync(ComplianceStatus.ExpiringSoon, cancellationToken);
        var expired = await _repository.GetByComplianceStatusAsync(ComplianceStatus.Expired, cancellationToken);
        
        var activeAlerts = await _alertRepository.GetAlertsAsync(isResolved: false, cancellationToken: cancellationToken);

        var report = new ComplianceReportResponse
        {
            ReportDate = DateTime.UtcNow,
            TotalEmployees = 0, // In a real implementation, we would get this from Employee Service
            RequiresAuthorization = compliant.Count() + expiring.Count() + expired.Count(),
            Compliant = compliant.Count(),
            ExpiringSoon = expiring.Count(),
            Expired = expired.Count(),
            ComplianceRate = 0, // Calculate rate
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

using Maliev.ComplianceService.Application.DTOs;
using Maliev.ComplianceService.Domain.Entities;

namespace Maliev.ComplianceService.Application.Mappers;

/// <summary>
/// Provides mapping methods between domain entities and data transfer objects.
/// </summary>
public static class DtoMapper
{
    /// <summary>
    /// Maps a WorkAuthorization entity to a WorkAuthorizationResponse DTO.
    /// </summary>
    /// <param name="entity">The work authorization entity.</param>
    /// <param name="employeeName">The name of the employee.</param>
    /// <returns>A mapped work authorization response.</returns>
    public static WorkAuthorizationResponse ToDto(WorkAuthorization entity, string employeeName = "(name unavailable)")
    {
        return new WorkAuthorizationResponse
        {
            Id = entity.Id,
            EmployeeId = entity.EmployeeId,
            EmployeeName = employeeName,
            AuthorizationType = entity.AuthorizationType,
            DocumentNumber = entity.DocumentNumber,
            IssueDate = entity.IssueDate,
            ExpirationDate = entity.ExpirationDate,
            DaysUntilExpiration = entity.ExpirationDate.HasValue
                ? (entity.ExpirationDate.Value - DateTime.UtcNow).Days
                : null,
            IssuingAuthority = entity.IssuingAuthority,
            SponsorshipStatus = entity.SponsorshipStatus,
            ComplianceStatus = entity.ComplianceStatus,
            CreatedDate = entity.CreatedDate,
            ModifiedDate = entity.ModifiedDate,
            RowVersion = entity.RowVersion
        };
    }

    /// <summary>
    /// Maps a WorkAuthorization entity to an ExpiringAuthorizationResponse DTO.
    /// </summary>
    /// <param name="entity">The work authorization entity.</param>
    /// <param name="employeeName">The name of the employee.</param>
    /// <returns>A mapped expiring authorization response.</returns>
    public static ExpiringAuthorizationResponse ToExpiringDto(WorkAuthorization entity, string employeeName = "(name unavailable)")
    {
        return new ExpiringAuthorizationResponse
        {
            Id = entity.Id,
            EmployeeId = entity.EmployeeId,
            EmployeeName = employeeName,
            AuthorizationType = entity.AuthorizationType,
            DocumentNumber = entity.DocumentNumber,
            IssueDate = entity.IssueDate,
            ExpirationDate = entity.ExpirationDate,
            DaysUntilExpiration = entity.ExpirationDate.HasValue
                ? (entity.ExpirationDate.Value - DateTime.UtcNow).Days
                : null,
            IssuingAuthority = entity.IssuingAuthority,
            SponsorshipStatus = entity.SponsorshipStatus,
            ComplianceStatus = entity.ComplianceStatus,
            CreatedDate = entity.CreatedDate,
            ModifiedDate = entity.ModifiedDate,
            RowVersion = entity.RowVersion
        };
    }

    /// <summary>
    /// Maps a ComplianceAlert entity to a ComplianceAlertResponse DTO.
    /// </summary>
    /// <param name="entity">The compliance alert entity.</param>
    /// <param name="employeeName">The name of the employee.</param>
    /// <returns>A mapped compliance alert response.</returns>
    public static ComplianceAlertResponse ToAlertDto(ComplianceAlert entity, string employeeName = "(name unavailable)")
    {
        return new ComplianceAlertResponse
        {
            Id = entity.Id,
            EmployeeId = entity.EmployeeId,
            EmployeeName = employeeName,
            AlertType = entity.AlertType,
            Severity = entity.Severity,
            Message = entity.Message,
            IsResolved = entity.IsResolved,
            ResolvedDate = entity.ResolvedDate,
            ResolvedByName = null,
            ResolutionNotes = entity.ResolutionNotes,
            CreatedDate = entity.CreatedDate
        };
    }
}

using System.Net;
using System.Net.Http.Json;
using Maliev.ComplianceService.Application.DTOs;
using Maliev.ComplianceService.Domain.Enums;
using Maliev.ComplianceService.Tests.Fixtures;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Maliev.ComplianceService.Tests.Integration.Controllers;

public class AlertsControllerTests : IClassFixture<ComplianceServiceTestFixture>
{
    private readonly ComplianceServiceTestFixture _fixture;
    private readonly HttpClient _client;

    public AlertsControllerTests(ComplianceServiceTestFixture fixture)
    {
        _fixture = fixture;
        _client = fixture.CreateClient();
    }

    [Fact]
    public async Task GetAlerts_ReturnsAlerts()
    {
        // Act
        var response = await _client.GetAsync("/alerts");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var result = await response.Content.ReadFromJsonSnakeCaseAsync<IEnumerable<ComplianceAlertResponse>>();
        Assert.NotNull(result);
    }

    [Fact]
    public async Task ResolveAlert_ReturnsNoContent()
    {
        // Arrange
        var employeeId = Guid.NewGuid();
        var recordRequest = new RecordWorkAuthorizationRequest
        {
            AuthorizationType = AuthorizationType.WorkVisa,
            DocumentNumber = "ALERT-FIX-001",
            IssueDate = DateTime.UtcNow.AddDays(-30),
            ExpirationDate = DateTime.UtcNow.AddDays(100),
            RightToWorkDocumentId = Guid.NewGuid()
        };
        var recordResponse = await _client.PostAsJsonSnakeCaseAsync($"/employees/{employeeId}/work-authorization", recordRequest);
        var auth = await recordResponse.Content.ReadFromJsonSnakeCaseAsync<WorkAuthorizationResponse>();

        using var scope = _fixture.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<Maliev.ComplianceService.Infrastructure.Data.ComplianceDbContext>();
        var alert = new Maliev.ComplianceService.Domain.Entities.ComplianceAlert
        {
            WorkAuthorizationId = auth!.Id,
            EmployeeId = employeeId,
            AlertType = AlertType.VerificationRequired,
            Severity = AlertSeverity.Medium,
            Message = "Test alert for resolution",
            IsResolved = false
        };
        context.ComplianceAlerts.Add(alert);
        await context.SaveChangesAsync();

        var request = new ResolveAlertRequest
        {
            ResolvedBy = Guid.NewGuid(),
            ResolutionNotes = "Alert resolved in integration test."
        };

        // Act
        var response = await _client.PutAsJsonSnakeCaseAsync($"/alerts/{alert.Id}/resolve", request);

        // Assert
        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);

        // Verify it's resolved in the list
        var getResponse = await _client.GetAsync($"/alerts?isResolved=true&employeeId={alert.EmployeeId}");
        var alerts = await getResponse.Content.ReadFromJsonSnakeCaseAsync<IEnumerable<ComplianceAlertResponse>>();
        Assert.Contains(alerts!, a => a.Id == alert.Id);
    }
}

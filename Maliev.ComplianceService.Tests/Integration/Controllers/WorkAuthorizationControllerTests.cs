using System.Net;
using System.Net.Http.Json;
using Maliev.ComplianceService.Application.DTOs;
using Maliev.ComplianceService.Domain.Enums;
using Maliev.ComplianceService.Tests.Fixtures;
using Xunit;

namespace Maliev.ComplianceService.Tests.Integration.Controllers;

public class WorkAuthorizationControllerTests : IClassFixture<ComplianceServiceTestFixture>
{
    private readonly ComplianceServiceTestFixture _fixture;
    private readonly HttpClient _client;

    public WorkAuthorizationControllerTests(ComplianceServiceTestFixture fixture)
    {
        _fixture = fixture;
        _client = fixture.CreateClient();
    }

    [Fact]
    public async Task Post_ValidRequest_ReturnsCreated()
    {
        // Arrange
        var employeeId = Guid.NewGuid();
        var request = new RecordWorkAuthorizationRequest
        {
            AuthorizationType = AuthorizationType.WorkVisa,
            DocumentNumber = "DOC-INT-001",
            IssueDate = DateTime.UtcNow.AddDays(-30),
            ExpirationDate = DateTime.UtcNow.AddDays(365),
            IssuingAuthority = "USCIS",
            SponsorshipStatus = SponsorshipStatus.Sponsored,
            RightToWorkDocumentId = Guid.NewGuid(),
            Notes = "Integration test"
        };

        // Act
        var response = await _client.PostAsJsonSnakeCaseAsync($"/employees/{employeeId}/work-authorization", request);

        // Assert
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var result = await response.Content.ReadFromJsonSnakeCaseAsync<WorkAuthorizationResponse>();
        Assert.NotNull(result);
        Assert.Equal(employeeId, result.EmployeeId);
        Assert.Equal(request.DocumentNumber, result.DocumentNumber);
        Assert.Equal(ComplianceStatus.Compliant, result.ComplianceStatus);
    }

    [Fact]
    public async Task GetExpiring_ReturnsExpiringAuthorizations()
    {
        // Arrange
        var employeeId = Guid.NewGuid();
        var request = new RecordWorkAuthorizationRequest
        {
            AuthorizationType = AuthorizationType.WorkVisa,
            DocumentNumber = "EXP-001",
            IssueDate = DateTime.UtcNow.AddDays(-30),
            ExpirationDate = DateTime.UtcNow.AddDays(10), // Expiring soon
            RightToWorkDocumentId = Guid.NewGuid()
        };
        await _client.PostAsJsonSnakeCaseAsync($"/employees/{employeeId}/work-authorization", request);

        // Act
        var response = await _client.GetAsync("/work-authorization/expiring?daysUntilExpiration=30");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var result = await response.Content.ReadFromJsonSnakeCaseAsync<IEnumerable<ExpiringAuthorizationResponse>>();
        Assert.NotNull(result);
        Assert.Contains(result, a => a.DocumentNumber == "EXP-001");
    }

    [Fact]
    public async Task Put_ValidRequest_UpdatesWorkAuthorization()
    {
        // Arrange
        var employeeId = Guid.NewGuid();
        var recordRequest = new RecordWorkAuthorizationRequest
        {
            AuthorizationType = AuthorizationType.WorkVisa,
            DocumentNumber = "UPD-001",
            IssueDate = DateTime.UtcNow.AddDays(-30),
            ExpirationDate = DateTime.UtcNow.AddDays(100),
            RightToWorkDocumentId = Guid.NewGuid()
        };
        var recordResponse = await _client.PostAsJsonSnakeCaseAsync($"/employees/{employeeId}/work-authorization", recordRequest);
        var auth = await recordResponse.Content.ReadFromJsonSnakeCaseAsync<WorkAuthorizationResponse>();

        var updateRequest = new UpdateWorkAuthorizationRequest
        {
            ExpirationDate = DateTime.UtcNow.AddDays(200),
            RowVersion = auth!.RowVersion
        };
        
        // Act
        var response = await _client.PutAsJsonSnakeCaseAsync($"/work-authorization/{auth.Id}", updateRequest);

        // Assert
        if (response.StatusCode != HttpStatusCode.OK)
        {
            var error = await response.Content.ReadAsStringAsync();
            throw new Exception($"Update failed with {response.StatusCode}: {error}");
        }
        
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var updated = await response.Content.ReadFromJsonSnakeCaseAsync<WorkAuthorizationResponse>();
        Assert.NotNull(updated);
        Assert.Equal(updateRequest.ExpirationDate, updated.ExpirationDate);
    }

    [Fact]
    public async Task GetById_ReturnsWorkAuthorization()
    {
        // Arrange
        var employeeId = Guid.NewGuid();
        var recordRequest = new RecordWorkAuthorizationRequest
        {
            AuthorizationType = AuthorizationType.Citizen,
            DocumentNumber = "GET-ID-001",
            IssueDate = DateTime.UtcNow.AddDays(-30)
        };
        var recordResponse = await _client.PostAsJsonSnakeCaseAsync($"/employees/{employeeId}/work-authorization", recordRequest);
        var auth = await recordResponse.Content.ReadFromJsonSnakeCaseAsync<WorkAuthorizationResponse>();

        // Act
        var response = await _client.GetAsync($"/work-authorization/{auth!.Id}");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var result = await response.Content.ReadFromJsonSnakeCaseAsync<WorkAuthorizationResponse>();
        Assert.Equal(auth.Id, result!.Id);
    }

    [Fact]
    public async Task GetByEmployee_ReturnsAuthorizations()
    {
        // Arrange
        var employeeId = Guid.NewGuid();
        var recordRequest = new RecordWorkAuthorizationRequest
        {
            AuthorizationType = AuthorizationType.Citizen,
            DocumentNumber = "GET-EMP-001",
            IssueDate = DateTime.UtcNow.AddDays(-30)
        };
        await _client.PostAsJsonSnakeCaseAsync($"/employees/{employeeId}/work-authorization", recordRequest);

        // Act
        var response = await _client.GetAsync($"/employees/{employeeId}/work-authorization");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var result = await response.Content.ReadFromJsonSnakeCaseAsync<IEnumerable<WorkAuthorizationResponse>>();
        Assert.Contains(result!, a => a.DocumentNumber == "GET-EMP-001");
    }
}

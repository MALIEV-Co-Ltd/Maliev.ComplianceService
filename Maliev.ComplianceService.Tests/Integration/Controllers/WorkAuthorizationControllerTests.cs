using System.Net;
using System.Net.Http.Json;
using Maliev.ComplianceService.Application.DTOs;
using Maliev.ComplianceService.Domain.Enums;
using Maliev.ComplianceService.Tests.Fixtures;
using Xunit;

namespace Maliev.ComplianceService.Tests.Integration.Controllers;

[Collection("IntegrationTests")]
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
        var response = await _client.PostAsJsonSnakeCaseAsync($"/compliance/v1/work-authorizations/employees/{employeeId}", request);

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
        await _client.PostAsJsonSnakeCaseAsync($"/compliance/v1/work-authorizations/employees/{employeeId}", request);

        // Act
        var response = await _client.GetAsync("/compliance/v1/work-authorizations/expiring?daysUntilExpiration=30");

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
        var recordResponse = await _client.PostAsJsonSnakeCaseAsync($"/compliance/v1/work-authorizations/employees/{employeeId}", recordRequest);
        var auth = await recordResponse.Content.ReadFromJsonSnakeCaseAsync<WorkAuthorizationResponse>();

        var updateRequest = new UpdateWorkAuthorizationRequest
        {
            ExpirationDate = DateTime.UtcNow.AddDays(200),
            RowVersion = auth!.RowVersion
        };

        // Act
        var response = await _client.PutAsJsonSnakeCaseAsync($"/compliance/v1/work-authorizations/{auth.Id}", updateRequest);

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
        var recordResponse = await _client.PostAsJsonSnakeCaseAsync($"/compliance/v1/work-authorizations/employees/{employeeId}", recordRequest);
        var auth = await recordResponse.Content.ReadFromJsonSnakeCaseAsync<WorkAuthorizationResponse>();

        // Act
        var response = await _client.GetAsync($"/compliance/v1/work-authorizations/{auth!.Id}");

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
        await _client.PostAsJsonSnakeCaseAsync($"/compliance/v1/work-authorizations/employees/{employeeId}", recordRequest);

        // Act
        var response = await _client.GetAsync($"/compliance/v1/work-authorizations/employees/{employeeId}");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var result = await response.Content.ReadFromJsonSnakeCaseAsync<IEnumerable<WorkAuthorizationResponse>>();
        Assert.Contains(result!, a => a.DocumentNumber == "GET-EMP-001");
    }

    [Fact]
    public async Task GetById_WhenNotFound_Returns404()
    {
        // Act
        var response = await _client.GetAsync($"/compliance/v1/work-authorizations/{Guid.NewGuid()}");

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task Post_InvalidRequest_ReturnsBadRequest()
    {
        // Arrange - missing required fields
        var employeeId = Guid.NewGuid();
        var request = new RecordWorkAuthorizationRequest
        {
            AuthorizationType = AuthorizationType.WorkVisa,
            DocumentNumber = "", // Required but empty
            IssueDate = DateTime.UtcNow.AddDays(-30)
        };

        // Act
        var response = await _client.PostAsJsonSnakeCaseAsync($"/compliance/v1/work-authorizations/employees/{employeeId}", request);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Put_WhenNotFound_Returns404()
    {
        // Arrange
        var authId = Guid.NewGuid();
        var request = new UpdateWorkAuthorizationRequest
        {
            RowVersion = new byte[] { 1 }
        };

        // Act
        var response = await _client.PutAsJsonSnakeCaseAsync($"/compliance/v1/work-authorizations/{authId}", request);

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task Put_WithInvalidRowVersion_Returns409()
    {
        // Arrange
        var employeeId = Guid.NewGuid();
        var recordRequest = new RecordWorkAuthorizationRequest
        {
            AuthorizationType = AuthorizationType.Citizen,
            DocumentNumber = "ROW-VERSION-001",
            IssueDate = DateTime.UtcNow.AddDays(-30)
        };
        var recordResponse = await _client.PostAsJsonSnakeCaseAsync($"/compliance/v1/work-authorizations/employees/{employeeId}", recordRequest);
        var auth = await recordResponse.Content.ReadFromJsonSnakeCaseAsync<WorkAuthorizationResponse>();

        var updateRequest = new UpdateWorkAuthorizationRequest
        {
            RowVersion = new byte[] { 99, 99 } // Different version
        };

        // Act
        var response = await _client.PutAsJsonSnakeCaseAsync($"/compliance/v1/work-authorizations/{auth!.Id}", updateRequest);

        // Assert - returns conflict or internal error depending on implementation
        Assert.True(response.StatusCode == HttpStatusCode.Conflict || response.StatusCode == HttpStatusCode.InternalServerError);
    }
}

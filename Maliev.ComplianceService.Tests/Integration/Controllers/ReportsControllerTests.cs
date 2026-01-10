using System.Net;
using System.Net.Http.Json;
using Maliev.ComplianceService.Application.DTOs;
using Maliev.ComplianceService.Tests.Fixtures;
using Xunit;

namespace Maliev.ComplianceService.Tests.Integration.Controllers;

[Collection("IntegrationTests")]
public class ReportsControllerTests : IClassFixture<ComplianceServiceTestFixture>
{
    private readonly ComplianceServiceTestFixture _fixture;
    private readonly HttpClient _client;

    public ReportsControllerTests(ComplianceServiceTestFixture fixture)
    {
        _fixture = fixture;
        _client = fixture.CreateClient();
    }

    [Fact]
    public async Task GetComplianceReport_ReturnsReport()
    {
        // Act
        var response = await _client.GetAsync("/compliance/v1/compliance-reports/compliance");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var result = await response.Content.ReadFromJsonSnakeCaseAsync<ComplianceReportResponse>();
        Assert.NotNull(result);
    }
}

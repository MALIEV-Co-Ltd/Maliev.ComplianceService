using Maliev.ComplianceService.Infrastructure.Services;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Maliev.ComplianceService.Tests.Unit.Services;

public class EmployeeServiceClientTests
{
    private readonly Mock<HttpClient> _httpClientMock;
    private readonly Mock<ILogger<EmployeeServiceClient>> _loggerMock;

    public EmployeeServiceClientTests()
    {
        _httpClientMock = new Mock<HttpClient>();
        _loggerMock = new Mock<ILogger<EmployeeServiceClient>>();
    }

    [Fact]
    public async Task GetEmployeeNameAsync_ReturnsPlaceholderName()
    {
        // Arrange
        var client = new EmployeeServiceClient(_httpClientMock.Object, _loggerMock.Object);
        var employeeId = Guid.NewGuid();

        // Act
        var result = await client.GetEmployeeNameAsync(employeeId);

        // Assert
        Assert.Equal("(name unavailable)", result);
    }

    [Fact]
    public async Task GetEmployeeNameAsync_WithCancellationToken_ReturnsPlaceholderName()
    {
        // Arrange
        var client = new EmployeeServiceClient(_httpClientMock.Object, _loggerMock.Object);
        var employeeId = Guid.NewGuid();
        using var cts = new CancellationTokenSource();

        // Act
        var result = await client.GetEmployeeNameAsync(employeeId, cts.Token);

        // Assert
        Assert.Equal("(name unavailable)", result);
    }
}

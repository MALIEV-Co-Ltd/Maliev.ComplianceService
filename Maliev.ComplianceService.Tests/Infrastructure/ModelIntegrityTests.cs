using Maliev.ComplianceService.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace Maliev.ComplianceService.Tests.Infrastructure;

public class ModelIntegrityTests
{
    [Fact]
    public void Model_ShouldNotHavePendingChanges()
    {
        var options = new DbContextOptionsBuilder<ComplianceDbContext>()
            .UseNpgsql("Host=localhost;Database=ModelCheck")
            .Options;

        using var context = new ComplianceDbContext(options);
        var hasChanges = context.Database.HasPendingModelChanges();

        Assert.False(hasChanges, "Run 'dotnet ef migrations add <Name> --project Maliev.ComplianceService.Infrastructure --startup-project Maliev.ComplianceService.Api'");
    }
}

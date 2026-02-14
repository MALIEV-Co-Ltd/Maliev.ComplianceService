using Maliev.ComplianceService.Infrastructure.Data;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Testcontainers.PostgreSql;
using Testcontainers.Redis;
using MassTransit;

namespace Maliev.ComplianceService.Tests.Fixtures;

public class ComplianceServiceTestFixture : WebApplicationFactory<Program>, IAsyncLifetime
{
    private readonly PostgreSqlContainer _postgreSqlContainer = new PostgreSqlBuilder().WithName("postgres:18-alpine")
        .Build();

    private readonly RedisContainer _redisContainer = new RedisBuilder().WithName("redis:7-alpine")
        .Build();

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");

        // Set environment variables for connection strings (read early in configuration pipeline)
        Environment.SetEnvironmentVariable("ConnectionStrings__ComplianceDbContext", _postgreSqlContainer.GetConnectionString());
        Environment.SetEnvironmentVariable("ConnectionStrings__redis", _redisContainer.GetConnectionString());

        builder.ConfigureServices(services =>
        {
            // Ensure MassTransit waits until started for tests to avoid race conditions
            services.Configure<MassTransitHostOptions>(options =>
            {
                options.WaitUntilStarted = true;
                options.StartTimeout = TimeSpan.FromSeconds(30);
            });

            services.AddMassTransitTestHarness();
        });
    }

    public async Task InitializeAsync()
    {
        await Task.WhenAll(_postgreSqlContainer.StartAsync(), _redisContainer.StartAsync());

        // Ensure database is created and migrated for tests
        using var scope = Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ComplianceDbContext>();
        await context.Database.MigrateAsync();
    }


    public new async Task DisposeAsync()
    {
        await Task.WhenAll(_postgreSqlContainer.StopAsync(), _redisContainer.StopAsync());
    }
}

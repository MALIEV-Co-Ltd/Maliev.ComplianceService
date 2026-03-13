using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using Maliev.ComplianceService.Infrastructure.Data;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using Moq;
using Testcontainers.PostgreSql;
using Testcontainers.Redis;
using MassTransit;
using Xunit;

namespace Maliev.ComplianceService.Tests.Fixtures;

public class ComplianceServiceTestFixture : WebApplicationFactory<Program>, IAsyncLifetime
{
    private static PostgreSqlContainer? _postgresContainer;
    private static RedisContainer? _redisContainer;
    private static bool _containersStarted;
    private static readonly SemaphoreSlim _initLock = new(1, 1);

    private readonly RSA _testRsa;

    public ComplianceServiceTestFixture()
    {
        _testRsa = RSA.Create(2048);
        Environment.SetEnvironmentVariable("ASPNETCORE_ENVIRONMENT", "Testing");
    }

    public async Task InitializeAsync()
    {
        await _initLock.WaitAsync();
        try
        {
            if (!_containersStarted)
            {
                _postgresContainer = 
                #pragma warning disable CS0618
        new PostgreSqlBuilder().WithImage("postgres:18-alpine")
                    .Build();

                _redisContainer = new RedisBuilder()
                    .WithImage("redis:7.4-alpine")
                    .Build();
#pragma warning restore CS0618

                await Task.WhenAll(_postgresContainer.StartAsync(), _redisContainer.StartAsync());

                // Ensure PostgreSQL is ready
                var postgresReady = false;
                var retryCount = 0;
                const int maxRetries = 60;
                while (!postgresReady && retryCount < maxRetries)
                {
                    try
                    {
                        await using var conn = new Npgsql.NpgsqlConnection(_postgresContainer.GetConnectionString());
                        await conn.OpenAsync();
                        await using var cmd = conn.CreateCommand();
                        cmd.CommandText = "SELECT 1";
                        await cmd.ExecuteScalarAsync();
                        postgresReady = true;
                    }
                    catch
                    {
                        retryCount++;
                        await Task.Delay(1000);
                    }
                }

                if (!postgresReady)
                {
                    throw new InvalidOperationException("PostgreSQL Testcontainer failed to become ready after 60 seconds.");
                }

                _containersStarted = true;
            }
        }
        finally
        {
            _initLock.Release();
        }

        Environment.SetEnvironmentVariable("ConnectionStrings__ComplianceDbContext", _postgresContainer!.GetConnectionString());
        Environment.SetEnvironmentVariable("ConnectionStrings__redis", _redisContainer!.GetConnectionString());
        Environment.SetEnvironmentVariable("CORS_ALLOWED_ORIGINS", "http://localhost:3000");
        Environment.SetEnvironmentVariable("IAM__RegistrationDelaySeconds", "0");

        // Apply migrations
        using var scope = Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ComplianceDbContext>();
        await context.Database.MigrateAsync();
    }

    public new async Task DisposeAsync()
    {
        await base.DisposeAsync();
        _testRsa.Dispose();
        Environment.SetEnvironmentVariable("ASPNETCORE_ENVIRONMENT", null);
        Environment.SetEnvironmentVariable("CORS_ALLOWED_ORIGINS", null);
        Environment.SetEnvironmentVariable("IAM__RegistrationDelaySeconds", null);
    }

    protected override IHost CreateHost(IHostBuilder builder)
    {
        if (!_containersStarted)
        {
            InitializeAsync().GetAwaiter().GetResult();
        }

        var rsaParams = _testRsa.ExportParameters(false);
        Environment.SetEnvironmentVariable("JWT_PUBLIC_KEY_MODULUS", Convert.ToBase64String(rsaParams.Modulus!));
        Environment.SetEnvironmentVariable("JWT_PUBLIC_KEY_EXPONENT", Convert.ToBase64String(rsaParams.Exponent!));
        var keyBytes = _testRsa.ExportSubjectPublicKeyInfo();
        Environment.SetEnvironmentVariable("Authentication__Jwt__PublicKey", Convert.ToBase64String(keyBytes));

        return base.CreateHost(builder);
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureLogging(logging =>
        {
            logging.ClearProviders();
            logging.AddConsole();
        });

        builder.ConfigureAppConfiguration((context, config) =>
        {
            config.AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["ConnectionStrings:ComplianceDbContext"] = _postgresContainer!.GetConnectionString(),
                ["ConnectionStrings:redis"] = _redisContainer!.GetConnectionString(),
                ["CORS_ALLOWED_ORIGINS"] = "http://localhost:3000",
                ["IAM:RegistrationDelaySeconds"] = "0"
            });
        });

        builder.ConfigureTestServices(services =>
        {
            // Manual Redis registration
            var redisConnectionString = _redisContainer!.GetConnectionString();
            services.AddSingleton<StackExchange.Redis.IConnectionMultiplexer>(sp =>
            {
                return StackExchange.Redis.ConnectionMultiplexer.Connect(redisConnectionString);
            });

            // Mock IAM
            var iamMock = new Mock<Maliev.Aspire.ServiceDefaults.IAM.IIamServiceClient>();
            iamMock.Setup(x => x.CheckPermissionAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(false);
            iamMock.Setup(x => x.GetUserPermissionsAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(Enumerable.Empty<string>());
            services.AddSingleton(iamMock.Object);

            var statusTracker = new Maliev.Aspire.ServiceDefaults.IAM.IAMRegistrationStatusTracker();
            statusTracker.MarkRegistered();
            services.AddSingleton(statusTracker);

            // JWT options
            services.PostConfigureAll<JwtBearerOptions>(options =>
            {
                options.MapInboundClaims = false;
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = "test-issuer",
                    ValidAudience = "test-audience",
                    IssuerSigningKey = new RsaSecurityKey(_testRsa),
                    ClockSkew = TimeSpan.Zero,
                    NameClaimType = "sub",
                    RoleClaimType = "role"
                };
                options.TokenValidationParameters.SignatureValidator = null;
            });

            services.AddMassTransitTestHarness();

            // Disable background services
            var backgroundServicesToDisable = new[]
            {
                "WorkAuthorizationExpirationReminderService",
                "ExpiredWorkAuthorizationFlaggingService",
                "BackgroundIAMRegistrationService"
            };

            var descriptors = services.Where(d =>
                d.ServiceType == typeof(IHostedService) &&
                backgroundServicesToDisable.Contains(d.ImplementationType?.Name)).ToList();

            foreach (var descriptor in descriptors)
            {
                services.Remove(descriptor);

                // Add as singleton for direct access in tests
                if (descriptor.ImplementationType != null &&
                    (descriptor.ImplementationType.Name == "WorkAuthorizationExpirationReminderService" ||
                     descriptor.ImplementationType.Name == "ExpiredWorkAuthorizationFlaggingService"))
                {
                    services.AddSingleton(descriptor.ImplementationType);
                }
            }
        });
    }

    public string CreateTestJwtToken(string userId = "test-user", string[]? roles = null, string[]? permissions = null)
    {
        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, userId),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
        };

        if (roles != null) foreach (var role in roles) claims.Add(new Claim(ClaimTypes.Role, role));
        if (permissions != null) foreach (var permission in permissions) claims.Add(new Claim("permissions", permission));

        var signingCredentials = new SigningCredentials(new RsaSecurityKey(_testRsa), SecurityAlgorithms.RsaSha256);
        var token = new JwtSecurityToken("test-issuer", "test-audience", claims, expires: DateTime.UtcNow.AddHours(1), signingCredentials: signingCredentials);
        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    public HttpClient CreateAuthenticatedClient(string userId = "test-user", string[]? roles = null, string[]? permissions = null)
    {
        var token = CreateTestJwtToken(userId, roles, permissions);
        var client = CreateClient();
        client.DefaultRequestHeaders.Add("Authorization", $"Bearer {token}");
        return client;
    }
}





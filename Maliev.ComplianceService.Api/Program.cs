using Maliev.Aspire.ServiceDefaults;
using Maliev.ComplianceService.Infrastructure.Data;
using Maliev.ComplianceService.Application.Interfaces;
using Maliev.ComplianceService.Infrastructure.Repositories;
using Maliev.ComplianceService.Application.Commands.RecordWorkAuthorization;
using Maliev.ComplianceService.Infrastructure.Services;
using Maliev.ComplianceService.Infrastructure.BackgroundServices;
using Maliev.ComplianceService.Infrastructure.Consumers;
using Maliev.ComplianceService.Infrastructure.IAM;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

// Initialize bootstrap logging
using var loggerFactory = LoggerFactory.Create(logBuilder => logBuilder.AddConsole());
var bootstrapLogger = loggerFactory.CreateLogger("Program");

try
{
    bootstrapLogger.LogInformation("Starting Compliance Service host");

    var builder = WebApplication.CreateBuilder(args);

    // --- Secrets & Configuration ---
    builder.AddGoogleSecretManagerVolume();

    // --- Infrastructure & Observability ---
    builder.AddServiceDefaults();
    builder.AddStandardMiddleware(options =>
    {
        options.EnableRequestLogging = true;
    });
    builder.AddServiceMeters("compliance-meter");

    // Database
    builder.AddPostgresDbContext<ComplianceDbContext>(connectionName: "ComplianceDbContext");

    // Redis
    builder.AddRedisDistributedCache(instanceName: "compliance:");

    // MassTransit
    builder.AddMassTransitWithRabbitMq(x =>
    {
        x.AddConsumer<EmployeeCreatedEventConsumer>();
        x.AddConsumer<EmployeeTerminatedEventConsumer>();
        x.AddConsumer<TrainingCompletedEventConsumer>();
    });

    // Authentication & Authorization
    builder.AddJwtAuthentication();

    // IAM Registration
    builder.AddIAMServiceClient("compliance");
    builder.Services.AddIAMRegistration<ComplianceIAMRegistrationService>("compliance");

    // --- API Configuration ---
    builder.AddDefaultCors();
    builder.AddDefaultApiVersioning();
    builder.AddStandardRateLimiting();

    if (!builder.Environment.IsProduction())
    {
        builder.AddStandardOpenApi(
            title: "MALIEV Compliance Service API",
            description: "Manages employee work authorizations and compliance alerts.");
    }

    builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.SnakeCaseLower;
    });

    // --- Application Services ---
    builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(RecordWorkAuthorizationCommand).Assembly));

    builder.Services.AddScoped<IWorkAuthorizationRepository, WorkAuthorizationRepository>();
    builder.Services.AddScoped<IComplianceAlertRepository, ComplianceAlertRepository>();

    builder.AddServiceClient<IEmployeeService, EmployeeServiceClient>("EmployeeService");

    builder.Services.AddHostedService<WorkAuthorizationExpirationReminderService>();
    builder.Services.AddHostedService<ExpiredWorkAuthorizationFlaggingService>();

    var app = builder.Build();
    var logger = app.Services.GetRequiredService<ILogger<Program>>();

    // --- Database Migrations ---
    await app.MigrateDatabaseAsync<ComplianceDbContext>();

    // --- Middleware Pipeline ---
    app.UseStandardMiddleware();
    if (!app.Environment.IsDevelopment())
    {
        app.UseHttpsRedirection();
    }
    app.UseRouting();
    app.UseCors();
    app.UseAuthentication();
    app.UseAuthorization();
    app.UseRateLimiter();

    // --- Endpoints ---
    app.MapControllers();
    app.MapDefaultEndpoints(servicePrefix: "compliance");
    app.MapApiDocumentation(servicePrefix: "compliance");

    logger.LogInformation("Compliance Service started successfully");
    await app.RunAsync();
}
catch (Exception ex)
{
    bootstrapLogger.LogCritical(ex, "Compliance Service host terminated unexpectedly during startup");
    throw;
}
finally
{
    loggerFactory.Dispose();
}

/// <summary>
/// Entry point for the Compliance Service API.
/// </summary>
public partial class Program { }

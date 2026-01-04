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

var builder = WebApplication.CreateBuilder(args);

// --- Secrets & Configuration ---
builder.AddGoogleSecretManagerVolume();

// --- Infrastructure & Observability ---
builder.AddServiceDefaults();
builder.AddStandardMiddleware(options =>
{
    options.EnableRequestLogging = true;
});
builder.AddServiceMeters("compliance-service");

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
builder.Services.AddIAMRegistration<ComplianceIAMRegistrationService>();

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

// --- Database Migrations ---
try
{
    await app.MigrateDatabaseAsync<ComplianceDbContext>();
}
catch (Exception ex)
{
    var logger = app.Services.GetRequiredService<ILogger<Program>>();
    logger.LogError(ex, "Database migration failed");
}

// --- Middleware Pipeline ---
app.UseStandardMiddleware();
app.UseHttpsRedirection();
app.UseRouting();
app.UseCors();
app.UseAuthentication();
app.UseAuthorization();
app.UseRateLimiter();

// --- Endpoints ---
app.MapControllers();
app.MapDefaultEndpoints(servicePrefix: "compliance");
app.MapApiDocumentation(servicePrefix: "compliance");

await app.RunAsync();

/// <summary>
/// Entry point for the Compliance Service API.
/// </summary>
public partial class Program { }
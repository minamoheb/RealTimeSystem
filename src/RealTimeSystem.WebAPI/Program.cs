using System.Threading.RateLimiting;
using Asp.Versioning;
using Microsoft.AspNetCore.RateLimiting;
using Hangfire;
using Microsoft.OpenApi.Models;
using RealTimeSystem.Application;
using RealTimeSystem.Application.Common.Interfaces;
using RealTimeSystem.Infrastructure;
using RealTimeSystem.Infrastructure.BackgroundJobs;
using RealTimeSystem.WebAPI.Filters;
using RealTimeSystem.WebAPI.Hubs;
using RealTimeSystem.WebAPI.Middleware;
using Serilog;
using RealTimeSystem.Infrastructure.Persistence.DBContext;

var builder = WebApplication.CreateBuilder(args);

builder.Host.UseSerilog((context, _, configuration) => configuration
    .ReadFrom.Configuration(context.Configuration)
    .MinimumLevel.Override("Microsoft.EntityFrameworkCore.Database.Command", Serilog.Events.LogEventLevel.Warning)
    .Enrich.FromLogContext()
    .WriteTo.Console(outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff} [{Level:u3}] {Message:lj}{NewLine}{Exception}")
    .WriteTo.File(
        Path.Combine(context.HostingEnvironment.ContentRootPath, "logs", "log-.txt"),
        rollingInterval: RollingInterval.Day,
        outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff} [{Level:u3}] {Message:lj}{NewLine}{Exception}"));

builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddSingleton<IPresenceTracker, PresenceTracker>();
builder.Services.AddScoped<IMonitoringNotifier, SignalRMonitoringNotifier>();
builder.Services.AddSignalR();
builder.Services.AddProblemDetails();
builder.Services.AddControllers();
builder.Services.AddApiVersioning(options =>
{
    options.DefaultApiVersion = new ApiVersion(1, 0);
    options.AssumeDefaultVersionWhenUnspecified = true;
    options.ReportApiVersions = true;
    options.ApiVersionReader = new UrlSegmentApiVersionReader();
}).AddApiExplorer(options =>
{
    options.GroupNameFormat = "'v'VVV";
    options.SubstituteApiVersionInUrl = true;
});

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Real-Time Monitoring API",
        Version = "v1",
        Description = "System monitoring, reports, and background jobs."
    });
    options.SwaggerDoc("v2", new OpenApiInfo
    {
        Title = "Real-Time Monitoring API",
        Version = "v2",
        Description = "Version 2 adds a computed health score to the server list."
    });
    options.DocInclusionPredicate((document, description) => description.GroupName == document);
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Paste a JWT access token."
    });
    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer" }
            },
            Array.Empty<string>()
        }
    });
});

builder.Services.AddRateLimiter(options =>
{
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
    options.AddFixedWindowLimiter("api", limiter =>
    {
        limiter.PermitLimit = 120;
        limiter.Window = TimeSpan.FromMinutes(1);
        limiter.QueueLimit = 0;
    });
    options.AddFixedWindowLimiter("auth", limiter =>
    {
        limiter.PermitLimit = 20;
        limiter.Window = TimeSpan.FromMinutes(1);
        limiter.QueueLimit = 0;
    });
});

var origins = builder.Configuration.GetSection("Cors:Origins").Get<string[]>() ?? ["http://localhost:4200"];
builder.Services.AddCors(options => options.AddPolicy("client", policy => policy
    .WithOrigins(origins)
    .AllowAnyHeader()
    .AllowAnyMethod()
    .AllowCredentials()));

var app = builder.Build();

// Hangfire resolves JobStorage as soon as the dashboard is mapped. Create the
// catalog and application schema first, or the storage install is skipped and
// the recurring job later fails with "Invalid object name 'HangFire.Hash'".
await HangfireStartup.EnsureDatabaseAsync(app);
await app.InitializeDatabaseAsync();
await HangfireStartup.EnsureSchemaAsync(app);

app.UseMiddleware<ExceptionHandlingMiddleware>();
app.UseMiddleware<RequestLoggingMiddleware>();

if (app.Environment.IsDevelopment() || app.Environment.IsStaging())
{
    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/swagger/v1/swagger.json", "Monitoring API v1");
        options.SwaggerEndpoint("/swagger/v2/swagger.json", "Monitoring API v2");
    });
}

app.UseCors("client");
app.UseRateLimiter();
app.UseAuthentication();
app.UseAuthorization();

if (app.Configuration.GetValue("Hangfire:Enabled", true))
{
    app.UseHangfireDashboard("/hangfire", new DashboardOptions
    {
        Authorization = [new HangfireDashboardAuthorizationFilter(app.Environment)]
    });
}

app.MapControllers();
app.MapHub<MonitoringHub>("/hubs/monitoring");
app.MapHealthChecks("/health").AllowAnonymous();

HangfireStartup.RegisterJobs(app);

var applicationStarted = DateTime.UtcNow;
Log.Information("Monitoring API ready. StartDate {StartDate:o}", applicationStarted);
app.Lifetime.ApplicationStopping.Register(() =>
    Log.Information("Monitoring API stopping. StartDate {StartDate:o} EndDate {EndDate:o}", applicationStarted, DateTime.UtcNow));

app.Run();

public partial class Program;

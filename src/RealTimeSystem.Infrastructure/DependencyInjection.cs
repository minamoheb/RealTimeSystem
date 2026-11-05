using System.Text;
using Hangfire;
using Hangfire.SqlServer;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using RealTimeSystem.Application.Common.Interfaces;
using RealTimeSystem.Domain.Services;
using RealTimeSystem.Infrastructure.BackgroundJobs;
using RealTimeSystem.Infrastructure.Persistence.DBContext;
using RealTimeSystem.Infrastructure.Persistence.Repository;
using RealTimeSystem.Infrastructure.Persistence.UnitOfWork;
using RealTimeSystem.Infrastructure.Services.Monitoring;
using RealTimeSystem.Infrastructure.Services.Security;

namespace RealTimeSystem.Infrastructure
{
public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<JwtOptions>(configuration.GetSection(JwtOptions.SectionName));
        services.Configure<ThresholdOptions>(configuration.GetSection(ThresholdOptions.SectionName));

        var provider = configuration["Database:Provider"] ?? "SqlServer";
        services.AddDbContext<ApplicationDbContext>((sp, options) =>
        {
            if (!provider.Equals("SqlServer", StringComparison.OrdinalIgnoreCase))
            {
                throw new InvalidOperationException("Database:Provider must be SqlServer.");
            }

            var connectionString = configuration.GetConnectionString("SqlServer")
                ?? throw new InvalidOperationException("ConnectionStrings:SqlServer is required when Database:Provider is SqlServer.");
            options.UseSqlServer(connectionString);
            options.ConfigureWarnings(warnings => warnings.Ignore(RelationalEventId.CommandExecuted));
        });
        services.AddScoped<DbContext>(sp => sp.GetRequiredService<ApplicationDbContext>());
        services.AddScoped<IApplicationDbContext>(sp => sp.GetRequiredService<ApplicationDbContext>());
        services.AddScoped(typeof(IRepository<>), typeof(EfRepository<>));
        services.AddScoped<IUnitOfWork, EfUnitOfWork>();
        services.AddHealthChecks().AddDbContextCheck<ApplicationDbContext>("database");

        services.AddHttpContextAccessor();
        services.AddSingleton<IDateTimeProvider, SystemDateTimeProvider>();
        services.AddSingleton<IPasswordHasher, Pbkdf2PasswordHasher>();
        services.AddSingleton<ITokenService, JwtTokenService>();
        services.AddSingleton<IRefreshTokenLifetime, RefreshTokenLifetime>();
        services.AddScoped<ICurrentUser, HttpCurrentUser>();
        services.AddScoped<IAuthTokenIssuer, AuthTokenIssuer>();
        services.AddSingleton<IMetricSampleFactory, SimulatedMetricSampleFactory>();
        services.AddScoped<IMonitoringNotifier, NullMonitoringNotifier>();
        services.AddScoped<IDomainEventDispatcher, DomainEventDispatcher>();
        services.AddScoped<DatabaseStartup>();

        services.AddSingleton<IAnomalyRule>(sp =>
        {
            var thresholds = sp.GetRequiredService<Microsoft.Extensions.Options.IOptions<ThresholdOptions>>().Value;
            return new CpuAnomalyRule(thresholds.CpuWarning, thresholds.CpuCritical);
        });
        services.AddSingleton<IAnomalyRule>(sp =>
        {
            var thresholds = sp.GetRequiredService<Microsoft.Extensions.Options.IOptions<ThresholdOptions>>().Value;
            return new MemoryAnomalyRule(thresholds.MemoryWarning, thresholds.MemoryCritical);
        });
        services.AddSingleton<IAnomalyRule>(sp =>
        {
            var thresholds = sp.GetRequiredService<Microsoft.Extensions.Options.IOptions<ThresholdOptions>>().Value;
            return new DiskAnomalyRule(thresholds.DiskWarning, thresholds.DiskCritical);
        });
        services.AddSingleton<IAnomalyRule>(sp =>
        {
            var thresholds = sp.GetRequiredService<Microsoft.Extensions.Options.IOptions<ThresholdOptions>>().Value;
            return new ResponseTimeAnomalyRule(thresholds.ResponseWarning, thresholds.ResponseCritical);
        });
        services.AddSingleton<AnomalyDetectionService>();

        services.AddScoped<MetricsCollectionJob>();
        services.AddScoped<GenerateReportJob>();
        services.AddScoped<AnalyzeReportJob>();
        services.AddScoped<MaintenanceJob>();

        var hangfireEnabled = configuration.GetValue("Hangfire:Enabled", true);
        if (hangfireEnabled)
        {
            if (!provider.Equals("SqlServer", StringComparison.OrdinalIgnoreCase))
            {
                throw new InvalidOperationException("Hangfire requires Database:Provider=SqlServer.");
            }

            services.AddHangfire((sp, config) =>
            {
                config.SetDataCompatibilityLevel(CompatibilityLevel.Version_180)
                    .UseSimpleAssemblyNameTypeSerializer()
                    .UseRecommendedSerializerSettings();

                var connectionString = configuration.GetConnectionString("SqlServer")
                    ?? throw new InvalidOperationException("ConnectionStrings:SqlServer is required when Hangfire is enabled.");
                config.UseSqlServerStorage(connectionString, new SqlServerStorageOptions
                {
                    PrepareSchemaIfNecessary = true
                });
            });
            services.AddHangfireServer(options => options.WorkerCount = 2);
            services.AddSingleton<IBackgroundJobScheduler, HangfireJobScheduler>();
            services.AddSingleton<IJobMonitoringReader, HangfireJobMonitoringReader>();
        }
        else
        {
            services.AddSingleton<IBackgroundJobScheduler, InlineJobScheduler>();
            services.AddSingleton<IJobMonitoringReader, EmptyJobMonitoringReader>();
        }

        var jwt = configuration.GetSection(JwtOptions.SectionName).Get<JwtOptions>() ?? new JwtOptions();
        if (string.IsNullOrWhiteSpace(jwt.Key) || jwt.Key.Length < 32)
        {
            throw new InvalidOperationException("Jwt:Key must be configured with at least 32 characters.");
        }

        services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                options.MapInboundClaims = false;
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateIssuerSigningKey = true,
                    ValidateLifetime = true,
                    ValidIssuer = jwt.Issuer,
                    ValidAudience = jwt.Audience,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwt.Key)),
                    ClockSkew = TimeSpan.FromMinutes(1),
                    RoleClaimType = "role",
                    NameClaimType = "name"
                };
                options.Events = new JwtBearerEvents
                {
                    OnMessageReceived = context =>
                    {
                        var path = context.HttpContext.Request.Path;
                        if (path.StartsWithSegments("/hubs") &&
                            context.Request.Query.TryGetValue("access_token", out var accessToken) &&
                            !string.IsNullOrEmpty(accessToken))
                        {
                            context.Token = accessToken;
                        }
                        else if (path.StartsWithSegments("/hangfire") &&
                            context.Request.Cookies.TryGetValue("rts.hangfire", out var hangfireToken) &&
                            !string.IsNullOrEmpty(hangfireToken))
                        {
                            context.Token = hangfireToken;
                        }

                        return Task.CompletedTask;
                    }
                };
            });

        services.AddAuthorization(options =>
        {
            options.FallbackPolicy = new Microsoft.AspNetCore.Authorization.AuthorizationPolicyBuilder()
                .RequireAssertion(context =>
                {
                    if (context.Resource is HttpContext http &&
                        http.Request.Path.StartsWithSegments("/hangfire"))
                    {
                        return true;
                    }

                    return context.User.Identity?.IsAuthenticated == true;
                })
                .Build();
        });

        return services;
    }

    public static void RegisterRecurringJobs()
    {
        RecurringJob.AddOrUpdate<MetricsCollectionJob>(
            "collect-system-metrics",
            job => job.Execute(),
            "*/1 * * * *");
    }
}
}

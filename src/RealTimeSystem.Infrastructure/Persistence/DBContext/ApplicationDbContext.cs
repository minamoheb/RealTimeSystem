using Microsoft.EntityFrameworkCore;
using RealTimeSystem.Application.Common.Interfaces;
using RealTimeSystem.Domain.Common;
using RealTimeSystem.Domain.Entities;

namespace RealTimeSystem.Infrastructure.Persistence.DBContext
{

    public sealed class ApplicationDbContext : DbContext, IApplicationDbContext
    {
        private readonly IDomainEventDispatcher? _dispatcher;

        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options, IDomainEventDispatcher? dispatcher = null)
            : base(options)
        {
            _dispatcher = dispatcher;
        }

        public DbSet<MonitoredServer> Servers => Set<MonitoredServer>();

        public DbSet<MetricSample> Metrics => Set<MetricSample>();

        public DbSet<Alert> Alerts => Set<Alert>();

        public DbSet<Report> Reports => Set<Report>();

        public DbSet<User> Users => Set<User>();

        public DbSet<Role> Roles => Set<Role>();

        public DbSet<Permission> Permissions => Set<Permission>();

        public DbSet<UserRole> UserRoles => Set<UserRole>();

        public DbSet<RolePermission> RolePermissions => Set<RolePermission>();

        public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();

        public DbSet<MaintenanceWindow> MaintenanceWindows => Set<MaintenanceWindow>();

        public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            var aggregates = ChangeTracker.Entries<IHasDomainEvents>()
                .Select(entry => entry.Entity)
                .Where(entity => entity.DomainEvents.Count > 0)
                .ToList();

            var result = await base.SaveChangesAsync(cancellationToken);
            if (aggregates.Count == 0 || _dispatcher is null)
            {
                foreach (var aggregate in aggregates)
                {
                    aggregate.ClearDomainEvents();
                }

                return result;
            }

            var events = aggregates.SelectMany(aggregate => aggregate.DomainEvents).ToList();
            foreach (var aggregate in aggregates)
            {
                aggregate.ClearDomainEvents();
            }

            await _dispatcher.DispatchAsync(events, cancellationToken);
            return result;
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);
            base.OnModelCreating(modelBuilder);
        }
    }
}

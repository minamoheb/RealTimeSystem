using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace RealTimeSystem.Infrastructure.Persistence.DBContext
{
public sealed class DesignTimeDbContextFactory : IDesignTimeDbContextFactory<ApplicationDbContext>
{
    public ApplicationDbContext CreateDbContext(string[] args)
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseSqlServer("Server=(localdb)\\MSSQLLocalDB;Database=RealTimeMonitoring;Trusted_Connection=True;TrustServerCertificate=True;MultipleActiveResultSets=true")
            .Options;
        return new ApplicationDbContext(options);
    }
}
}

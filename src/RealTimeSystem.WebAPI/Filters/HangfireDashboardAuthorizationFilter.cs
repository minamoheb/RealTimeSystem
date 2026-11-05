using Hangfire.Dashboard;

namespace RealTimeSystem.WebAPI.Filters
{
public sealed class HangfireDashboardAuthorizationFilter : IDashboardAuthorizationFilter
{
    private readonly IWebHostEnvironment _environment;

    public HangfireDashboardAuthorizationFilter(IWebHostEnvironment environment)
    {
        _environment = environment;
    }

    public bool Authorize(DashboardContext context)
    {
        if (_environment.IsDevelopment())
        {
            return true;
        }

        var user = context.GetHttpContext().User;
        return user.Identity?.IsAuthenticated == true && user.IsInRole("Admin");
    }
}
}


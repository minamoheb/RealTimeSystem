using RealTimeSystem.Application.Common.Interfaces;

namespace RealTimeSystem.Infrastructure.Services.Security
{
    public sealed class SystemDateTimeProvider : IDateTimeProvider
    {
        public DateTime UtcNow => DateTime.UtcNow;
    }
}

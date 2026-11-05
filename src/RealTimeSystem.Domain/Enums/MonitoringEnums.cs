namespace RealTimeSystem.Domain.Enums
{
    public enum ServerStatus
    {
        Healthy = 0,
        Degraded = 1,
        Maintenance = 2,
        Offline = 3
    }

    public enum AlertSeverity
    {
        Info = 0,
        Warning = 1,
        Critical = 2
    }

    public enum ReportStatus
    {
        Queued = 0,
        Generating = 1,
        Analyzing = 2,
        Ready = 3,
        Failed = 4
    }

    public enum MaintenanceStatus
    {
        Scheduled = 0,
        InProgress = 1,
        Completed = 2
    }
}


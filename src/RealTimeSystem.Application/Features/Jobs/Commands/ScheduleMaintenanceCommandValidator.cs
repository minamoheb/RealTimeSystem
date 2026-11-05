namespace RealTimeSystem.Application.Features.Jobs
{
    public sealed class ScheduleMaintenanceCommandValidator : AbstractValidator<ScheduleMaintenanceCommand>
    {
        public ScheduleMaintenanceCommandValidator()
        {
            RuleFor(command => command.ServerId).NotEmpty();
            RuleFor(command => command.Reason).NotEmpty().MaximumLength(300);
            RuleFor(command => command.DelayMinutes).InclusiveBetween(1, 1440);
        }
    }
}


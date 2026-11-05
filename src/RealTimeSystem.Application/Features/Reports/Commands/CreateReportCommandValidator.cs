namespace RealTimeSystem.Application.Features.Reports
{
    public sealed class CreateReportCommandValidator : AbstractValidator<CreateReportCommand>
    {
        public CreateReportCommandValidator()
        {
            RuleFor(command => command.Title).NotEmpty().MaximumLength(160);
            RuleFor(command => command.ToUtc).GreaterThan(command => command.FromUtc)
                .WithMessage("The report end must be after the start.");
        }
    }
}


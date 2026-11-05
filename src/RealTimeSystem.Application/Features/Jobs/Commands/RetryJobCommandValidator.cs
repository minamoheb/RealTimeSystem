namespace RealTimeSystem.Application.Features.Jobs
{
    public sealed class RetryJobCommandValidator : AbstractValidator<RetryJobCommand>
    {
        public RetryJobCommandValidator()
        {
            RuleFor(command => command.JobId).NotEmpty().MaximumLength(100);
        }
    }
}

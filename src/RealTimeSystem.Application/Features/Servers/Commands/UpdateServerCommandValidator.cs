namespace RealTimeSystem.Application.Features.Servers
{
public sealed class UpdateServerCommandValidator : AbstractValidator<UpdateServerCommand>
{
    public UpdateServerCommandValidator()
    {
        RuleFor(command => command.Id).NotEmpty();
        RuleFor(command => command.Name).NotEmpty().MaximumLength(120);
        RuleFor(command => command.HostName).NotEmpty().MaximumLength(200);
        RuleFor(command => command.EnvironmentName).NotEmpty().MaximumLength(40);
        RuleFor(command => command.Description).MaximumLength(500);
    }
}
}


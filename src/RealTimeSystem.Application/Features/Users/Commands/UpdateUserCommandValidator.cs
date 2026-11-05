namespace RealTimeSystem.Application.Features.Users
{
public sealed class UpdateUserCommandValidator : AbstractValidator<UpdateUserCommand>
{
    public UpdateUserCommandValidator()
    {
        RuleFor(command => command.Id).NotEmpty();
        RuleFor(command => command.FullName).NotEmpty().MaximumLength(160);
        RuleFor(command => command.Roles).NotNull();
    }
}
}


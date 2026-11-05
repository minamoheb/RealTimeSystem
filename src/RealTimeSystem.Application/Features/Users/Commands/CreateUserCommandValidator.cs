namespace RealTimeSystem.Application.Features.Users
{
public sealed class CreateUserCommandValidator : AbstractValidator<CreateUserCommand>
{
    public CreateUserCommandValidator()
    {
        RuleFor(command => command.Email).NotEmpty().EmailAddress().MaximumLength(256);
        RuleFor(command => command.FullName).NotEmpty().MaximumLength(160);
        RuleFor(command => command.Password).NotEmpty().MinimumLength(8).MaximumLength(128);
        RuleFor(command => command.Roles).NotNull();
    }
}
}


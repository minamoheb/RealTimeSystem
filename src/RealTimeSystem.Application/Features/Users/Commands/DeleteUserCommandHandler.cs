namespace RealTimeSystem.Application.Features.Users
{
    public sealed record DeleteUserCommand(long Id) : IRequest<bool>;
    public sealed class DeleteUserCommandHandler(
    IUnitOfWork unitOfWork,
    ICurrentUser currentUser) : IRequestHandler<DeleteUserCommand, bool>
    {
        public async Task<bool> Handle(DeleteUserCommand request, CancellationToken cancellationToken)
        {
            if (currentUser.UserId == request.Id)
            {
                throw new ForbiddenException("You cannot deactivate your own account.");
            }

            var users = unitOfWork.Repository<User>();
            var user = await users.Query(asNoTracking: false).FirstOrDefaultAsync(candidate => candidate.Id == request.Id, cancellationToken)
                ?? throw new NotFoundException(nameof(User), request.Id);
            user.Deactivate();
            await unitOfWork.SaveChangesAsync(cancellationToken);
            return true;
        }
    }
}

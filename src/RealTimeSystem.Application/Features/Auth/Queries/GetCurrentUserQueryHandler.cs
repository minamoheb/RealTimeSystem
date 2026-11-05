using RealTimeSystem.Application.Common.Mapping;

namespace RealTimeSystem.Application.Features.Auth
{
    public sealed record GetCurrentUserQuery : IRequest<UserProfileDto>;
    public sealed class GetCurrentUserQueryHandler(
    IUnitOfWork unitOfWork,
    ICurrentUser currentUser) : IRequestHandler<GetCurrentUserQuery, UserProfileDto>
    {
        public async Task<UserProfileDto> Handle(GetCurrentUserQuery request, CancellationToken cancellationToken)
        {
            if (currentUser.UserId is not long userId)
            {
                throw new UnauthorizedAppException("Authentication is required.");
            }

            var users = unitOfWork.Repository<User>();
            var user = await users.FirstOrDefaultAsync(
                predicate: candidate => candidate.Id == userId,
                include: query => query
                    .Include(candidate => candidate.UserRoles)
                    .ThenInclude(userRole => userRole.Role),
                cancellationToken: cancellationToken);

            if (user is null || !user.IsActive)
            {
                throw new UnauthorizedAppException("Authentication is required.");
            }

            return user.ToProfile();
        }
    }
}

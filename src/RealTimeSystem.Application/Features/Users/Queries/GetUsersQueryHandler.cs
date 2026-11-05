using RealTimeSystem.Application.Common.Mapping;
using System.Linq.Expressions;

namespace RealTimeSystem.Application.Features.Users
{
    public sealed record GetUsersQuery : Pagination, IRequest<PagedResult<UserDto>>
    {
        public string? Search { get; init; }

        public GetUsersQuery()
        {
            PageSize = 20;
        }
    }
    public sealed class GetUsersQueryHandler(IUnitOfWork unitOfWork) : IRequestHandler<GetUsersQuery, PagedResult<UserDto>>
    {
        public async Task<PagedResult<UserDto>> Handle(GetUsersQuery request, CancellationToken cancellationToken)
        {
            var usersRepository = unitOfWork.Repository<User>();
            var (page, pageSize) = Paging.Normalize(request.Page, request.PageSize);
            var term = request.Search?.Trim() ?? string.Empty;
            var hasSearch = term.Length > 0;

            Expression<Func<User, bool>> predicate = user =>
                !hasSearch || EF.Functions.Like(user.Email, $"%{term}%") || EF.Functions.Like(user.FullName, $"%{term}%");

            var total = await usersRepository.CountAsync(predicate, cancellationToken);
            var users = await usersRepository.SelectListAsync(
                selector: UserMappings.UserDtoProjection,
                predicate: predicate,
                orderBy: query => query.OrderBy(user => user.FullName),
                skip: (page - 1) * pageSize,
                take: pageSize,
                cancellationToken: cancellationToken);

            return PagedResult<UserDto>.Create(users, page, pageSize, total);
        }
    }
}

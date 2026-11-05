using RealTimeSystem.Domain.Constants;

namespace RealTimeSystem.Application.Features.Auth
{
    public static class RoleGuard
    {
        public static async Task<IReadOnlyList<Role>> ResolveRolesAsync(IUnitOfWork unitOfWork, IReadOnlyList<string> requestedRoles, CancellationToken cancellationToken)
        {
            var rolesRepository = unitOfWork.Repository<Role>();
            var names = requestedRoles
                .Select(role => role.Trim())
                .Where(role => role.Length > 0)
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToArray();

            if (names.Length == 0)
            {
                names = [RoleNames.User];
            }

            var allRoles = await rolesRepository.Query().ToListAsync(cancellationToken);
            var roles = allRoles.Where(role => names.Contains(role.Name, StringComparer.OrdinalIgnoreCase)).ToList();
            if (roles.Count != names.Length)
            {
                throw new ValidationAppException(new Dictionary<string, string[]>
                {
                    ["Roles"] = ["One or more roles are unknown. Use Admin or User."]
                });
            }

            return roles;
        }
    }
}

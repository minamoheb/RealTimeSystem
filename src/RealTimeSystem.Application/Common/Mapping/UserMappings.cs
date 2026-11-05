using Mapster;
using System.Linq.Expressions;

namespace RealTimeSystem.Application.Common.Mapping
{
    public static class UserMappings
    {
        private static readonly TypeAdapterConfig UserConfig = BuildUserConfig();
        private static readonly TypeAdapterConfig ProfileConfig = BuildProfileConfig();

        public static readonly Expression<Func<User, UserDto>> UserDtoProjection = user =>
            new UserDto(
                user.Id,
                user.Email,
                user.FullName,
                user.IsActive,
                user.CreatedAtUtc,
                user.UserRoles.Select(userRole => userRole.Role.Name).OrderBy(name => name).ToArray());

        public static UserDto ToDto(this User user) => user.Adapt<UserDto>(UserConfig);

        public static UserProfileDto ToProfile(this User user) => user.Adapt<UserProfileDto>(ProfileConfig);

        private static TypeAdapterConfig BuildUserConfig()
        {
            var config = new TypeAdapterConfig();
            config.NewConfig<User, UserDto>()
                .Map(destination => destination.Roles, source => source.UserRoles.Select(userRole => userRole.Role.Name).OrderBy(name => name).ToArray());

            return config;
        }

        private static TypeAdapterConfig BuildProfileConfig()
        {
            var config = new TypeAdapterConfig();
            config.NewConfig<User, UserProfileDto>()
                .Map(destination => destination.Roles, source => source.UserRoles.Select(userRole => userRole.Role.Name).OrderBy(name => name).ToArray());

            return config;
        }
    }
}

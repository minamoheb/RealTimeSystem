using Microsoft.Extensions.DependencyInjection;
using RealTimeSystem.Application.Common.Behaviors;

namespace RealTimeSystem.Application
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddApplication(this IServiceCollection services)
        {
            services.AddValidatorsFromAssembly(typeof(DependencyInjection).Assembly);
            services.AddScoped<ISender, Mediator>();
            services.AddScoped(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));
            services.AddScoped(typeof(IPipelineBehavior<,>), typeof(LoggingBehavior<,>));

            var handlerInterface = typeof(IRequestHandler<,>);
            var handlers = typeof(DependencyInjection).Assembly
                .GetTypes()
                .Where(type => type is { IsAbstract: false, IsInterface: false })
                .Select(type => new
                {
                    Implementation = type,
                    Service = type.GetInterfaces().FirstOrDefault(candidate =>
                        candidate.IsGenericType && candidate.GetGenericTypeDefinition() == handlerInterface)
                })
                .Where(pair => pair.Service is not null);

            foreach (var handler in handlers)
            {
                services.AddScoped(handler.Service!, handler.Implementation);
            }

            return services;
        }
    }
}


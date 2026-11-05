using Microsoft.Extensions.DependencyInjection;

namespace RealTimeSystem.Application.Common.Mediator
{
    public interface IRequest<TResponse>;

    public interface IRequestHandler<in TRequest, TResponse>
        where TRequest : IRequest<TResponse>
    {
        Task<TResponse> Handle(TRequest request, CancellationToken cancellationToken);
    }

    public delegate Task<TResponse> RequestHandlerDelegate<TResponse>();

    public interface IPipelineBehavior<in TRequest, TResponse>
        where TRequest : IRequest<TResponse>
    {
        Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken);
    }

    public interface ISender
    {
        Task<TResponse> Send<TResponse>(IRequest<TResponse> request, CancellationToken cancellationToken = default);
    }

    public sealed class Mediator : ISender
    {
        private readonly IServiceProvider _serviceProvider;

        public Mediator(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public Task<TResponse> Send<TResponse>(IRequest<TResponse> request, CancellationToken cancellationToken = default)
        {
            var wrapperType = typeof(RequestHandlerWrapper<,>).MakeGenericType(request.GetType(), typeof(TResponse));
            var wrapper = (RequestHandlerBase<TResponse>)Activator.CreateInstance(wrapperType)!;
            return wrapper.Handle(request, _serviceProvider, cancellationToken);
        }

        private abstract class RequestHandlerBase<TResponse>
        {
            public abstract Task<TResponse> Handle(IRequest<TResponse> request, IServiceProvider provider, CancellationToken cancellationToken);
        }

        private sealed class RequestHandlerWrapper<TRequest, TResponse> : RequestHandlerBase<TResponse>
            where TRequest : IRequest<TResponse>
        {
            public override Task<TResponse> Handle(IRequest<TResponse> request, IServiceProvider provider, CancellationToken cancellationToken)
            {
                var typedRequest = (TRequest)request;
                var behaviors = provider.GetServices<IPipelineBehavior<TRequest, TResponse>>().Reverse().ToArray();

                RequestHandlerDelegate<TResponse> next = () =>
                {
                    var handler = provider.GetRequiredService<IRequestHandler<TRequest, TResponse>>();
                    return handler.Handle(typedRequest, cancellationToken);
                };
                foreach (var behavior in behaviors)
                {
                    var current = next;
                    var currentBehavior = behavior;
                    next = () => currentBehavior.Handle(typedRequest, current, cancellationToken);
                }

                return next();
            }
        }
    }
}


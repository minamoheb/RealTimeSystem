using Microsoft.Extensions.Logging;
using System.Diagnostics;

namespace RealTimeSystem.Application.Common.Behaviors
{
    public sealed class ValidationBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
        where TRequest : IRequest<TResponse>
    {
        private readonly IEnumerable<IValidator<TRequest>> _validators;

        public ValidationBehavior(IEnumerable<IValidator<TRequest>> validators)
        {
            _validators = validators;
        }

        public async Task<TResponse> Handle(
            TRequest request,
            RequestHandlerDelegate<TResponse> next,
            CancellationToken cancellationToken)
        {
            if (!_validators.Any())
            {
                return await next();
            }

            var context = new ValidationContext<TRequest>(request);
            var results = await Task.WhenAll(_validators.Select(validator => validator.ValidateAsync(context, cancellationToken)));
            var failures = results.SelectMany(result => result.Errors).Where(error => error is not null).ToList();
            if (failures.Count == 0)
            {
                return await next();
            }

            var errors = failures
                .GroupBy(failure => failure.PropertyName)
                .ToDictionary(group => group.Key, group => group.Select(failure => failure.ErrorMessage).Distinct().ToArray());

            throw new ValidationAppException(errors);
        }
    }

    public sealed class LoggingBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
        where TRequest : IRequest<TResponse>
    {
        private readonly ILogger<LoggingBehavior<TRequest, TResponse>> _logger;

        public LoggingBehavior(ILogger<LoggingBehavior<TRequest, TResponse>> logger)
        {
            _logger = logger;
        }

        public async Task<TResponse> Handle(
            TRequest request,
            RequestHandlerDelegate<TResponse> next,
            CancellationToken cancellationToken)
        {
            var name = typeof(TRequest).Name;
            var startDate = DateTime.UtcNow;
            var stopwatch = Stopwatch.StartNew();
            try
            {
                return await next();
            }
            finally
            {
                stopwatch.Stop();
                _logger.LogInformation(
                    "Handled {RequestName} in {ElapsedMilliseconds} ms. StartDate {StartDate:o} EndDate {EndDate:o}",
                    name,
                    stopwatch.ElapsedMilliseconds,
                    startDate,
                    DateTime.UtcNow);
            }
        }
    }
}


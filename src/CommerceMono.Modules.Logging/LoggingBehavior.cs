using System.Diagnostics;
using MediatR;
using Microsoft.Extensions.Logging;

namespace CommerceMono.Modules.Logging;

public class LoggingBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull, IRequest<TResponse>
    where TResponse : notnull
{
    private readonly ILogger<LoggingBehavior<TRequest, TResponse>> _logger;

    public LoggingBehavior(ILogger<LoggingBehavior<TRequest, TResponse>> logger)
    {
        _logger = logger;
    }

    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        const string prefix = nameof(LoggingBehavior<TRequest, TResponse>);
        var requestName = typeof(TRequest).Name;
        var responseName = typeof(TResponse).Name;

        _logger.LogInformation($"[{prefix}] Handle request={requestName} and response={responseName}");

        var timer = new Stopwatch();
        timer.Start();

        var response = await next();

        timer.Stop();
        var timeTaken = timer.Elapsed;
        if (timeTaken.Seconds > 3) // if the request is greater than 3 seconds, then log the warnings
            _logger.LogWarning($"[{prefix}] The request {requestName} took {timeTaken.Seconds} seconds.");

        _logger.LogInformation($"[{prefix}] Handled {requestName}");
        return response;
    }
}

using MediatR;
using Microsoft.Extensions.Logging;

namespace Smurfs.Application.Core;

/// <summary>
/// Logs unexpected application request failures before propagating them to the caller.
/// </summary>
public sealed class RequestFailureLoggingBehavior<TRequest, TResponse>(
    ILogger<RequestFailureLoggingBehavior<TRequest, TResponse>> logger)
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
{
    /// <inheritdoc />
    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        try
        {
            return await next(cancellationToken);
        }
        catch (Exception exception) when (exception is not OperationCanceledException)
        {
            logger.LogError(exception, "Application request {RequestType} failed.", typeof(TRequest).FullName);
            throw;
        }
    }
}

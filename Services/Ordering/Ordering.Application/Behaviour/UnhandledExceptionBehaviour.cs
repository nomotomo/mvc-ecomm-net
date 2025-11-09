using MediatR;
using Microsoft.Extensions.Logging;

namespace Ordering.Application.Behaviour;

public class UnhandledExceptionBehaviour<TRequest, TResponse>(ILogger<TRequest> logger) :
    IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        try
        {
            return await next(cancellationToken);
        }
        catch (Exception ex)
        {
            var requestName = typeof(TRequest).Name;
            logger.LogError(ex, "Unhandled Exception for Request {RequestName} {@Request}", requestName, request);
            throw;
        }
    }
}
using FluentValidation;
using MediatR;

namespace Ordering.Application.Behaviour;

public class ValidationBehaviour<TRequest, TResponse>(IEnumerable<IValidator<TRequest>> validators) :
    IPipelineBehavior<TRequest, TResponse>
    where TRequest :
    IRequest<TResponse>
{
    // this method will be called before the request reaches the handler
    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        if (!validators.Any()) return await next(cancellationToken);
        var context = new ValidationContext<TRequest>(request);
        // this will run all the validation rules one by one and returns the validation results
        var validationResults = await Task.WhenAll(
            validators.Select(v => 
                v.ValidateAsync(context, cancellationToken)));
        var failures = validationResults.SelectMany(result => 
            result.Errors).Where(failure => failure != null).ToList();

        if (failures.Count != 0)
        { 
            throw new ValidationException(failures);
        }
        return await next(cancellationToken);
    }
}
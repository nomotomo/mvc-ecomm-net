using FluentValidation.Results;

namespace Ordering.Application.Exceptions;

public class ValidationException : ApplicationException
{
    public Dictionary<string, string> Errors { get; set; }
    public ValidationException() : base("One or more validation error occurred")
    {
        Errors = new Dictionary<string, string>();
    }

    public ValidationException(IEnumerable<ValidationFailure> failures) : this()
    {
        Errors = failures
            .GroupBy(e => e.PropertyName, e => e.ErrorMessage)
            .ToDictionary(failureGroup => failureGroup.Key, failureGroup => string.Join(", ", failureGroup));
    }
}
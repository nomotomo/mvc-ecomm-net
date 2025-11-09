using FluentValidation;
using Ordering.Application.Commands;

namespace Ordering.Application.Validators;

public class UpdateOrderCommandValidator : AbstractValidator<UpdateOrderCommand>
{
    public UpdateOrderCommandValidator()
    {
        RuleFor(o => o.UserName)
            .NotEmpty().WithMessage("{UserName} is required.")
            .NotNull()
            .MaximumLength(50).WithMessage("{UserName} must not exceed 50 characters.");

        RuleFor(o => o.TotalPrice)
            .NotEmpty()
            .WithMessage("{TotalPrice} is required.")
            .NotNull()
            .GreaterThan(-1)
            .WithMessage("{TotalPrice} must be greater than or equal to 0.");
        RuleFor(o => o.EmailAddress)
            .NotEmpty()
            .WithMessage("Email is required.")
            .NotNull();
        RuleFor(o => o.FirstName)
            .NotEmpty()
            .WithMessage("{FirstName} is required.")
            .NotNull();
        RuleFor(o => o.LastName)
            .NotEmpty()
            .WithMessage("{LastName} is required.")
            .NotNull();
        RuleFor(o => o.AddressLine)
            .NotEmpty()
            .WithMessage("{AddressLine} is required.")
            .NotNull();
        RuleFor(o => o.Id)
            .NotEmpty()
            .WithMessage("{Id} is required.")
            .NotNull()
            .GreaterThan(Guid.Empty)
            .WithMessage("{Id} cannot be an empty GUID.");
    }
}
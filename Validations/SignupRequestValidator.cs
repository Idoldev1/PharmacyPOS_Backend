using FluentValidation;
using POS.API.Models;

namespace POS.API.Validations;

public class SignupRequestValidator : AbstractValidator<SignupRequest>
{
    public SignupRequestValidator()
    {
        RuleFor(x => x.Username)
            .NotEmpty().WithMessage("Username is required.")
            .Length(3, 20).WithMessage("Username must be between 3 and 20 characters.")
            .Matches(@"^[a-zA-Z0-9_-]+$").WithMessage("Username can only contain alphanumeric characters, hyphens, and underscores.");

        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("Password is required.")
            .Length(6, 20).WithMessage("Password must be between 6 and 20 characters.");

        RuleFor(x => x.FirstName)
            .NotEmpty().WithMessage("First name is required.")
            .Length(2, 50).WithMessage("First name must be between 2 and 50 characters.");

        RuleFor(x => x.LastName)
            .NotEmpty().WithMessage("Last name is required.")
            .Length(2, 50).WithMessage("Last name must be between 2 and 50 characters.");

        RuleFor(x => x.Role)
            .NotEmpty().WithMessage("Role is required.")
            .Must(x => new[] { "admin", "pharmacist", "cashier", "manager" }.Contains(x.ToLower()))
            .WithMessage("Role must be one of: admin, pharmacist, cashier, manager.");

        RuleFor(x => x.BranchId)
            .NotEmpty().WithMessage("Branch ID is required.")
            .Length(1, 50).WithMessage("Branch ID must be between 1 and 50 characters.");
    }
}

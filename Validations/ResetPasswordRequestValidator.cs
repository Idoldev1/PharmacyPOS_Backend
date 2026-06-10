using FluentValidation;
using POS.API.Models;

namespace POS.API.Validations;

public class ResetPasswordRequestValidator : AbstractValidator<ResetPasswordRequest>
{
    public ResetPasswordRequestValidator()
    {
        RuleFor(x => x.Token)
            .NotEmpty().WithMessage("Reset token is required.")
            .Length(10, 500).WithMessage("Reset token appears to be invalid.");

        RuleFor(x => x.NewPassword)
            .NotEmpty().WithMessage("New password is required.")
            .Length(6, 100).WithMessage("New password must be between 6 and 100 characters.");
    }
}

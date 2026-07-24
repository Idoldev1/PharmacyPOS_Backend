using FluentValidation;
using POS.API.Models;

namespace POS.API.Validations;

public class ResetStaffPasswordRequestValidator : AbstractValidator<ResetStaffPasswordRequest>
{
    public ResetStaffPasswordRequestValidator()
    {
        RuleFor(x => x.NewPassword)
            .NotEmpty().WithMessage("New password is required.")
            .Length(6, 20).WithMessage("New password must be between 6 and 20 characters.");
    }
}

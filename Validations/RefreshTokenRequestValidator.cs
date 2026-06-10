using FluentValidation;
using POS.API.Models;

namespace POS.API.Validations;

public class RefreshTokenRequestValidator : AbstractValidator<RefreshTokenRequest>
{
    public RefreshTokenRequestValidator()
    {
        RuleFor(x => x.Token)
            .NotEmpty().WithMessage("Refresh token is required.")
            .Length(10, 500).WithMessage("Refresh token appears to be invalid.");
    }
}

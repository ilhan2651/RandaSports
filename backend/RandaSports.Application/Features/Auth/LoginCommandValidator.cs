using FluentValidation;

namespace RandaSports.Application.Features.Auth;

public sealed class LoginCommandValidator : AbstractValidator<LoginCommand>
{
    public LoginCommandValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("E-posta gerekli.")
            .MaximumLength(256).WithMessage("E-posta en fazla 256 karakter olabilir.");

        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("Parola gerekli.")
            .MaximumLength(200).WithMessage("Parola en fazla 200 karakter olabilir.");
    }
}

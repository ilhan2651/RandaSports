using FluentValidation;
using Microsoft.Extensions.Options;
using RandaSports.Application.Common.Options;

namespace RandaSports.Application.Features.Auth;

public sealed class RegisterCommandValidator : AbstractValidator<RegisterCommand>
{
    public RegisterCommandValidator(IOptions<AuthPolicyOptions> options)
    {
        var policy = options.Value;

        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("E-posta gerekli.")
            .EmailAddress().WithMessage("Geçerli bir e-posta adresi gir.")
            .MaximumLength(256).WithMessage("E-posta en fazla 256 karakter olabilir.");

        // Karmaşıklık kuralı koymuyoruz: uzunluk, işaret zorunluluğundan daha
        // koruyucu ve kullanıcıyı "Parola1!" gibi tahmin edilebilir kalıplara itmiyor.
        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("Parola gerekli.")
            .MinimumLength(policy.MinPasswordLength)
            .WithMessage($"Parola en az {policy.MinPasswordLength} karakter olmalı.")
            .MaximumLength(200).WithMessage("Parola en fazla 200 karakter olabilir.");

        RuleFor(x => x.FullName)
            .MaximumLength(150).WithMessage("Ad en fazla 150 karakter olabilir.");
    }
}

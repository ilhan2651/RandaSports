using FluentValidation;

namespace RandaSports.Application.Features.Sports.Command.UpdateTeamLogo;

public sealed class UpdateTeamLogoCommandValidator : AbstractValidator<UpdateTeamLogoCommand>
{
    public UpdateTeamLogoCommandValidator()
    {
        RuleFor(x => x.LogoUrl)
            .MaximumLength(500).WithMessage("Logo adresi en fazla 500 karakter olabilir.")
            .Must(BeWebAddress).WithMessage("Logo adresi http veya https ile başlamalı.")
            .When(x => !string.IsNullOrWhiteSpace(x.LogoUrl));
    }

    private static bool BeWebAddress(string? value) =>
        Uri.TryCreate(value, UriKind.Absolute, out var uri) && uri.Scheme is "http" or "https";
}

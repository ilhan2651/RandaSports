using FluentValidation;

namespace RandaSports.Application.Features.Commentary.Command.UpdateCommentator;

public sealed class UpdateCommentatorCommandValidator : AbstractValidator<UpdateCommentatorCommand>
{
    public UpdateCommentatorCommandValidator()
    {
        RuleFor(x => x.PhotoUrl)
            .MaximumLength(500).WithMessage("Görsel adresi en fazla 500 karakter olabilir.")
            .Must(BeWebAddress).WithMessage("Görsel adresi http veya https ile başlamalı.")
            .When(x => !string.IsNullOrWhiteSpace(x.PhotoUrl));

        RuleFor(x => x.Bio)
            .MaximumLength(1000).WithMessage("Biyografi en fazla 1000 karakter olabilir.");
    }

    private static bool BeWebAddress(string? value) =>
        Uri.TryCreate(value, UriKind.Absolute, out var uri)
        && uri.Scheme is "http" or "https";
}

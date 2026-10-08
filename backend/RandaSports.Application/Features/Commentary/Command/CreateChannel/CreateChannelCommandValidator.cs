using FluentValidation;

namespace RandaSports.Application.Features.Commentary.Command.CreateChannel;

public sealed class CreateChannelCommandValidator : AbstractValidator<CreateChannelCommand>
{
    public CreateChannelCommandValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Kanal adı gerekli.")
            .MaximumLength(150).WithMessage("Kanal adı en fazla 150 karakter olabilir.");

        RuleFor(x => x.Reference)
            .NotEmpty().WithMessage("Kanal adresi veya kullanıcı adı gerekli.")
            .MaximumLength(200).WithMessage("Kanal adresi en fazla 200 karakter olabilir.");
    }
}

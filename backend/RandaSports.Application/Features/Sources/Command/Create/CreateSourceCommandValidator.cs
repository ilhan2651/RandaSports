using FluentValidation;

namespace RandaSports.Application.Features.Sources.Command.Create;

public sealed class CreateSourceCommandValidator : AbstractValidator<CreateSourceCommand>
{
    public CreateSourceCommandValidator()
    {
        RuleFor(x => x.Name).ValidSourceName();
        RuleFor(x => x.Url).ValidSourceUrl();
        RuleFor(x => x.Language).ValidLanguage();
        RuleFor(x => x.FetchIntervalMinutes).ValidFetchInterval();
        RuleFor(x => x.Type).IsInEnum().WithMessage("Geçersiz kaynak tipi.");
    }
}

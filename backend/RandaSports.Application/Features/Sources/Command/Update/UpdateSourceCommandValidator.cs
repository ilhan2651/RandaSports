using FluentValidation;

namespace RandaSports.Application.Features.Sources.Command.Update;

public sealed class UpdateSourceCommandValidator : AbstractValidator<UpdateSourceCommand>
{
    public UpdateSourceCommandValidator()
    {
        RuleFor(x => x.Name).ValidSourceName();
        RuleFor(x => x.Url).ValidSourceUrl();
        RuleFor(x => x.Language).ValidLanguage();
        RuleFor(x => x.FetchIntervalMinutes).ValidFetchInterval();
        RuleFor(x => x.Type).IsInEnum().WithMessage("Geçersiz kaynak tipi.");
    }
}

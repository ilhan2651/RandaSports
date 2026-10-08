using FluentValidation;

namespace RandaSports.Application.Features.Sources;

internal static class SourceValidationRules
{
    public static IRuleBuilderOptions<T, string> ValidSourceName<T>(this IRuleBuilder<T, string> rule) =>
        rule.NotEmpty().WithMessage("Kaynak adı boş olamaz.")
            .MaximumLength(150).WithMessage("Kaynak adı en fazla 150 karakter olabilir.");

    public static IRuleBuilderOptions<T, string> ValidSourceUrl<T>(this IRuleBuilder<T, string> rule) =>
        rule.NotEmpty().WithMessage("URL boş olamaz.")
            .MaximumLength(500).WithMessage("URL en fazla 500 karakter olabilir.")
            .Must(BeHttpUrl).WithMessage("Geçerli bir http/https adresi girin.");

    public static IRuleBuilderOptions<T, string> ValidLanguage<T>(this IRuleBuilder<T, string> rule) =>
        rule.NotEmpty().WithMessage("Dil boş olamaz.")
            .Length(2, 5).WithMessage("Dil kodu 2-5 karakter olmalı (örn. tr, en).");

    public static IRuleBuilderOptions<T, int> ValidFetchInterval<T>(this IRuleBuilder<T, int> rule) =>
        rule.InclusiveBetween(5, 1440).WithMessage("Çekme aralığı 5 ile 1440 dakika arasında olmalı.");

    private static bool BeHttpUrl(string url) =>
        Uri.TryCreate(url, UriKind.Absolute, out var uri)
        && (uri.Scheme == Uri.UriSchemeHttp || uri.Scheme == Uri.UriSchemeHttps);
}

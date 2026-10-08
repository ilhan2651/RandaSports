namespace RandaSports.Application.Common.Options;

/// <summary>
/// Giriş politikası. JWT ayarlarından ayrı duruyor çünkü bunlar uygulama kuralı,
/// jeton biçimiyle ilgisi yok — ve Application katmanı Infrastructure'a bakamaz.
/// </summary>
public sealed class AuthPolicyOptions
{
    public const string SectionName = "AuthPolicy";

    /// <summary>Art arda bu kadar hatalı girişten sonra hesap geçici kilitleniyor.</summary>
    public int MaxFailedAttempts { get; set; } = 5;

    public int LockoutMinutes { get; set; } = 15;

    public int MinPasswordLength { get; set; } = 8;
}

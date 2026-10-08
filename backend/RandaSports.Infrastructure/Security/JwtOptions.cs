namespace RandaSports.Infrastructure.Security;

public sealed class JwtOptions
{
    public const string SectionName = "Jwt";

    /// <summary>
    /// İmza anahtarı. En az 32 karakter olmalı (HMAC-SHA256 anahtar boyu);
    /// kısa anahtar imzayı tahmin edilebilir kılıyor. Depoya girmiyor.
    /// </summary>
    public string Key { get; set; } = string.Empty;

    public string Issuer { get; set; } = "RandaSports";

    public string Audience { get; set; } = "RandaSports";

    /// <summary>
    /// Erişim jetonunun ömrü. Kısa tutuluyor: jeton iptal edilemediği için, rolü
    /// alınan ya da kapatılan bir hesabın elindeki jetonun en fazla bu kadar
    /// yaşaması gerekiyor. Kullanıcı bunu hissetmiyor, yenileme jetonu devralıyor.
    /// </summary>
    public int LifetimeMinutes { get; set; } = 60;

    /// <summary>Yenileme jetonunun ömrü; okuyucunun her gün giriş yapmaması için uzun.</summary>
    public int RefreshLifetimeDays { get; set; } = 30;

}

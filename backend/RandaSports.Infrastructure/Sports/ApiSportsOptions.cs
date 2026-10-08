namespace RandaSports.Infrastructure.Sports;

/// <summary>
/// api-sports.io hesabının ortak ayarları. Anahtar tüm sporlarda aynı;
/// her sporun kendi adresi, kendi günlük kotası ve kendi istemcisi var.
/// </summary>
public sealed class ApiSportsOptions
{
    public const string SectionName = "ApiSports";

    public string ApiKey { get; set; } = string.Empty;

    public bool HasKey => !string.IsNullOrWhiteSpace(ApiKey);
}

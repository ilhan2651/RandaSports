namespace RandaSports.Persistence.Seeders;

/// <summary>
/// İlk yönetici hesabı. Yalnızca hiç kullanıcı yokken kullanılıyor; sonrasında
/// parola buradan değiştirilemiyor, yani ayar dosyasında unutulsa bile mevcut
/// hesabın parolasını geri almıyor.
/// </summary>
public sealed class AdminSeedOptions
{
    public const string SectionName = "AdminSeed";

    public string Email { get; set; } = string.Empty;

    public string Password { get; set; } = string.Empty;

    public string? FullName { get; set; }
}

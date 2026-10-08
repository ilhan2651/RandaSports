using RandaSports.Domain.Common;

namespace RandaSports.Domain.Entities;

/// <summary>
/// Siteye giren herkes: hem okuyucu hem yönetici. Rol <see cref="UserRoles"/>
/// üzerinden veriliyor, kullanıcı tipi diye ayrı bir tablo tutulmuyor — aynı kişi
/// hem okur hem yönetir, ikinci bir hesap açmak gerekmiyor.
/// </summary>
public class User : BaseEntity
{
    /// <summary>Kullanıcının yazdığı hâli; ekranda bunu gösteriyoruz.</summary>
    public required string Email { get; set; }

    /// <summary>
    /// Arama ve benzersizlik bunun üzerinden. Postgres'te büyük/küçük harf duyarlı
    /// karşılaştırma yapıldığı için "Ali@x.com" ile "ali@x.com" aynı kişi olsun diye
    /// ayrı bir sütunda normalleştirilmiş hâli duruyor.
    /// </summary>
    public required string NormalizedEmail { get; set; }

    public string? FullName { get; set; }

    /// <summary>PBKDF2 ile türetilmiş özet. Parolanın kendisi hiçbir yerde durmuyor.</summary>
    public required string PasswordHash { get; set; }

    /// <summary>Kapatılan hesap parolası doğru olsa bile giremiyor.</summary>
    public bool IsActive { get; set; } = true;

    /// <summary>E-posta doğrulaması henüz kurulmadı; alan şimdiden duruyor.</summary>
    public bool IsEmailConfirmed { get; set; }

    /// <summary>
    /// Parola ya da rol değişince yenilenen damga. Elde duran jetonları topluca
    /// geçersiz kılmanın tek yolu bu: jetonun içine yazılıyor, doğrulamada
    /// kullanıcının güncel damgasıyla karşılaştırılıyor.
    /// </summary>
    public string SecurityStamp { get; set; } = Guid.CreateVersion7().ToString("N");

    public DateTimeOffset? LastLoginAt { get; set; }

    /// <summary>
    /// Takip listesi sihirbazı tamamlandı mı. "Üç liste de boş" ölçütüyle karar
    /// veremiyoruz: kullanıcı bilerek hiçbir şey seçmemiş olabilir ve her girişte
    /// aynı modalla karşılaşır.
    /// </summary>
    public DateTimeOffset? OnboardingCompletedAt { get; set; }

    /// <summary>
    /// Art arda hatalı giriş sayacı. Doğru girişte sıfırlanıyor; eşiği aşınca
    /// <see cref="LockoutEnd"/> doluyor ve hesap bir süre kilitleniyor.
    /// Parola deneme saldırısını yavaşlatan en ucuz önlem.
    /// </summary>
    public int FailedLoginAttempts { get; set; }

    public DateTimeOffset? LockoutEnd { get; set; }

    public ICollection<UserRole> UserRoles { get; set; } = [];
    public ICollection<RefreshToken> RefreshTokens { get; set; } = [];

    // Günlük bültenin dayanağı: kullanıcı neyi takip ediyorsa bülten ondan derleniyor.
    public ICollection<Team> FollowedTeams { get; set; } = [];
    public ICollection<Sport> FollowedSports { get; set; } = [];
    public ICollection<Commentator> FollowedCommentators { get; set; } = [];
}

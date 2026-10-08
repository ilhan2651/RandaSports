using RandaSports.Domain.Common;

namespace RandaSports.Domain.Entities;

/// <summary>
/// Erişim jetonu kısa ömürlü; okuyucunun her saat yeniden giriş yapmaması için
/// yenileme jetonu tutuluyor. Jetonun kendisi değil özeti saklanıyor: veritabanı
/// sızsa bile eldeki satırlarla oturum açılamıyor.
/// </summary>
public class RefreshToken : BaseEntity
{
    public Guid UserId { get; set; }
    public User User { get; set; } = null!;

    public required string TokenHash { get; set; }

    public DateTimeOffset ExpiresAt { get; set; }

    /// <summary>Dolduysa jeton artık geçersiz: çıkış yapıldı ya da yenisiyle değiştirildi.</summary>
    public DateTimeOffset? RevokedAt { get; set; }

    /// <summary>Şüpheli oturumu ayırt edebilmek için; kişisel veri olduğundan kısa tutuluyor.</summary>
    public string? CreatedIp { get; set; }

    public bool IsActive(DateTimeOffset now) => RevokedAt is null && ExpiresAt > now;
}

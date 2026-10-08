namespace RandaSports.Domain.Enums;

/// <summary>
/// Sözlükteki kişinin ne olduğu. Ayrım şu yüzden önemli: bir teknik direktörün
/// basın toplantısındaki sözü yanlış atfedilmiş değildir — adam gerçekten onu
/// söylemiştir — ama o bir yorum değil, demeçtir. Tekrar sayısı bu ayrımı
/// göremiyor: haftada basın toplantısı yapan bir teknik direktör, en çalışkan
/// yorumcudan bile sık görünüyor.
/// </summary>
public enum PersonRole
{
    /// <summary>Henüz sınıflandırılmadı.</summary>
    Unknown = 0,

    /// <summary>Yorumcu, spiker, sunucu, spor yazarı — mesleği konuşmak olan kişi.</summary>
    Commentator = 1,

    /// <summary>Aktif ya da emekli sporcu. (Emekli futbolcu yorumculuk yapıyorsa Commentator.)</summary>
    Athlete = 2,

    /// <summary>Teknik direktör, antrenör, yardımcı antrenör.</summary>
    Coach = 3,

    /// <summary>Kulüp başkanı, yönetici, federasyon yetkilisi, hakem, menajer.</summary>
    Official = 4
}

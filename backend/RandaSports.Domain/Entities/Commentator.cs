using RandaSports.Domain.Common;
using RandaSports.Domain.Enums;

namespace RandaSports.Domain.Entities;

public class Commentator : BaseEntity
{
    public required string FullName { get; set; }
    public required string Slug { get; set; }
    public List<string> Aliases { get; set; } = [];
    public string? Bio { get; set; }
    public string? PhotoUrl { get; set; }
    public bool IsActive { get; set; } = true;

    /// <summary>
    /// Bu kişinin gerçekten yorumcu olduğunu insan doğruladı mı. Seed'den gelenler ve
    /// onay ekranından bağlananlar doğrulanmış sayılıyor; videodan otomatik eklenenler
    /// doğrulanana kadar false kalıyor.
    ///
    /// Önemli: kanal kadrosu güveni yalnızca doğrulanmış isimlere veriliyor. Başlıktan
    /// otomatik eklenen bir isim (ki başlıktaki isim çoğu zaman haberin KONUSUDUR,
    /// konuşan değil) yanlışsa, o yanlış sonraki videolarda güven yükseltmiyor.
    /// </summary>
    public bool IsVerified { get; set; }

    /// <summary>
    /// Yorumcu mu, sporcu mu, teknik direktör mü. Tekrar sayısı bunu ölçemiyor:
    /// her hafta basın toplantısı yapan bir teknik direktör en çalışkan yorumcudan
    /// bile sık görünüyor. Bu yüzden rol ayrıca modele soruluyor.
    /// </summary>
    public PersonRole PersonRole { get; set; } = PersonRole.Unknown;

    /// <summary>Rol kararının güveni; düşükse doğrulama kendiliğinden yapılmıyor.</summary>
    public double? RoleConfidence { get; set; }

    /// <summary>Modelin gerekçesi — kararı sonradan denetleyebilmek için.</summary>
    public string? RoleNote { get; set; }

    public ICollection<Sport> Sports { get; set; } = [];
    public ICollection<Channel> Channels { get; set; } = [];
    public ICollection<Opinion> Opinions { get; set; } = [];
}

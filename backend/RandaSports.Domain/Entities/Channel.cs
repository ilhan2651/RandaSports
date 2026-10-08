using RandaSports.Domain.Common;

namespace RandaSports.Domain.Entities;

public class Channel : BaseEntity
{
    public required string Name { get; set; }
    public required string Slug { get; set; }

    /// <summary>YouTube kullanıcı adı: "@sporx". Kanal kimliği bundan çözülüyor.</summary>
    public required string Handle { get; set; }

    /// <summary>UC... ile başlayan kanal kimliği; RSS beslemesi bunu istiyor.</summary>
    public string? YouTubeChannelId { get; set; }

    /// <summary>
    /// Kanalın kapsadığı branşlar. Tek branş yetmiyor: HTalks hem NFL hem futbol,
    /// 8GEN hem MMA hem boks yapıyor. Bu liste görüşün branşını BELİRLEMİYOR —
    /// modele "bu kanal şunları yapıyor" diye verilen bir ipucu, ve tek branş
    /// varsa son çare yedek. Asıl kararı videoyu izleyen model veriyor.
    /// Genel spor kanallarında (A Spor, beIN) boş bırakılıyor.
    /// </summary>
    public ICollection<Sport> Sports { get; set; } = [];

    public bool IsActive { get; set; } = true;
    public DateTimeOffset? LastCheckedAt { get; set; }
    public string? LastError { get; set; }

    public ICollection<Commentator> RegularCommentators { get; set; } = [];
    public ICollection<Video> Videos { get; set; } = [];
}

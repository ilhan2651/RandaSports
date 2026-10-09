using RandaSports.Domain.Entities;

namespace RandaSports.Application.Interfaces.Repositories;

/// <param name="OpinionCount">Yalnızca onaylanmış görüşler sayılıyor.</param>
/// <param name="SportSlugs">
/// Yorumcunun branşları, GÖRÜŞLERİNDEN türetiliyor — kayıttaki statik alandan
/// değil. Sebep: o alan yalnızca tohum listesindekilerde dolu ve orada da
/// sadece futbol/basketbol var; videodan otomatik eklenenlerde hiç yok.
/// Görüşlerden türetince kendiliğinden güncel kalıyor.
/// </param>
public sealed record CommentatorProfile(
    Commentator Commentator,
    int OpinionCount,
    DateTimeOffset? LastOpinionAt,
    List<string> SportSlugs);

/// <param name="Sample">Karar verirken bakılacak örnek görüşler.</param>
public sealed record UnverifiedCommentator(
    Commentator Commentator,
    int OpinionCount,
    int ApprovedCount,
    DateTimeOffset? LastOpinionAt,
    List<string> ChannelNames,
    List<OpinionSample> Sample);

public sealed record OpinionSample(
    string Topic,
    string Quote,
    string VideoTitle,
    string YouTubeVideoId,
    int? TimestampSeconds);

/// <summary>
/// Doğrulanmamış bir ismin arkasındaki kanıt. İnsana sormadan karar verebilmek için
/// sayıyoruz: yayıncının ekrana bastığı isim (altbant) en güçlü kanıt, farklı
/// kanallarda tekrar etmesi ikinci güçlü kanıt.
/// </summary>
/// <summary>Rol tespiti için modele gönderilecek bağlam.</summary>
public sealed record RoleCandidate(
    Guid Id,
    string FullName,
    List<string> VideoTitles,
    List<string> Quotes,
    List<string> Channels);

public sealed record CommentatorEvidence(
    Guid Id,
    string FullName,
    int DistinctVideos,
    int DistinctChannels,
    int AltbantVideos);

public interface ICommentatorRepository : IGenericRepository<Commentator>
{
    /// <summary>
    /// Videodan otomatik eklenip henüz insan onayından geçmemiş isimler. Yanlış
    /// eklenmiş biri — başlıkta adı geçen bir futbolcu gibi — burada görünüp temizleniyor.
    /// </summary>
    Task<List<UnverifiedCommentator>> GetUnverifiedAsync(CancellationToken cancellationToken = default);

    /// <summary>Doğrulanmamış isimlerin kanıt sayıları; otomatik doğrulama bunu kullanıyor.</summary>
    Task<List<CommentatorEvidence>> GetVerificationEvidenceAsync(
        CancellationToken cancellationToken = default);

    /// <summary>Rolü henüz belirlenmemiş kişiler, modele verilecek bağlamla birlikte.</summary>
    Task<List<RoleCandidate>> GetRoleCandidatesAsync(
        int take,
        CancellationToken cancellationToken = default);

    /// <summary>Görseli olmayan, doğrulanmış yorumcular — portre aramak için.</summary>
    Task<List<Commentator>> GetVerifiedWithoutPhotoAsync(
        int take,
        CancellationToken cancellationToken = default);

    /// <summary>Onay/ret için görüşleri ve kanallarıyla birlikte, takip edilen hâlde.</summary>
    Task<Commentator?> GetForReviewAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>Konuşmacı doğrulamasının sözlüğü.</summary>
    Task<List<Commentator>> GetActiveAsync(CancellationToken cancellationToken = default);

    /// <summary>Takip listesi kaydedilirken seçilen kimliklerin gerçekliğini doğruluyor.</summary>
    Task<List<Commentator>> GetByIdsAsync(List<Guid> ids, CancellationToken cancellationToken = default);

    /// <summary>Takip edilen hâlde döner: bulunan yorumcu kanala da bağlanabiliyor.</summary>
    Task<Commentator?> GetBySlugAsync(string slug, CancellationToken cancellationToken = default);

    /// <summary>
    /// Yorumcular sayfası için: görüşü yayına çıkmış yorumcular, görüş sayılarıyla.
    /// Hiç görüşü olmayanlar listede görünmüyor — boş profil göstermenin anlamı yok.
    /// </summary>
    Task<List<CommentatorProfile>> GetProfilesAsync(CancellationToken cancellationToken = default);

    Task<CommentatorProfile?> GetProfileBySlugAsync(string slug, CancellationToken cancellationToken = default);
}

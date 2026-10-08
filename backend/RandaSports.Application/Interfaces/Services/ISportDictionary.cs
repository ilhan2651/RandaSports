namespace RandaSports.Application.Interfaces.Services;

/// <summary>
/// Branş listesi her haberde iki yerde okunuyor: modele seçenek olarak veriliyor ve
/// modelin döndürdüğü slug doğrulanıyor. Liste neredeyse hiç değişmediği için
/// önbellekten servis ediliyor.
/// </summary>
public interface ISportDictionary
{
    Task<List<SportEntry>> GetAllAsync(CancellationToken cancellationToken = default);

    /// <summary>Slug kayıtlı bir branşa denk gelmiyorsa null döner — model uydurmuşsa kabul etmiyoruz.</summary>
    Task<SportEntry?> FindAsync(string? slug, CancellationToken cancellationToken = default);
}

public sealed record SportEntry(Guid Id, string Name, string Slug);

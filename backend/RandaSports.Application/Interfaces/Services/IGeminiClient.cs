namespace RandaSports.Application.Interfaces.Services;

public interface IGeminiClient
{
    /// <summary>Verilen istemi gönderir ve modelin döndürdüğü ham JSON metnini verir.</summary>
    Task<string?> GenerateJsonAsync(string prompt, CancellationToken cancellationToken = default);

    /// <summary>
    /// İstemi bir YouTube videosuyla birlikte gönderir. Video indirilmiyor;
    /// bağlantı doğrudan modele veriliyor.
    /// </summary>
    Task<string?> GenerateJsonFromVideoAsync(
        string prompt,
        string videoUrl,
        CancellationToken cancellationToken = default);
}

namespace RandaSports.Application.Interfaces.Services;

public interface IGeminiClient
{
    /// <summary>Verilen istemi gönderir ve modelin döndürdüğü ham JSON metnini verir.</summary>
    Task<string?> GenerateJsonAsync(string prompt, CancellationToken cancellationToken = default);

    /// <summary>
    /// İstemi bir YouTube videosuyla birlikte gönderir. Video indirilmiyor;
    /// bağlantı doğrudan modele veriliyor.
    /// </summary>
    /// <param name="startSeconds">
    /// Verilirse model videonun yalnızca bu aralığını görüyor. Uzun yayınları
    /// dilimlemek için: model 40 dakikayı değil 8 dakikayı takip ettiğinde zaman
    /// damgaları belirgin biçimde isabetli oluyor. Kırpma orantılı olduğu için
    /// dilimlemenin toplam token maliyeti videoyu bir kez işlemekle hemen hemen aynı.
    /// </param>
    Task<string?> GenerateJsonFromVideoAsync(
        string prompt,
        string videoUrl,
        int? startSeconds = null,
        int? endSeconds = null,
        CancellationToken cancellationToken = default);
}

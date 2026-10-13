namespace RandaSports.Application.Interfaces.Services;

/// <summary>
/// Video analizi Gemini'ye özgü: video indirilmiyor, bağlantı doğrudan modele
/// veriliyor ve yalnızca istenen zaman aralığı izletiliyor. Standart sohbet
/// soyutlaması (IChatClient) bunu kapsamadığı için ayrı duruyor.
/// </summary>
public interface IGeminiVideoClient
{
    /// <param name="startSeconds">
    /// Verilirse model videonun yalnızca bu aralığını görüyor. Uzun yayınları
    /// dilimlemek için: model 40 dakikayı değil 8 dakikayı takip ettiğinde zaman
    /// damgaları belirgin biçimde isabetli oluyor. Kırpma orantılı olduğu için
    /// dilimlemenin toplam token maliyeti videoyu bir kez işlemekle aynı.
    /// </param>
    Task<GeminiResult> GenerateJsonFromVideoAsync(
        string prompt,
        string videoUrl,
        int? startSeconds = null,
        int? endSeconds = null,
        CancellationToken cancellationToken = default);
}

using System.Text.Json.Serialization;

namespace RandaSports.Application.Features.Commentary.Command.ExtractVideoOpinions;

public sealed record AiVideoAnalysis
{
    [JsonPropertyName("videoSummary")]
    public string? VideoSummary { get; init; }

    [JsonPropertyName("opinions")]
    public List<AiVideoOpinion> Opinions { get; init; } = [];
}

public sealed record AiVideoOpinion
{
    /// <summary>Modelin duyduğu konuşmacı adı. Sözlükte yoksa görüş isme bağlanmıyor.</summary>
    [JsonPropertyName("speaker")]
    public string? Speaker { get; init; }

    [JsonPropertyName("topic")]
    public string? Topic { get; init; }

    [JsonPropertyName("summary")]
    public string? Summary { get; init; }

    /// <summary>Konuşmacının birebir sözleri.</summary>
    [JsonPropertyName("quote")]
    public string? Quote { get; init; }

    /// <summary>Videodaki an: saniye ("738") veya "12:18" biçiminde gelebiliyor.</summary>
    [JsonPropertyName("timestamp")]
    public string? Timestamp { get; init; }

    /// <summary>"olumlu" | "olumsuz" | "notr"</summary>
    [JsonPropertyName("stance")]
    public string? Stance { get; init; }

    [JsonPropertyName("prediction")]
    public string? Prediction { get; init; }

    /// <summary>
    /// Görüşün hangi branşa ait olduğu, bizim listemizdeki adres (slug) olarak.
    /// Takımdan türetmek güvenilmez: Fenerbahçe'nin hem futbol hem basketbol takımı
    /// var ama kayıtta tek branş duruyor. Videoyu izleyen model daha iyi biliyor.
    /// </summary>
    [JsonPropertyName("sport")]
    public string? Sport { get; init; }

    /// <summary>Görüşün konusu olan takım/oyuncu adları.</summary>
    [JsonPropertyName("subjects")]
    public List<string> Subjects { get; init; } = [];

    /// <summary>
    /// "gorus" | "haber" | "soru". Yalnızca "gorus" yayına alınıyor: muhabirin olay
    /// aktarımı ve sunucunun sorusu, konuşmacının görüşü değil.
    /// </summary>
    [JsonPropertyName("kind")]
    public string? Kind { get; init; }

    /// <summary>Modelin konuşmacıyı tanıma güveni (0-1).</summary>
    [JsonPropertyName("speakerConfidence")]
    public double? SpeakerConfidence { get; init; }

    /// <summary>
    /// Görüşün ne kadar çarpıcı olduğu (0-1), modelin kendi değerlendirmesi.
    /// Uzun video dilimlere bölündüğünde her dilim kendi görüşlerini döndürüyor;
    /// hepsini yayına almak akışı dolduruyor. Bu puan, atıf güveniyle çarpılarak
    /// video başına en iyi birkaçını seçmekte kullanılıyor.
    /// </summary>
    [JsonPropertyName("onem")]
    public double? Importance { get; init; }

    /// <summary>
    /// İsmi nereden buldu: "altbant" | "baslik" | "hitap" | "aciklama" | "tahmin".
    /// Yüze bakıp tanıdığını iddia etmesi "tahmin" sayılıyor.
    /// </summary>
    [JsonPropertyName("speakerSource")]
    public string? SpeakerSource { get; init; }
}

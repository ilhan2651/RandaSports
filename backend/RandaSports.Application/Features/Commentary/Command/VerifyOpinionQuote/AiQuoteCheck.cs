using System.Text.Json.Serialization;

namespace RandaSports.Application.Features.Commentary.Command.VerifyOpinionQuote;

/// <summary>Modelin ikinci bakışta verdiği cevap.</summary>
public sealed record AiQuoteCheck
{
    /// <summary>"birebir" | "yakin" | "farkli" | "baskasi" | "duyulmadi"</summary>
    [JsonPropertyName("sonuc")]
    public string? Verdict { get; init; }

    /// <summary>Pencerede gerçekten duyulan cümle.</summary>
    [JsonPropertyName("duyulan")]
    public string? Heard { get; init; }

    /// <summary>Cümleyi söyleyen kişi — videonun içinden belirlenmiş hali.</summary>
    [JsonPropertyName("konusan")]
    public string? Speaker { get; init; }

    /// <summary>Cümlenin pencere içindeki başlangıcı, saniye.</summary>
    [JsonPropertyName("saniye")]
    public int? OffsetSeconds { get; init; }
}

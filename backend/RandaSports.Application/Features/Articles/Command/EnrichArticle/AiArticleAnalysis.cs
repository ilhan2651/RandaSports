using System.Text.Json.Serialization;

namespace RandaSports.Application.Features.Articles.Command.EnrichArticle;

public sealed record AiArticleAnalysis
{
    [JsonPropertyName("headline")]
    public string? Headline { get; init; }

    [JsonPropertyName("summary")]
    public string? Summary { get; init; }

    [JsonPropertyName("body")]
    public string? Body { get; init; }

    [JsonPropertyName("isLive")]
    public bool IsLive { get; init; }

    [JsonPropertyName("analysis")]
    public string? Analysis { get; init; }

    [JsonPropertyName("category")]
    public string? Category { get; init; }

    /// <summary>
    /// Modelin tahmin ettiği branş slug'ı. Yalnızca kaynakta branş yoksa VE takım sözlüğü
    /// karar veremediğinde kullanılıyor; kayıtlı bir slug'a denk gelmiyorsa yok sayılıyor.
    /// </summary>
    [JsonPropertyName("sport")]
    public string? Sport { get; init; }

    [JsonPropertyName("teams")]
    public List<string> Teams { get; init; } = [];

    [JsonPropertyName("athletes")]
    public List<string> Athletes { get; init; } = [];

    [JsonPropertyName("facts")]
    public List<string> Facts { get; init; } = [];

    [JsonPropertyName("quotes")]
    public List<AiQuote> Quotes { get; init; } = [];
}

public sealed record AiQuote
{
    [JsonPropertyName("speaker")]
    public string? Speaker { get; init; }

    [JsonPropertyName("text")]
    public string? Text { get; init; }
}

using System.Text.Json.Serialization;

namespace RandaSports.Application.Features.Stories.Command.WriteStory;

public sealed record AiStoryContent
{
    [JsonPropertyName("headline")]
    public string? Headline { get; init; }

    [JsonPropertyName("summary")]
    public string? Summary { get; init; }

    [JsonPropertyName("body")]
    public string? Body { get; init; }

    [JsonPropertyName("analysis")]
    public string? Analysis { get; init; }

    [JsonPropertyName("isLive")]
    public bool IsLive { get; init; }

    [JsonPropertyName("category")]
    public string? Category { get; init; }

    [JsonPropertyName("conflicts")]
    public List<string> Conflicts { get; init; } = [];

    [JsonPropertyName("quotes")]
    public List<AiStoryQuote> Quotes { get; init; } = [];
}

public sealed record AiStoryQuote
{
    [JsonPropertyName("speaker")]
    public string? Speaker { get; init; }

    [JsonPropertyName("text")]
    public string? Text { get; init; }
}

/// <summary>Modele verilecek tek kaynak.</summary>
public sealed record StorySourceInput(
    string SourceName,
    string Title,
    DateTimeOffset PublishedAt,
    string? Summary,
    string? Body,
    bool IsLive);

namespace RandaSports.Infrastructure.Ai;

public sealed class GeminiOptions
{
    public const string SectionName = "Gemini";

    public string ApiKey { get; set; } = string.Empty;
    public string Model { get; set; } = "gemini-3.5-flash-lite";
    public string BaseUrl { get; set; } = "https://generativelanguage.googleapis.com/v1beta/";
    public double Temperature { get; set; } = 0.2;

    /// <summary>Video analizi uzun sürebildiği için metin isteklerine göre yüksek tutuluyor.</summary>
    public int TimeoutSeconds { get; set; } = 300;
}

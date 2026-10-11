namespace RandaSports.Application.Interfaces.Services;

public enum GeminiOutcome
{
    /// <summary>Model cevap verdi.</summary>
    Success,

    /// <summary>Kota (429) ya da sunucu hatası (5xx). İstek sağlıklıydı, sistem meşguldü.</summary>
    Transient,

    /// <summary>İstekte ya da cevapta sorun var: anahtar yok, içerik üretilemedi, boş cevap.</summary>
    Permanent
}

/// <summary>
/// Modelin cevabı ve çağrının künyesi. Eskiden yalnızca <c>string?</c> dönüyordu;
/// <c>null</c> hem "kota doldu" hem "geçersiz cevap" anlamına geldiği için çağıran
/// taraf ikisini ayırt edemiyor ve geçici hatalar kalıcı kayba dönüşüyordu.
/// </summary>
public sealed record GeminiResult
{
    public required GeminiOutcome Outcome { get; init; }
    public string? Json { get; init; }
    public int PromptTokens { get; init; }
    public int OutputTokens { get; init; }

    public bool IsSuccess => Outcome == GeminiOutcome.Success && Json is not null;

    /// <summary>true ise çağıran taraf deneme hakkını yakmamalı.</summary>
    public bool IsTransient => Outcome == GeminiOutcome.Transient;

    public static GeminiResult Ok(string json, int promptTokens, int outputTokens) => new()
    {
        Outcome = GeminiOutcome.Success,
        Json = json,
        PromptTokens = promptTokens,
        OutputTokens = outputTokens
    };

    public static GeminiResult Transient() => new() { Outcome = GeminiOutcome.Transient };

    public static GeminiResult Permanent() => new() { Outcome = GeminiOutcome.Permanent };
}

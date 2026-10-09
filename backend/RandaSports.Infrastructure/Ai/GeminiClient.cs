using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RandaSports.Application.Interfaces.Services;

namespace RandaSports.Infrastructure.Ai;

public sealed class GeminiClient(
    HttpClient httpClient,
    IOptions<GeminiOptions> options,
    ILogger<GeminiClient> logger) : IGeminiClient
{
    private static readonly TimeSpan[] RetryDelays =
    [
        TimeSpan.FromSeconds(3),
        TimeSpan.FromSeconds(10),
        TimeSpan.FromSeconds(30)
    ];

    private readonly GeminiOptions _options = options.Value;

    public Task<string?> GenerateJsonAsync(string prompt, CancellationToken cancellationToken = default) =>
        SendWithRetryAsync(prompt, null, null, null, cancellationToken);

    public Task<string?> GenerateJsonFromVideoAsync(
        string prompt,
        string videoUrl,
        int? startSeconds = null,
        int? endSeconds = null,
        CancellationToken cancellationToken = default) =>
        SendWithRetryAsync(prompt, videoUrl, startSeconds, endSeconds, cancellationToken);

    private async Task<string?> SendWithRetryAsync(
        string prompt,
        string? videoUrl,
        int? startSeconds,
        int? endSeconds,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(_options.ApiKey))
        {
            logger.LogError("Gemini API anahtarı ayarlanmamış (Gemini:ApiKey).");
            return null;
        }

        for (var attempt = 0; attempt <= RetryDelays.Length; attempt++)
        {
            var (text, shouldRetry) = await TrySendAsync(
                prompt,
                videoUrl,
                startSeconds,
                endSeconds,
                cancellationToken);

            if (!shouldRetry)
                return text;

            if (attempt == RetryDelays.Length)
                break;

            var delay = RetryDelays[attempt];
            logger.LogInformation("Gemini meşgul, {Delay} sn sonra tekrar denenecek.", delay.TotalSeconds);
            await Task.Delay(delay, cancellationToken);
        }

        logger.LogWarning("Gemini birkaç denemeye rağmen cevap vermedi.");
        return null;
    }

    private static object[] BuildParts(
        string prompt,
        string? videoUrl,
        int? startSeconds,
        int? endSeconds)
    {
        if (videoUrl is null)
            return [new { text = prompt }];

        if (startSeconds is null && endSeconds is null)
            return [new { text = prompt }, new { file_data = new { file_uri = videoUrl } }];

        var metadata = new Dictionary<string, object>();

        if (startSeconds is { } start)
            metadata["start_offset"] = new { seconds = start };

        if (endSeconds is { } end)
            metadata["end_offset"] = new { seconds = end };

        return
        [
            new { text = prompt },
            new { file_data = new { file_uri = videoUrl }, video_metadata = metadata }
        ];
    }

    private async Task<(string? Text, bool ShouldRetry)> TrySendAsync(
        string prompt,
        string? videoUrl,
        int? startSeconds,
        int? endSeconds,
        CancellationToken cancellationToken)
    {
        // Video varsa ikinci bir parça olarak bağlantısı ekleniyor; dosya yüklemiyoruz.
        // Aralık verildiyse video_metadata ile gönderiliyor: model yalnızca o bölümü
        // görüyor ve token da aynı oranda düşüyor.
        var parts = BuildParts(prompt, videoUrl, startSeconds, endSeconds);

        var request = new
        {
            contents = new[] { new { parts } },
            generationConfig = new
            {
                temperature = _options.Temperature,
                responseMimeType = "application/json"
            }
        };

        using var message = new HttpRequestMessage(
            HttpMethod.Post,
            $"models/{_options.Model}:generateContent")
        {
            Content = JsonContent.Create(request)
        };

        message.Headers.Add("x-goog-api-key", _options.ApiKey);

        using var response = await httpClient.SendAsync(message, cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            var error = await response.Content.ReadAsStringAsync(cancellationToken);
            var statusCode = (int)response.StatusCode;

            // 503 (model dolu), 429 (kota), 500 geçici — tekrar denemeye değer.
            var transient = response.StatusCode is HttpStatusCode.ServiceUnavailable
                or HttpStatusCode.TooManyRequests
                or HttpStatusCode.InternalServerError;

            if (transient)
            {
                logger.LogWarning("Gemini geçici hata ({StatusCode}).", statusCode);
                return (null, true);
            }

            // Geçici teşhis: aynı istek curl ile 200 dönerken uygulamadan 403
            // geliyor. Anahtarın kendisi loga yazılmıyor, yalnızca uzunluğu —
            // boş ya da kırpılmış gelip gelmediğini görmek için bu yetiyor.
            logger.LogError(
                "Gemini hatası ({StatusCode}): {Error} | adres={Uri} | model={Model} | anahtarUzunluk={KeyLength} | videoVar={HasVideo} | aralik={Start}-{End}",
                statusCode,
                error,
                response.RequestMessage?.RequestUri,
                _options.Model,
                _options.ApiKey?.Length ?? -1,
                videoUrl is not null,
                startSeconds?.ToString() ?? "-",
                endSeconds?.ToString() ?? "-");

            return (null, false);
        }

        await using var stream = await response.Content.ReadAsStreamAsync(cancellationToken);
        using var document = await JsonDocument.ParseAsync(stream, cancellationToken: cancellationToken);

        if (!document.RootElement.TryGetProperty("candidates", out var candidates) || candidates.GetArrayLength() == 0)
        {
            logger.LogWarning("Gemini boş cevap döndürdü.");
            return (null, false);
        }

        var candidate = candidates[0];

        if (!candidate.TryGetProperty("content", out var content)
            || !content.TryGetProperty("parts", out var responseParts)
            || responseParts.GetArrayLength() == 0)
        {
            // Video işlenemediğinde (erişim yok, çok uzun) cevap gövdesiz gelebiliyor.
            var reason = candidate.TryGetProperty("finishReason", out var finish) ? finish.GetString() : "bilinmiyor";
            logger.LogWarning("Gemini içerik döndürmedi (sebep: {Reason}).", reason);
            return (null, false);
        }

        return (responseParts[0].TryGetProperty("text", out var textNode) ? textNode.GetString() : null, false);
    }
}

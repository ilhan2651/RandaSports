using System.Net;
using System.Text;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RandaSports.Application.Interfaces.Services;

namespace RandaSports.Infrastructure.Ai;

public sealed class GeminiClient(
    HttpClient httpClient,
    IOptions<GeminiOptions> options,
    GeminiCircuitBreaker circuitBreaker,
    ILogger<GeminiClient> logger) : IGeminiClient
{
    private static readonly TimeSpan[] RetryDelays =
    [
        TimeSpan.FromSeconds(3),
        TimeSpan.FromSeconds(10),
        TimeSpan.FromSeconds(30)
    ];

    private readonly GeminiOptions _options = options.Value;

    public Task<GeminiResult> GenerateJsonAsync(string prompt, CancellationToken cancellationToken = default) =>
        SendWithRetryAsync(prompt, null, null, null, cancellationToken);

    public Task<GeminiResult> GenerateJsonFromVideoAsync(
        string prompt,
        string videoUrl,
        int? startSeconds = null,
        int? endSeconds = null,
        CancellationToken cancellationToken = default) =>
        SendWithRetryAsync(prompt, videoUrl, startSeconds, endSeconds, cancellationToken);

    private async Task<GeminiResult> SendWithRetryAsync(
        string prompt,
        string? videoUrl,
        int? startSeconds,
        int? endSeconds,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(_options.ApiKey))
        {
            logger.LogError("Gemini API anahtarı ayarlanmamış (Gemini:ApiKey).");
            return GeminiResult.Permanent();
        }

        // Devre açıkken HTTP'ye hiç çıkmıyoruz. Günlük kota saatlerce geri
        // gelmediği için her turda yeniden denemek sadece log üretiyordu.
        if (circuitBreaker.IsOpen(out var remaining))
        {
            logger.LogDebug("Gemini devresi açık, {Minutes:F0} dk kaldı.", remaining.TotalMinutes);
            return GeminiResult.Transient();
        }

        for (var attempt = 0; attempt <= RetryDelays.Length; attempt++)
        {
            var (result, shouldRetry) = await TrySendAsync(
                prompt,
                videoUrl,
                startSeconds,
                endSeconds,
                cancellationToken);

            if (!shouldRetry)
                return result;

            if (attempt == RetryDelays.Length)
                break;

            var delay = RetryDelays[attempt];
            logger.LogInformation("Gemini meşgul, {Delay} sn sonra tekrar denenecek.", delay.TotalSeconds);
            await Task.Delay(delay, cancellationToken);
        }

        logger.LogWarning("Gemini birkaç denemeye rağmen cevap vermedi.");

        // Denemeler tükendi ama sebep hâlâ geçici: çağıran taraf deneme hakkını yakmamalı.
        return GeminiResult.Transient();
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

    /// <summary>Cevaptaki sayısal alanı okur; alan yoksa 0 döner.</summary>
    private static int ReadInt(JsonElement parent, string name) =>
        parent.TryGetProperty(name, out var node) && node.TryGetInt32(out var value) ? value : 0;

    private async Task<(GeminiResult Result, bool ShouldRetry)> TrySendAsync(
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

        // Geçici teşhis: gövdeyi elle serileştiriyoruz ki loga aynısını
        // yazabilelim. JsonContent.Create nesneyi içeride serileştirdiği için
        // gönderilen metni görmenin başka yolu yok.
        var json = JsonSerializer.Serialize(request);

        using var message = new HttpRequestMessage(
            HttpMethod.Post,
            $"models/{_options.Model}:generateContent")
        {
            Content = new StringContent(json, Encoding.UTF8, "application/json")
        };

        message.Headers.Add("x-goog-api-key", _options.ApiKey);

        using var response = await httpClient.SendAsync(message, cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            var error = await response.Content.ReadAsStringAsync(cancellationToken);
            var statusCode = (int)response.StatusCode;

            // 429 kota: saatlerce sürebiliyor, 30 saniye sonra tekrar denemek
            // boşuna. Devreyi açıp hemen çekiliyoruz.
            if (response.StatusCode is HttpStatusCode.TooManyRequests)
            {
                circuitBreaker.Trip(
                                   TimeSpan.FromMinutes(_options.QuotaCooldownMinutes),
                                   error.Length > 400 ? error[..400] : error);
                return (GeminiResult.Transient(), false);
            }

            // 503 (model dolu) ve 500 gerçekten geçici: kısa beklemeyle düzeliyor.
            var transient = response.StatusCode is HttpStatusCode.ServiceUnavailable
                or HttpStatusCode.InternalServerError;

            if (transient)
            {
                logger.LogWarning("Gemini geçici hata ({StatusCode}).", statusCode);
                return (GeminiResult.Transient(), true);
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

            // Gövdenin başı ve sonu: parçaların gerçekten dolu gidip gitmediğini
            // ve video bağlantısının yerinde olup olmadığını gösteriyor.
            logger.LogError(
                "Gemini gövde ({Length} karakter): {Head} ... {Tail}",
                json.Length,
                json.Length > 180 ? json[..180] : json,
                json.Length > 260 ? json[^260..] : string.Empty);

            return (GeminiResult.Permanent(), false);
        }

        await using var stream = await response.Content.ReadAsStreamAsync(cancellationToken);
        using var document = await JsonDocument.ParseAsync(stream, cancellationToken: cancellationToken);

        // Token künyesi her cevapta geliyor; şimdiye kadar okunmuyordu. Maliyeti
        // ölçebilmenin tek kaynağı bu.
        var promptTokens = 0;
        var outputTokens = 0;

        if (document.RootElement.TryGetProperty("usageMetadata", out var usage))
        {
            promptTokens = ReadInt(usage, "promptTokenCount");
            outputTokens = ReadInt(usage, "candidatesTokenCount");
        }

        if (!document.RootElement.TryGetProperty("candidates", out var candidates) || candidates.GetArrayLength() == 0)
        {
            logger.LogWarning("Gemini boş cevap döndürdü.");
            return (GeminiResult.Permanent(), false);
        }

        var candidate = candidates[0];

        if (!candidate.TryGetProperty("content", out var content)
            || !content.TryGetProperty("parts", out var responseParts)
            || responseParts.GetArrayLength() == 0)
        {
            // Video işlenemediğinde (erişim yok, çok uzun) cevap gövdesiz gelebiliyor.
            var reason = candidate.TryGetProperty("finishReason", out var finish) ? finish.GetString() : "bilinmiyor";
            logger.LogWarning("Gemini içerik döndürmedi (sebep: {Reason}).", reason);
            return (GeminiResult.Permanent(), false);
        }

        var text = responseParts[0].TryGetProperty("text", out var textNode) ? textNode.GetString() : null;

        if (string.IsNullOrWhiteSpace(text))
        {
            logger.LogWarning("Gemini boş metin döndürdü.");
            return (GeminiResult.Permanent(), false);
        }

        circuitBreaker.Close();

        logger.LogInformation(
            "Gemini token: {In} girdi / {Out} cikti ({Kind})",
            promptTokens,
            outputTokens,
            videoUrl is null ? "metin" : "video");

        return (GeminiResult.Ok(text, promptTokens, outputTokens), false);
    }
}

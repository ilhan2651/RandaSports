using System.Net;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RandaSports.Application.Interfaces.Services;

namespace RandaSports.Infrastructure.Ai;

/// <summary>
/// Kota ve tekrar deneme mantığı tek yerde duruyor. Handler'lar ne Gemini'yi ne
/// de kotayı biliyor; hepsi yalnızca <see cref="IChatClient"/> görüyor.
/// Sağlayıcı değişse bu sınıf yerinde kalıyor.
/// </summary>
public sealed class QuotaAwareChatClient(
    IChatClient inner,
    GeminiCircuitBreaker circuitBreaker,
    IOptions<GeminiOptions> options,
    ILogger<QuotaAwareChatClient> logger) : DelegatingChatClient(inner)
{
    /// <summary>
    /// 503 (model dolu) kısa beklemeyle düzeliyor, o yüzden tekrar deneniyor.
    /// Kota öyle değil — saatlerce geri gelmiyor — ayrı ele alınıyor.
    /// </summary>
    private static readonly TimeSpan[] RetryDelays =
    [
        TimeSpan.FromSeconds(3),
        TimeSpan.FromSeconds(10),
        TimeSpan.FromSeconds(30)
    ];

    public override async Task<ChatResponse> GetResponseAsync(
        IEnumerable<ChatMessage> messages,
        ChatOptions? chatOptions = null,
        CancellationToken cancellationToken = default)
    {
        // Devre açıkken sağlayıcıya hiç çıkmıyoruz: günlük kota saatlerce geri
        // gelmiyor, her turda denemek sadece log üretiyordu.
        if (circuitBreaker.IsOpen(out var remaining))
            throw new AiQuotaExhaustedException($"Kota dolu, {remaining.TotalMinutes:F0} dk kaldı.");

        for (var attempt = 0; ; attempt++)
        {
            try
            {
                var response = await base.GetResponseAsync(messages, chatOptions, cancellationToken);

                circuitBreaker.Close();

                // Token künyesi tüm handler'lar için tek yerde: maliyeti ölçmek için
                // tek kaynak bu ve artık çağrı yerleri bunu bilmek zorunda değil.
                logger.LogInformation(
                    "Gemini token: {In} girdi / {Out} cikti",
                    response.Usage?.InputTokenCount ?? 0,
                    response.Usage?.OutputTokenCount ?? 0);

                return response;
            }
            catch (Exception ex) when (IsQuotaError(ex))
            {
                circuitBreaker.Trip(
                    TimeSpan.FromMinutes(options.Value.QuotaCooldownMinutes),
                    ex.Message.Length > 400 ? ex.Message[..400] : ex.Message);

                throw new AiQuotaExhaustedException("Sağlayıcı kotası doldu.", ex);
            }
            catch (Exception ex) when (attempt < RetryDelays.Length
                                       && !cancellationToken.IsCancellationRequested
                                       && IsTransient(ex))
            {
                var delay = RetryDelays[attempt];

                logger.LogInformation(
                    "Model meşgul ({Type}), {Delay} sn sonra tekrar denenecek.",
                    ex.GetType().Name,
                    delay.TotalSeconds);

                await Task.Delay(delay, cancellationToken);
            }
        }
    }

    /// <summary>Kısa beklemeyle düzelen hatalar: model dolu, sunucu hatası, zaman aşımı.</summary>
    private static bool IsTransient(Exception ex) =>
        ex is HttpRequestException
        {
            StatusCode: HttpStatusCode.ServiceUnavailable or HttpStatusCode.InternalServerError
        }
        || ex is TaskCanceledException
        || ex.Message.Contains("UNAVAILABLE", StringComparison.OrdinalIgnoreCase)
        || ex.Message.Contains("503", StringComparison.Ordinal);

    /// <summary>
    /// Google SDK'sının kota hatasını hangi tiple fırlattığını henüz bilmiyoruz.
    /// Tanımadığımız hataları loglayıp geçiyoruz; ilk gerçek 429'dan sonra burası
    /// daraltılacak.
    /// </summary>
    private bool IsQuotaError(Exception ex)
    {
        if (ex is HttpRequestException { StatusCode: HttpStatusCode.TooManyRequests })
            return true;

        var quota = ex.Message.Contains("RESOURCE_EXHAUSTED", StringComparison.OrdinalIgnoreCase)
                    || ex.Message.Contains("429", StringComparison.Ordinal);

        if (!quota && !IsTransient(ex))
            logger.LogWarning(ex, "Tanınmayan AI hatası ({Type}).", ex.GetType().Name);

        return quota;
    }
}

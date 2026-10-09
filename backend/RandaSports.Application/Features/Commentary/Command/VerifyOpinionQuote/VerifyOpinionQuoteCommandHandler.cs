using System.Net;
using System.Text.Json;
using MediatR;
using Microsoft.Extensions.Logging;
using RandaSports.Application.Common.Wrappers;
using RandaSports.Application.Interfaces;
using RandaSports.Application.Interfaces.Repositories;
using RandaSports.Application.Interfaces.Services;
using RandaSports.Domain.Entities;
using RandaSports.Domain.Enums;

namespace RandaSports.Application.Features.Commentary.Command.VerifyOpinionQuote;

/// <summary>
/// Bekleyen bir görüşün alıntısını, damganın etrafındaki kısa pencereyi modele
/// tekrar izleterek kontrol eder.
///
/// Neden ayrı bir çağrı: çıkarım sırasında model 8-40 dakikalık bir bölümü
/// dinleyip onlarca cümleden birini seçiyor; burada tek bir cümleye, 40 saniyelik
/// bir pencerede bakıyor. İkinci bakış birincisinden bağımsız ve çok daha dar.
///
/// Onay durumuna DOKUNMUYOR. Sonucu yalnızca kontrol alanlarına yazıyor; görüşü
/// yayına almak ya da reddetmek onay ekranındaki insanın işi. Doğrulanmış görüş
/// bu ürünün tek iddiası, onu makineye imzalatmıyoruz.
/// </summary>
public sealed class VerifyOpinionQuoteCommandHandler(
    IOpinionRepository opinionRepository,
    IGeminiClient geminiClient,
    IUnitOfWork unitOfWork,
    TimeProvider timeProvider,
    ILogger<VerifyOpinionQuoteCommandHandler> logger)
    : IRequestHandler<VerifyOpinionQuoteCommand, Result<bool>>
{
    private const int HeardMaxLength = 2000;
    private const int SpeakerMaxLength = 100;

    public async Task<Result<bool>> Handle(VerifyOpinionQuoteCommand request, CancellationToken cancellationToken)
    {
        var opinion = await opinionRepository.GetWithContextAsync(request.OpinionId, cancellationToken);

        if (opinion is null)
            return Result<bool>.Fail("Görüş bulunamadı.", HttpStatusCode.NotFound);

        // Damgası olmayan görüşte bakılacak pencere yok. Videoyu baştan sona
        // taratmak bu kontrolün ucuz olma sebebini ortadan kaldırırdı.
        if (opinion.TimestampSeconds is null)
        {
            await WriteAsync(opinion, QuoteCheckResult.NotHeard, null, null, null, cancellationToken);
            return Result<bool>.Ok(false, "Görüşün zaman damgası yok, kontrol edilemedi.");
        }

        var (start, end) = Window(opinion.TimestampSeconds.Value, request.WindowSeconds, opinion.Video.DurationSeconds);

        var prompt = QuoteCheckPromptBuilder.Build(
            opinion.Quote,
            opinion.Commentator?.FullName ?? opinion.SpeakerLabel);

        string? json;
        try
        {
            json = await geminiClient.GenerateJsonFromVideoAsync(
                prompt,
                $"https://www.youtube.com/watch?v={opinion.Video.YouTubeVideoId}",
                start,
                end,
                cancellationToken);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch (Exception ex)
        {
            // Failed olarak işaretliyoruz, NotChecked bırakmıyoruz: tur her seferinde
            // aynı kayda takılıp kuyruğun gerisine hiç geçmesin.
            logger.LogWarning(ex, "Alıntı kontrolü başarısız ({OpinionId}).", opinion.Id);
            await WriteAsync(opinion, QuoteCheckResult.Failed, null, null, null, cancellationToken);
            return Result<bool>.Fail("Modele ulaşılamadı.", HttpStatusCode.BadGateway);
        }

        AiQuoteCheck? parsed = null;
        try
        {
            if (!string.IsNullOrWhiteSpace(json))
                parsed = JsonSerializer.Deserialize<AiQuoteCheck>(json);
        }
        catch (JsonException ex)
        {
            logger.LogWarning(ex, "Alıntı kontrolü cevabı okunamadı ({OpinionId}).", opinion.Id);
        }

        if (parsed is null)
        {
            await WriteAsync(opinion, QuoteCheckResult.Failed, null, null, null, cancellationToken);
            return Result<bool>.Fail("Modelin cevabı okunamadı.", HttpStatusCode.BadGateway);
        }

        var sonuc = ParseVerdict(parsed.Verdict);

        // Modelin verdiği saniye pencerenin başından itibaren; mutlak ana çeviriyoruz.
        var duzeltilmis = parsed.OffsetSeconds is >= 0 && sonuc is not QuoteCheckResult.NotHeard
            ? start + parsed.OffsetSeconds.Value
            : (int?)null;

        await WriteAsync(opinion, sonuc, parsed.Heard, parsed.Speaker, duzeltilmis, cancellationToken);

        logger.LogInformation(
            "Alıntı kontrolü {Sonuc} ({Sapma}): {Speaker} — {Topic}",
            sonuc,
            duzeltilmis is null ? "sapma ölçülemedi" : $"{duzeltilmis - opinion.TimestampSeconds:+0;-0;0} sn",
            opinion.Commentator?.FullName ?? opinion.SpeakerLabel ?? "bilinmiyor",
            opinion.Topic);

        return Result<bool>.Ok(sonuc == QuoteCheckResult.Verbatim);
    }

    /// <summary>
    /// Bakılacak aralık. Damga zaten kayabildiği için pencere sözün iki yanını da
    /// kapsıyor; başı sıfırın, sonu video süresinin dışına taşmıyor.
    /// </summary>
    private static (int Start, int End) Window(int timestamp, int windowSeconds, int? durationSeconds)
    {
        var genislik = Math.Max(5, windowSeconds);
        var start = Math.Max(0, timestamp - genislik);
        var end = timestamp + genislik;

        if (durationSeconds is > 0)
            end = Math.Min(end, durationSeconds.Value);

        // Damga süreyi aşıyorsa pencere ters dönebiliyor; en az bir aralık bırakıyoruz.
        if (end <= start)
            end = start + genislik;

        return (start, end);
    }

    private static QuoteCheckResult ParseVerdict(string? value) =>
        value?.Trim().ToLowerInvariant() switch
        {
            "birebir" => QuoteCheckResult.Verbatim,
            "yakin" or "yakın" => QuoteCheckResult.Paraphrased,
            "farkli" or "farklı" => QuoteCheckResult.Different,
            "baskasi" or "başkası" => QuoteCheckResult.WrongSpeaker,
            // Tanımadığımız bir cevap da "duyulmadı" sayılıyor: belirsiz cevabı
            // olumlu yorumlamak bu kontrolün amacına aykırı.
            _ => QuoteCheckResult.NotHeard
        };

    private async Task WriteAsync(
        Opinion opinion,
        QuoteCheckResult sonuc,
        string? heard,
        string? speaker,
        int? timestampSeconds,
        CancellationToken cancellationToken)
    {
        opinion.QuoteCheck = sonuc;
        opinion.QuoteCheckHeard = Truncate(heard, HeardMaxLength);
        opinion.QuoteCheckSpeaker = Truncate(speaker, SpeakerMaxLength);
        opinion.QuoteCheckTimestampSeconds = timestampSeconds;
        opinion.QuoteCheckedAt = timeProvider.GetUtcNow();

        await unitOfWork.SaveChangesAsync(cancellationToken);
    }

    private static string? Truncate(string? value, int maxLength)
    {
        if (string.IsNullOrWhiteSpace(value))
            return null;

        var trimmed = value.Trim();
        return trimmed.Length <= maxLength ? trimmed : trimmed[..maxLength];
    }
}

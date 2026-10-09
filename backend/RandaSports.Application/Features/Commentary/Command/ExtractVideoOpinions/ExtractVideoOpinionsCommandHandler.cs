using System.Globalization;
using System.Net;
using System.Text.Json;
using MediatR;
using Microsoft.Extensions.Logging;
using RandaSports.Application.Common.Matching;
using RandaSports.Application.Common.Text;
using RandaSports.Application.Common.Wrappers;
using RandaSports.Application.Interfaces;
using RandaSports.Application.Interfaces.Repositories;
using RandaSports.Application.Interfaces.Services;
using RandaSports.Domain.Entities;
using RandaSports.Domain.Enums;

namespace RandaSports.Application.Features.Commentary.Command.ExtractVideoOpinions;

public sealed class ExtractVideoOpinionsCommandHandler(
    IVideoRepository videoRepository,
    IOpinionRepository opinionRepository,
    ICommentatorRepository commentatorRepository,
    IStoryRepository storyRepository,
    ITeamMatcher teamMatcher,
    ISportsReadRepository sportsReadRepository,
    IGeminiClient geminiClient,
    IUnitOfWork unitOfWork,
    TimeProvider timeProvider,
    ILogger<ExtractVideoOpinionsCommandHandler> logger)
    : IRequestHandler<ExtractVideoOpinionsCommand, Result<int>>
{
    private const int TopicMaxLength = 200;
    private const int SummaryMaxLength = 2000;
    private const int QuoteMaxLength = 2000;
    private const int PredictionMaxLength = 300;
    private const int SpeakerLabelMaxLength = 100;
    private const int QuoteMinLength = 15;

    /// <summary>
    /// Kopya alıntı aramasında iki sözün aynı sayılması için gereken en az ortak
    /// baş uzunluğu. Bu kadarı birebir tutuyorsa tesadüf değil, aynı söz.
    /// </summary>
    private const int QuoteFingerprintLength = 25;
    private const int SummaryMaxForVideo = 2000;

    /// <summary>
    /// Bu süreye kadar olan videolar tek konuşmacılı klip sayılıyor; başlıktaki isim
    /// o klipte konuşan kişidir. Uzun programlarda başlık yalnızca bir konuğu anar,
    /// o yüzden orada başlığa bakmıyoruz.
    /// </summary>
    private const int SingleSpeakerClipSeconds = 180;

    /// <summary>
    /// Bu güvenin altındaki atıflar onay ekranına düşüyor. Alt bant (0.95) ve başlık
    /// (0.75-0.9) üstünde kalıyor, kulaktan duyma hitap (0.55-0.7) altında.
    /// </summary>
    private const double AutoApproveConfidence = 0.75;

    /// <summary>
    /// Sözlüğe yeni isim yazarken kabul edilen en kısa ad. Eşleştirmenin alt sınırından
    /// yüksek: sözlüğe yazmak eşleştirmekten daha kalıcı bir karar, "Ali B" gibi
    /// parçalar kayıt açmasın ("Ali Koç" 7 karakterle geçiyor).
    /// </summary>
    private const int MinNewNameLength = 7;

    private const int MaxNewNameLength = 60;

    /// <summary>
    /// Otomatik onaylanan görüşleri insanın onayladıklarından ayırmak için not öneki.
    /// Video yeniden işlenirse otomatik kayıtlar tazeleniyor, insanın dokunduğu
    /// kayıtlara dokunulmuyor.
    /// </summary>
    private const string AutoReviewNotePrefix = "Otomatik onay";

    public async Task<Result<int>> Handle(ExtractVideoOpinionsCommand request, CancellationToken cancellationToken)
    {
        var video = await videoRepository.GetWithChannelAsync(request.VideoId, cancellationToken);
        if (video is null)
            return Result<int>.Fail("Video bulunamadı.", HttpStatusCode.NotFound);

        video.ProcessingAttempts++;

        var (candidates, isChannelRoster) = await ResolveCandidatesAsync(video, cancellationToken);

        // Branş adreslerini bir kez okuyup hem isteme hem de modelin cevabını
        // doğrulamaya veriyoruz: model listede olmayan bir şey yazarsa atılıyor.
        var sports = await sportsReadRepository.GetAllSportsAsync(cancellationToken);
        var sportSlugs = sports.Select(x => x.Slug).ToList();
        var channelSportSlugs = video.Channel.Sports.Select(x => x.Slug).ToList();

        var videoUrl = $"https://www.youtube.com/watch?v={video.YouTubeVideoId}";

        var segments = PlanSegments(
            video.DurationSeconds,
            request.SegmentThresholdMinutes,
            request.SegmentMinutes,
            request.SegmentOverlapSeconds,
            request.MaxSegmentsPerVideo);

        if (segments.Count > 1)
            logger.LogInformation(
                "Uzun video {Count} dilimde işleniyor ({Minutes} dk): {Title}",
                segments.Count,
                (video.DurationSeconds ?? 0) / 60,
                video.Title);

        var collected = new List<AiVideoOpinion>();
        string? videoSummary = null;
        var failure = 0;

        for (var index = 0; index < segments.Count; index++)
        {
            var segment = segments[index];

            var prompt = OpinionPromptBuilder.Build(
                video.Title,
                video.Description,
                candidates.Select(x => x.FullName).ToList(),
                isChannelRoster,
                sportSlugs,
                channelSportSlugs,
                segments.Count > 1 ? index + 1 : null,
                segments.Count > 1 ? segments.Count : null);

            string? json;
            try
            {
                json = await geminiClient.GenerateJsonFromVideoAsync(
                    prompt,
                    videoUrl,
                    segment.Start,
                    segment.End,
                    cancellationToken);
            }
            catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
            {
                // Uygulama kapanıyor: denemeyi harcamadan çekiliyoruz, video sırada kalsın.
                throw;
            }
            catch (Exception ex)
            {
                // İstisna buradan dışarı çıkarsa scope atılıyor ve yukarıdaki
                // ProcessingAttempts++ hiç kaydedilmiyor; video sonsuza kadar aynı yere
                // düşüyor. Zaman aşımı ve ağ hatası bu yüzden burada yakalanıyor.
                logger.LogWarning(ex, "Gemini çağrısı başarısız ({VideoId}).", video.Id);
                failure++;
                continue;
            }

            if (string.IsNullOrWhiteSpace(json))
            {
                failure++;
                continue;
            }

            // Teşhis: modelin ham cevabının başı. Alan adları tutmazsa ya da cevap
            // beklediğimiz biçimde değilse burada görünür.
            logger.LogInformation(
                "Model cevabı ({Length} karakter): {Head}",
                json.Length,
                json.Length > 400 ? json[..400] : json);

            AiVideoAnalysis? parsed;
            try
            {
                parsed = JsonSerializer.Deserialize<AiVideoAnalysis>(json);
            }
            catch (JsonException ex)
            {
                logger.LogWarning(ex, "Video cevabı okunamadı ({VideoId}).", video.Id);
                failure++;
                continue;
            }

            if (parsed is null)
            {
                failure++;
                continue;
            }

            videoSummary ??= parsed.VideoSummary;

            logger.LogInformation(
                "Modelden {Count} görüş geldi (dilim {Index}/{Total}): {Kinds}",
                parsed.Opinions.Count,
                index + 1,
                segments.Count,
                string.Join(", ", parsed.Opinions.Select(x =>
                    $"{x.Kind ?? "?"}/{(string.IsNullOrWhiteSpace(x.Speaker) ? "ISIMSIZ" : x.Speaker)}")));

            // Model dilimin başından itibaren zaman veriyor; mutlak ana burada çeviriyoruz.
            foreach (var item in parsed.Opinions)
                collected.Add(ShiftTimestamp(item, segment.Start));
        }

        // Dilimlerin TAMAMI başarısızsa bu gerçek bir hata; biri tutmuşsa elimizdekiyle
        // devam ediyoruz, yarım sonuç hiç sonuçtan iyi.
        if (failure == segments.Count)
            return await FailAsync(video, "Modele ulaşılamadı.", HttpStatusCode.BadGateway, cancellationToken);

        video.AiSummary = Truncate(videoSummary, SummaryMaxForVideo);

        var secilen = RankAndTrim(collected, request.MaxOpinionsPerVideo);

        if (collected.Count != secilen.Count)
            logger.LogInformation(
                "Sıralama sonrası {Before} görüşten {After} tanesi alındı (video başı sınır {Limit}).",
                collected.Count,
                secilen.Count,
                request.MaxOpinionsPerVideo);

        var analysis = new AiVideoAnalysis
        {
            VideoSummary = videoSummary,
            Opinions = secilen
        };

        // Model videoyu izleyemediyse boş liste dönüyor; bu bir hata değil, atlanacak video.
        if (analysis.Opinions.Count == 0)
        {
            video.ProcessingStatus = VideoProcessingStatus.Skipped;
            video.ProcessingError = "Videoda alınacak görüş bulunamadı.";
            video.ProcessedAt = timeProvider.GetUtcNow();
            await unitOfWork.SaveChangesAsync(cancellationToken);
            return Result<int>.Ok(0, video.ProcessingError);
        }

        await ClearPendingOpinionsAsync(video.Id, cancellationToken);

        var clusterRefs = await storyRepository.GetRecentClusterRefsAsync(
            timeProvider.GetUtcNow().AddDays(-request.StoryMatchDays),
            cancellationToken);

        var saved = 0;
        var autoApproved = 0;

        // Aynı video içinde aynı yeni isim birkaç görüşte geçebiliyor; iki kez
        // oluşturmamak için bu turda eklenenleri burada tutuyoruz.
        var newCommentators = new Dictionary<string, Commentator>(StringComparer.Ordinal);
        var sportIdsBySlug = sports.ToDictionary(x => x.Slug, x => x.Id, StringComparer.OrdinalIgnoreCase);

        foreach (var item in analysis.Opinions)
        {
            var opinion = await BuildOpinionAsync(
                video,
                item,
                candidates,
                isChannelRoster,
                clusterRefs,
                newCommentators,
                sportIdsBySlug,
                cancellationToken);

            if (opinion is null)
                continue;

            await opinionRepository.AddAsync(opinion, cancellationToken);
            saved++;

            if (opinion.Status == OpinionStatus.Approved)
                autoApproved++;
        }

        video.ProcessingStatus = saved > 0 ? VideoProcessingStatus.Processed : VideoProcessingStatus.Skipped;
        video.ProcessingError = saved > 0 ? null : "Görüşlerin hiçbiri kurallara uymadı.";
        video.ProcessedAt = timeProvider.GetUtcNow();

        await unitOfWork.SaveChangesAsync(cancellationToken);

        logger.LogInformation(
            "{Channel} — {Title}: {Count} görüş kaydedildi ({Auto} otomatik onaylandı, {Pending} onay bekliyor).",
            video.Channel.Name,
            video.Title,
            saved,
            autoApproved,
            saved - autoApproved);

        return Result<int>.Ok(saved);
    }

    /// <summary>
    /// Kanalın kadrosu doluysa onu kullanıyoruz — bu kadro, onay ekranında insanın
    /// doğruladığı isimlerden oluşuyor, yani güçlü bir ipucu. Kadro boşsa tüm
    /// yorumcu sözlüğüne düşüyoruz; o zayıf bir liste olduğu için modele öyle söylüyoruz.
    /// </summary>
    private async Task<(List<Commentator> Candidates, bool IsChannelRoster)> ResolveCandidatesAsync(
        Video video,
        CancellationToken cancellationToken)
    {
        // Kadro güveni yalnızca doğrulanmış isimlere: otomatik eklenmiş bir isim
        // yanlışsa, kadroya girip sonraki videolarda da yanlış eşleşme üretmesin.
        var roster = video.Channel.RegularCommentators.Where(x => x.IsVerified).ToList();

        if (roster.Count > 0)
            return (roster, true);

        return (await commentatorRepository.GetActiveAsync(cancellationToken), false);
    }

    /// <summary>
    /// Video yeniden işlenirse eski görüşler tekrarlanmasın diye siliniyor. Bekleyenlerin
    /// yanında otomatik onaylananlar da tazeleniyor — yoksa her yeniden işlemede aynı
    /// görüş ikinci kez yayına girerdi. İnsanın onayladığı ya da reddettiği kayıtlara
    /// dokunulmuyor.
    /// </summary>
    private async Task ClearPendingOpinionsAsync(Guid videoId, CancellationToken cancellationToken)
    {
        var existing = await opinionRepository.GetByVideoAsync(videoId, cancellationToken);

        var temizlenecek = existing.Where(x =>
            x.Status == OpinionStatus.Pending
            || (x.Status == OpinionStatus.Approved
                && x.ReviewNote?.StartsWith(AutoReviewNotePrefix, StringComparison.Ordinal) == true));

        foreach (var opinion in temizlenecek)
            opinionRepository.Delete(opinion);
    }

    private async Task<Opinion?> BuildOpinionAsync(
        Video video,
        AiVideoOpinion item,
        IReadOnlyCollection<Commentator> candidates,
        bool isChannelRoster,
        List<StoryClusterRef> clusterRefs,
        Dictionary<string, Commentator> newCommentators,
        Dictionary<string, Guid> sportIdsBySlug,
        CancellationToken cancellationToken)
    {
        // Muhabirin olay aktarımı ve sunucunun sorusu yayına girmiyor: ikisi de
        // konuşmacının görüşü değil, biz görüş yayınlıyoruz.
        if (!string.Equals(item.Kind?.Trim(), "gorus", StringComparison.OrdinalIgnoreCase)
            && !string.IsNullOrWhiteSpace(item.Kind))
        {
            logger.LogInformation(
                "Görüş elendi — tür \"{Kind}\" (yalnızca \"gorus\" yayınlanıyor): {Topic}",
                item.Kind,
                item.Topic);

            return null;
        }

        var topic = Truncate(item.Topic, TopicMaxLength);
        var summary = Truncate(item.Summary, SummaryMaxLength);
        var quote = Truncate(item.Quote, QuoteMaxLength);

        if (topic is null || summary is null || quote is null)
        {
            logger.LogInformation(
                "Görüş elendi — zorunlu alan boş (konu:{Topic} özet:{Summary} alıntı:{Quote}): {Speaker}",
                topic is null ? "YOK" : "var",
                summary is null ? "YOK" : "var",
                quote is null ? "YOK" : "var",
                item.Speaker ?? "isimsiz");

            return null;
        }

        // Tek kelimelik "alıntı" işe yaramaz; modelin boş geçtiği alan olur.
        if (quote.Length < QuoteMinLength)
        {
            logger.LogInformation(
                "Görüş elendi — alıntı çok kısa ({Length} < {Min}): \"{Quote}\"",
                quote.Length,
                QuoteMinLength,
                quote);

            return null;
        }

        var modelSpeaker = SpeakerMatcher.Match(item.Speaker, candidates);

        // Kısa klipte başlıktaki isim modelin tahmininden üstün: model açıklamadaki
        // konuk listesinden yanlış ismi seçebiliyor, başlıktaki isim ise o klibin sahibi.
        var isClip = video.DurationSeconds is > 0 and <= SingleSpeakerClipSeconds;
        var titleSpeaker = isClip
            ? SpeakerMatcher.Match(SpeakerMatcher.ExtractTitleSpeaker(video.Title), candidates)
            : null;

        // İsmin nereden geldiği, modelin ne kadar emin olduğundan daha belirleyici.
        var source = NormalizeSource(item.SpeakerSource);

        // Başlıktaki isim çoğu zaman videonun KONUSUDUR, konuşanı değil:
        // "...transferi | Clarke-Harris" başlığındaki oyuncu konuşmuyor. Bu yüzden
        // başlık ve hitap, ancak kişinin o kanalda konuştuğu daha önce insan tarafından
        // doğrulanmışsa (kanal kadrosu) isme bağlanıyor. Alt bant farklı: yayıncının
        // konuşan kişinin yanına yazdığı isim, kadro olmasa da sağlam.
        var commentator = source switch
        {
            "altbant" or "baslik" or "hitap" => titleSpeaker ?? modelSpeaker,
            _ => null
        };

        // Sözlükte karşılığı yok ama ortada yazılı ya da duyulmuş bir isim var:
        // kişiyi sözlüğe alıyoruz. Doğrulanmamış olarak giriyor — görüşü yayına
        // çıksa bile kadro güveni kazanmıyor, admin onaylayana kadar.
        if (commentator is null && source is "altbant" or "baslik" or "hitap")
            commentator = await EnsureCommentatorAsync(
                (isClip ? SpeakerMatcher.ExtractTitleSpeaker(video.Title) : null) ?? item.Speaker,
                video,
                newCommentators,
                cancellationToken);

        var speakerLabel = commentator is not null && titleSpeaker is not null
            ? titleSpeaker.FullName
            : item.Speaker;

        if (commentator is not null && titleSpeaker is not null)
            source = "baslik";

        if (commentator is null && modelSpeaker is not null)
            logger.LogInformation(
                "İsim bağlanmadı ({Source}, kadro {Roster}): {Name} — {VideoTitle}",
                source,
                isChannelRoster ? "var" : "yok",
                modelSpeaker.FullName,
                video.Title);

        if (titleSpeaker is not null && modelSpeaker is not null && titleSpeaker.Id != modelSpeaker.Id)
            logger.LogInformation(
                "Konuşmacı başlığa göre düzeltildi: model {Model} dedi, başlık {Title} diyor ({VideoTitle}).",
                modelSpeaker.FullName,
                titleSpeaker.FullName,
                video.Title);

        // Ne isim eşleşti ne de bir isim duyuldu: görüşü kime ait yazacağımızı bilmiyoruz.
        if (commentator is null && string.IsNullOrWhiteSpace(speakerLabel))
        {
            logger.LogInformation(
                "Görüş elendi — konuşmacı belirlenemedi (kaynak {Source}, modelin dediği \"{Model}\"): {Topic}",
                source,
                item.Speaker ?? "",
                topic);

            return null;
        }

        var confidence = ResolveConfidence(commentator, source, isChannelRoster, item.SpeakerConfidence);

        // İsim yazılı bir kanıta dayanıyorsa görüş doğrudan yayına giriyor; kulaktan
        // duyma ya da belirsizse onay ekranına düşüyor.
        var otomatik = commentator is not null && confidence >= AutoApproveConfidence;

        var subjectText = string.Join(' ', item.Subjects);
        var teams = await teamMatcher.MatchAsync($"{topic} {subjectText}", summary, cancellationToken);
        var team = teams.FirstOrDefault(x => x.InTitle) ?? teams.FirstOrDefault();

        return new Opinion
        {
            VideoId = video.Id,
            CommentatorId = commentator?.Id,
            SpeakerLabel = Truncate(speakerLabel, SpeakerLabelMaxLength),
            SpeakerSource = source,
            AttributionConfidence = confidence,
            TeamId = team?.Id,

            SportId = ResolveSportId(item.Sport, team, video, sportIdsBySlug),
            StoryId = MatchStory(teams, clusterRefs),
            Topic = topic,
            Summary = summary,
            Quote = quote,
            TimestampSeconds = ParseTimestamp(item.Timestamp, video.DurationSeconds),
            Stance = ParseStance(item.Stance),
            Prediction = Truncate(item.Prediction, PredictionMaxLength),
            IsQuoteVerified = false,
            Status = otomatik ? OpinionStatus.Approved : OpinionStatus.Pending,
            ReviewedAt = otomatik ? timeProvider.GetUtcNow() : null,
            ReviewNote = otomatik
                ? $"{AutoReviewNotePrefix} ({source}, güven {confidence.ToString("0.00", CultureInfo.InvariantCulture)})"
                : null
        };
    }

    /// <summary>
    /// Görüşün branşı. Sıra önemli:
    ///
    /// 1. Modelin söylediği — videoyu izleyen o. Yalnızca bizim listemizde olan bir
    ///    adres kabul ediliyor, uydurma değer atılıyor.
    /// 2. Takımın branşı — model boş bıraktıysa. Zayıf bir kaynak: Fenerbahçe'nin
    ///    hem futbol hem basketbol takımı var ama kayıtta tek branş duruyor.
    /// 3. Kanalın tek branşı varsa o. Birden fazlaysa seçim yapmıyoruz; yanlış
    ///    tahmin etmektense branşsız bırakmak daha iyi.
    /// </summary>
    private static Guid? ResolveSportId(
        string? modelSport,
        MatchedTeam? team,
        Video video,
        Dictionary<string, Guid> sportIdsBySlug)
    {
        if (!string.IsNullOrWhiteSpace(modelSport)
            && sportIdsBySlug.TryGetValue(modelSport.Trim(), out var fromModel))
            return fromModel;

        if (team is not null)
            return team.SportId;

        var channelSports = video.Channel.Sports;

        return channelSports.Count == 1 ? channelSports.First().Id : null;
    }

    /// <summary>
    /// Sözlükte karşılığı bulunan isme modelin kendi güveninden bağımsız olarak
    /// taban bir güven veriyoruz; bulunmayanın güveni modelin söylediğinden yukarı çıkmıyor.
    /// </summary>
    /// <summary>
    /// Sözlükte olmayan konuşmacıyı kaydeder ve kanalın kadrosuna ekler. Yeni kayıt
    /// doğrulanmamış (IsVerified = false) giriyor: görüşü yayına çıkabilir ama bir
    /// sonraki videoda kadro güveni kazanmaz, önce admin onaylamalı.
    ///
    /// Ad makul bir kişi adına benzemiyorsa (unvan, rakam, tek kelime) kayıt açılmıyor;
    /// görüş isimsiz kalıp onay ekranına düşüyor.
    /// </summary>
    private async Task<Commentator?> EnsureCommentatorAsync(
        string? rawName,
        Video video,
        Dictionary<string, Commentator> newCommentators,
        CancellationToken cancellationToken)
    {
        var name = CleanPersonName(rawName);

        if (name is null)
            return null;

        var slug = TextNormalizer.Slugify(name, SpeakerLabelMaxLength);

        if (string.IsNullOrEmpty(slug))
            return null;

        if (newCommentators.TryGetValue(slug, out var pending))
            return pending;

        // Başlıkta geçen isim çoğu zaman haberin KONUSUDUR: sporcu ya da takım.
        // Sözlüğe böyle bir isim girerse hem yanlış atıf üretiyor hem de kanal
        // kadrosunu kirletiyor. Sporcu/takım kayıtlarıyla eşleşeni hiç almıyoruz.
        if (await IsSportsSubjectAsync(slug, cancellationToken))
        {
            logger.LogInformation(
                "İsim sözlüğe alınmadı, sporcu/takım kaydıyla eşleşti: {Name} — {VideoTitle}",
                name,
                video.Title);

            return null;
        }

        // Sözlükte pasif ya da aday listesine girmemiş bir kayıt olabilir.
        var existing = await commentatorRepository.GetBySlugAsync(slug, cancellationToken);

        if (existing is null)
        {
            existing = new Commentator
            {
                FullName = name,
                Slug = slug,
                IsVerified = false
            };

            await commentatorRepository.AddAsync(existing, cancellationToken);

            logger.LogInformation(
                "Sözlüğe yeni konuşmacı eklendi (doğrulanmamış): {Name} — {Channel} / {VideoTitle}",
                name,
                video.Channel.Name,
                video.Title);
        }

        if (video.Channel.RegularCommentators.All(x => x.Id != existing.Id))
            video.Channel.RegularCommentators.Add(existing);

        newCommentators[slug] = existing;
        return existing;
    }

    /// <summary>
    /// İsim bir sporcu ya da takım mı. Her ikisinin de kısa adı aynı
    /// <see cref="TextNormalizer.Slugify"/> ile üretildiği için kısa ad üzerinden
    /// karşılaştırıyoruz; isim normalleştirme farklarına takılmıyor.
    /// </summary>
    private async Task<bool> IsSportsSubjectAsync(string slug, CancellationToken cancellationToken)
    {
        if (await sportsReadRepository.GetAthleteBySlugAsync(slug, cancellationToken) is not null)
            return true;

        return await sportsReadRepository.GetTeamBySlugAsync(slug, cancellationToken) is not null;
    }

    /// <summary>
    /// Modelin söylediği her metin kişi adı değil: "Spor Yorumcusu", "Sunucu",
    /// "Bölüm 3" gibi şeyler geliyor. Sözlüğe yalnızca ad-soyad biçimindekileri alıyoruz.
    /// </summary>
    private static string? CleanPersonName(string? value)
    {
        var name = value?.Trim();

        if (string.IsNullOrEmpty(name) || name.Length is < MinNewNameLength or > MaxNewNameLength)
            return null;

        if (name.Any(char.IsDigit))
            return null;

        var words = name.Split(' ', StringSplitOptions.RemoveEmptyEntries);

        // Tek kelimelik ad ("Rıdvan", "Hoca") yanlış kişiye bağlanmaya çok açık.
        if (words.Length is < 2 or > 4)
            return null;

        // Her parça harfle başlamalı: unvan kısaltmaları ve etiketler elensin.
        return words.All(x => char.IsLetter(x[0])) ? name : null;
    }

    /// <summary>Kaynak adını sabit bir kümeye indiriyoruz; model serbest metin yazabiliyor.</summary>
    private static string NormalizeSource(string? value) =>
        value?.Trim().ToLowerInvariant() switch
        {
            "altbant" or "alt bant" or "ekran" or "lowerthird" or "lower third" => "altbant",
            "baslik" or "başlık" or "title" => "baslik",
            "hitap" or "address" => "hitap",
            "aciklama" or "açıklama" or "description" => "aciklama",
            _ => "tahmin"
        };

    /// <summary>
    /// Güven, ismin nereden geldiğine göre veriliyor. Yazılı kanıt (alt bant, başlık)
    /// en üstte; kulaktan duyma hitap ortada; tahmin ve konuk listesi en altta.
    /// </summary>
    private static double ResolveConfidence(
        Commentator? commentator,
        string source,
        bool isChannelRoster,
        double? modelConfidence)
    {
        var reported = Math.Clamp(modelConfidence ?? 0.3, 0, 1);

        if (commentator is null)
            return Math.Min(reported, 0.4);

        return source switch
        {
            // Yayıncının konuşanın yanına yazdığı isim: en sağlam kanıt.
            "altbant" => 0.95,

            // Başlıktaki isim, kişi kanalın doğrulanmış kadrosundaysa neredeyse kesin;
            // kadroda değilse haberin konusu olma ihtimali var ama yine de yazılı.
            "baslik" => isChannelRoster ? 0.9 : 0.75,

            // Kulaktan duyma: "Buyurun Ahmet Bey". Kadroda bile olsa onay istiyoruz.
            "hitap" => isChannelRoster ? 0.7 : 0.55,

            _ => Math.Min(reported, 0.4)
        };
    }

    /// <summary>
    /// Görüşü habere bağlar: kümeleme anahtarındaki takım slug'ı görüşteki takımla
    /// aynıysa en güncel haber seçilir.
    /// </summary>
    private static Guid? MatchStory(List<MatchedTeam> teams, List<StoryClusterRef> clusterRefs)
    {
        if (teams.Count == 0 || clusterRefs.Count == 0)
            return null;

        var wanted = teams
            .Select(x => (Sport: x.SportSlug, Slug: x.Slug))
            .ToHashSet();

        // clusterRefs en yeniden eskiye geliyor; ilk eşleşme en güncel haber.
        foreach (var reference in clusterRefs)
        {
            var parts = reference.ClusterKey.Split('|');
            if (parts.Length < 3)
                continue;

            var sport = parts[0];

            foreach (var entity in parts[2].Split('+', StringSplitOptions.RemoveEmptyEntries))
            {
                if (wanted.Contains((sport, entity)))
                    return reference.Id;
            }
        }

        return null;
    }

    private readonly record struct VideoSegment(int? Start, int? End);

    /// <summary>
    /// Videoyu hangi aralıklarda soracağımızı planlar. Eşiğin altındaki video tek
    /// parça kalıyor (aralık verilmiyor, model videonun tamamını görüyor).
    ///
    /// Uzun videoda model 40 dakikayı takip etmeye çalışırken zamanı kaçırıyor;
    /// 8 dakikalık bir bölümde aynı sorun yok. Gemini aralığı orantılı kırptığı için
    /// toplam token maliyeti videoyu bir kez işlemekle hemen hemen aynı kalıyor.
    /// </summary>
    private static List<VideoSegment> PlanSegments(
        int? durationSeconds,
        int thresholdMinutes,
        int segmentMinutes,
        int overlapSeconds,
        int maxSegments)
    {
        var tek = new List<VideoSegment> { new(null, null) };

        if (durationSeconds is not > 0 || segmentMinutes <= 0)
            return tek;

        var esik = Math.Max(1, thresholdMinutes) * 60;
        if (durationSeconds <= esik)
            return tek;

        var uzunluk = segmentMinutes * 60;

        // Üst sınır: iki saatlik bir yayın 8 dakikalık dilimlerle on beş çağrı demek,
        // günlük kota tek videoya gidiyor. Sınırı aşan videoda dilimi uzatıyoruz —
        // videonun sonunu kesmek yerine zaman damgasından biraz feragat ediyoruz.
        var sinir = Math.Max(1, maxSegments);
        if (durationSeconds > uzunluk * (long)sinir)
            uzunluk = (int)Math.Ceiling(durationSeconds.Value / (double)sinir);
        var bindirme = Math.Clamp(overlapSeconds, 0, uzunluk / 2);

        var segments = new List<VideoSegment>();

        for (var start = 0; start < durationSeconds; start += uzunluk)
        {
            // Bindirme geriye doğru: dilim sınırına denk gelen cümle ikinci dilimde
            // baştan duyulsun. İlk dilimde geriye gidecek yer yok.
            var from = start == 0 ? 0 : start - bindirme;
            var to = Math.Min(start + uzunluk, durationSeconds.Value);

            segments.Add(new VideoSegment(from, to));

            if (to >= durationSeconds)
                break;
        }

        if (segments.Count == 0)
            return tek;

        // Son dilim bir tutamsa (süre dilim boyuna tam bölünmediğinde oluyor) ayrı
        // çağrı yapmaya değmez: onu bir öncekine ekliyoruz. Yoksa 40:10'luk videoda
        // son dilim 10 saniye oluyor ve modele boşluk soruyoruz.
        var sonuncu = segments[^1];
        if (segments.Count > 1 && sonuncu.End - sonuncu.Start < uzunluk / 4)
        {
            var onceki = segments[^2];
            segments.RemoveAt(segments.Count - 1);
            segments[^1] = new VideoSegment(onceki.Start, sonuncu.End);
        }

        return segments;
    }

    /// <summary>
    /// Dilimden gelen zamanı mutlak ana çevirir. Modelden bölümün başından itibaren
    /// saymasını istiyoruz; toplamayı burada yapmak, ondan kafasında hesap yapmasını
    /// istemekten güvenli.
    /// </summary>
    private static AiVideoOpinion ShiftTimestamp(AiVideoOpinion item, int? segmentStart)
    {
        if (segmentStart is not > 0 || string.IsNullOrWhiteSpace(item.Timestamp))
            return item;

        var relative = ParseTimestamp(item.Timestamp, null);

        return relative is null
            ? item
            : item with { Timestamp = (relative.Value + segmentStart.Value).ToString(CultureInfo.InvariantCulture) };
    }

    /// <summary>
    /// Dilimlerden toplanan görüşleri sıralayıp video başına sınırlar.
    ///
    /// Sıralama ölçütü önem × konuşmacı güveni: hem çarpıcı hem kime ait olduğu
    /// sağlam olan öne geçiyor. Sınır olmadan beş dilimli bir yayından on beş görüş
    /// çıkıyor ve akış tek videoyla doluyor.
    /// </summary>
    private static List<AiVideoOpinion> RankAndTrim(List<AiVideoOpinion> items, int maxPerVideo)
    {
        var limit = Math.Max(1, maxPerVideo);
        var secilen = new List<AiVideoOpinion>();

        var sirali = items
            .Where(x => !string.IsNullOrWhiteSpace(x.Quote))
            .OrderByDescending(x => (x.Importance ?? 0.5) * (x.SpeakerConfidence ?? 0.5))
            .ToList();

        foreach (var item in sirali)
        {
            if (secilen.Count >= limit)
                break;

            // Bindirme yüzünden aynı söz iki dilimden gelebiliyor; aynı kişinin
            // başlangıcı aynı olan alıntısını bir kez alıyoruz.
            if (secilen.Any(x => AyniAlinti(x, item)))
                continue;

            secilen.Add(item);
        }

        return secilen;
    }

    /// <summary>Bindirmeden gelen kopyaları ayıklamak için: aynı konuşmacının aynı sözü mü.</summary>
    private static bool AyniAlinti(AiVideoOpinion a, AiVideoOpinion b)
    {
        if (!string.Equals(
                TextNormalizer.Slugify(a.Speaker),
                TextNormalizer.Slugify(b.Speaker),
                StringComparison.Ordinal))
            return false;

        var ilk = TextNormalizer.Slugify(a.Quote);
        var ikinci = TextNormalizer.Slugify(b.Quote);

        // Alıntının tamamı birebir eşleşmiyor: model aynı sözü iki dilimde farklı
        // yerden kesiyor ("...şampiyon olur" / "...şampiyon olur ama Mourinho").
        // Bu yüzden kısa olan uzun olanın başlangıcı mı diye bakıyoruz. Alt sınır,
        // "evet aynen" gibi kısa onaylamaların birbirine karışmasını engelliyor.
        var kisa = ilk.Length <= ikinci.Length ? ilk : ikinci;
        var uzun = ilk.Length <= ikinci.Length ? ikinci : ilk;

        return kisa.Length >= QuoteFingerprintLength
               && uzun.StartsWith(kisa, StringComparison.Ordinal);
    }

    /// <summary>
    /// "12:18", "1:02:18" ve ham saniye biçimlerini saniyeye çevirir.
    ///
    /// Çevrilemeyen ya da video süresini aşan değer için NULL dönüyor, 0 değil.
    /// Eskiden 0 dönüyordu ve bu, bilinmeyen anı ekranda "0:00" diye geçerli görünen
    /// bir bilgiye çeviriyordu; kullanıcı videoyu baştan açıp sözü bulamıyordu.
    /// Modelden de emin olmadığında alanı boş bırakması isteniyor.
    /// </summary>
    private static int? ParseTimestamp(string? value, int? durationSeconds)
    {
        if (string.IsNullOrWhiteSpace(value))
            return null;

        var trimmed = value.Trim();
        var seconds = 0;

        if (trimmed.Contains(':'))
        {
            foreach (var part in trimmed.Split(':'))
            {
                if (!int.TryParse(part, CultureInfo.InvariantCulture, out var number) || number < 0)
                    return null;

                seconds = seconds * 60 + number;
            }
        }
        else if (!int.TryParse(trimmed, CultureInfo.InvariantCulture, out seconds) || seconds < 0)
        {
            return null;
        }

        // Süreyi aşan an uydurmadır; sıfıra çekmek yerine bilinmiyor sayıyoruz.
        if (durationSeconds is > 0 && seconds > durationSeconds)
            return null;

        return seconds;
    }

    private static Stance ParseStance(string? value) =>
        value?.Trim().ToLowerInvariant() switch
        {
            "olumlu" or "positive" => Stance.Positive,
            "olumsuz" or "negative" => Stance.Negative,
            _ => Stance.Neutral
        };

    private async Task<Result<int>> FailAsync(
        Video video,
        string message,
        HttpStatusCode statusCode,
        CancellationToken cancellationToken)
    {
        video.ProcessingError = message;
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return Result<int>.Fail(message, statusCode);
    }

    private static string? Truncate(string? value, int maxLength)
    {
        if (string.IsNullOrWhiteSpace(value))
            return null;

        var trimmed = value.Trim();
        return trimmed.Length <= maxLength ? trimmed : trimmed[..maxLength];
    }
}

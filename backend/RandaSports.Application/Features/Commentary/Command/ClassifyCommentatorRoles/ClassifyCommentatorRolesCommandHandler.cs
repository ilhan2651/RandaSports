using MediatR;
using Microsoft.Extensions.Logging;
using RandaSports.Application.Common.Wrappers;
using RandaSports.Application.Interfaces;
using RandaSports.Application.Interfaces.Repositories;
using RandaSports.Application.Interfaces.Services;
using RandaSports.Domain.Enums;

namespace RandaSports.Application.Features.Commentary.Command.ClassifyCommentatorRoles;

/// <summary>
/// Sözlükteki kişilerin rolünü belirler: yorumcu olanları doğrulanmış kadroya alır,
/// olmayanları kadrodan düşürür.
///
/// Neden gerekli: otomatik doğrulama yalnızca tekrarı sayıyordu ve bu, "gerçek bir
/// insan mı, kamerada konuşuyor mu" sorusunu ölçüyor — "bu kişi yorumcu mu"
/// sorusunu değil. Basın toplantısı veren bir teknik direktör her eşiği geçiyordu.
/// Doğrulamayı geri almak önemli: doğrulanmış isim kanal kadrosu güveni kazanıyor
/// ve sonraki videolarda yanlış atıfları güçlendiriyor.
/// </summary>
public sealed class ClassifyCommentatorRolesCommandHandler(
    ICommentatorRepository commentatorRepository,
    IPersonRoleClassifier classifier,
    IUnitOfWork unitOfWork,
    ILogger<ClassifyCommentatorRolesCommandHandler> logger)
    : IRequestHandler<ClassifyCommentatorRolesCommand, Result<int>>
{
    /// <summary>Bu güvenin altındaki karara göre işlem yapmıyoruz, tekrar soruyoruz.</summary>
    private const double MinConfidence = 0.6;

    /// <summary>
    /// Kadroya ALMAK için aranan güven. İndirme eşiğinden kasıtlı olarak yüksek.
    ///
    /// Asimetrinin sebebi: yanlış doğrulamanın bedeli yanlış bırakmanınkinden ağır.
    /// Doğrulanmış bir isim sonraki videolarda kanal kadrosu güveni kazanıyor, bu da
    /// başlıktan gelen atıfı 0.75'ten 0.90'a çıkarıp hatayı yayına taşıyor. Doğrulamayı
    /// geciktirmenin bedeli ise yalnızca birkaç görüşün onay kuyruğunda beklemesi.
    /// </summary>
    private const double VerifyConfidence = 0.85;

    public async Task<Result<int>> Handle(
        ClassifyCommentatorRolesCommand request,
        CancellationToken cancellationToken)
    {
        var candidates = await commentatorRepository.GetRoleCandidatesAsync(
            request.Take,
            cancellationToken);

        if (candidates.Count == 0)
            return Result<int>.Ok(0, "Rolü sorulacak kişi yok.");

        var verdicts = await classifier.ClassifyAsync(
            [.. candidates.Select(x => new PersonRoleCandidate(
                x.FullName,
                x.VideoTitles,
                x.Quotes,
                x.Channels))],
            cancellationToken);

        if (verdicts.Count == 0)
            return Result<int>.Ok(0, "Rol tespiti sonuç vermedi.");

        var karara_baglanan = 0;

        foreach (var verdict in verdicts)
        {
            if (verdict.Role == PersonRole.Unknown || verdict.Confidence < MinConfidence)
                continue;

            // Karar, sorduğumuz adayın adıyla dönüyor; yine de eşleşmezse atlıyoruz.
            var aday = candidates.FirstOrDefault(x => x.FullName == verdict.FullName);

            if (aday is null)
                continue;

            var commentator = await commentatorRepository.GetForReviewAsync(aday.Id, cancellationToken);

            if (commentator is null)
                continue;

            commentator.PersonRole = verdict.Role;
            commentator.RoleConfidence = verdict.Confidence;
            commentator.RoleNote = Kirp(verdict.Reason, 300);

            // Yorumcu olmayan kişi kadro güveni kazanmamalı. Kaydı silmiyoruz:
            // söyledikleri yanlış atfedilmiş değil, yalnızca yorum değil demeç.
            if (verdict.Role != PersonRole.Commentator && commentator.IsVerified)
            {
                commentator.IsVerified = false;

                logger.LogInformation(
                    "Doğrulama geri alındı, {Role} olarak belirlendi: {Name} — {Reason}",
                    verdict.Role,
                    commentator.FullName,
                    verdict.Reason);
            }
            // Yorumcu olduğu yüksek güvenle belirlenen kişi kadroya giriyor. Bu dal
            // olmadığı için sınıflandırma doğru çalışıp da kimse doğrulanmıyordu:
            // kişiler "Commentator" etiketi alıp doğrulanmamış kalıyor, görüşleri de
            // kadro güveni kazanamadığı için gereksiz yere onay kuyruğuna düşüyordu.
            else if (verdict.Role == PersonRole.Commentator
                     && !commentator.IsVerified
                     && verdict.Confidence >= VerifyConfidence)
            {
                commentator.IsVerified = true;

                logger.LogInformation(
                    "Kadroya alındı, yorumcu olarak doğrulandı: {Name} ({Confidence}) — {Reason}",
                    commentator.FullName,
                    verdict.Confidence,
                    verdict.Reason);
            }

            karara_baglanan++;
        }

        if (karara_baglanan > 0)
            await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result<int>.Ok(karara_baglanan, $"{karara_baglanan} kişinin rolü belirlendi.");
    }

    private static string? Kirp(string? value, int max) =>
        string.IsNullOrWhiteSpace(value) ? null : value.Length <= max ? value : value[..max];
}

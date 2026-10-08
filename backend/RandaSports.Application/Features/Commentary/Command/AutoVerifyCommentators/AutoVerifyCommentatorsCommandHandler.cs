using MediatR;
using Microsoft.Extensions.Logging;
using RandaSports.Application.Common.Wrappers;
using RandaSports.Application.Interfaces;
using RandaSports.Application.Interfaces.Repositories;
using RandaSports.Domain.Enums;

namespace RandaSports.Application.Features.Commentary.Command.AutoVerifyCommentators;

/// <summary>
/// Kişi onayı kuyruğunu insana sormadan boşaltıyor. Üç kanıttan biri yeterli:
/// yayıncının ekrana bastığı isim iki ayrı videoda tekrarlıyorsa, isim iki ayrı
/// kanalda geçiyorsa, ya da aynı kanalda üç ayrı videoda çıkıyorsa.
///
/// Tek videoda bir kez geçen isme dokunmuyoruz: yanlış eklenmiş bir ad genelde
/// tam olarak böyle görünüyor, kanıt birikene kadar doğrulanmamış kalsın.
/// Doğrulanmamış olmak yayını engellemiyor, yalnızca kanal kadrosu güvenini vermiyor.
/// </summary>
public sealed class AutoVerifyCommentatorsCommandHandler(
    ICommentatorRepository commentatorRepository,
    IUnitOfWork unitOfWork,
    ILogger<AutoVerifyCommentatorsCommandHandler> logger)
    : IRequestHandler<AutoVerifyCommentatorsCommand, Result<int>>
{
    private const int MinAltbantVideos = 2;
    private const int MinChannels = 2;
    private const int MinVideos = 3;

    public async Task<Result<int>> Handle(
        AutoVerifyCommentatorsCommand request,
        CancellationToken cancellationToken)
    {
        var evidence = await commentatorRepository.GetVerificationEvidenceAsync(cancellationToken);
        var yeterli = evidence.Where(Kanitli).ToList();

        if (yeterli.Count == 0)
            return Result<int>.Ok(0, "Doğrulanacak isim yok.");

        var verified = 0;

        foreach (var row in yeterli)
        {
            var commentator = await commentatorRepository.GetForReviewAsync(row.Id, cancellationToken);

            if (commentator is null || commentator.IsVerified)
                continue;

            // Rol şartı: kanıt ne kadar güçlü olursa olsun, yorumcu olmayan kişi
            // doğrulanmıyor. Rolü henüz belirlenmemiş kişiyi de bekletiyoruz —
            // sınıflandırma turu geçsin, sonraki turda tekrar bakılır.
            if (commentator.PersonRole != PersonRole.Commentator)
                continue;

            commentator.IsVerified = true;
            verified++;

            logger.LogInformation(
                "Yorumcu otomatik doğrulandı: {Name} ({Altbant} altbant, {Channels} kanal, {Videos} video)",
                row.FullName,
                row.AltbantVideos,
                row.DistinctChannels,
                row.DistinctVideos);
        }

        if (verified > 0)
            await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result<int>.Ok(verified, $"{verified} isim otomatik doğrulandı.");
    }

    private static bool Kanitli(CommentatorEvidence x) =>
        x.AltbantVideos >= MinAltbantVideos
        || x.DistinctChannels >= MinChannels
        || x.DistinctVideos >= MinVideos;
}

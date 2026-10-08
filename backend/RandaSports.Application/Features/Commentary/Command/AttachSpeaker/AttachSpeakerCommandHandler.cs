using System.Net;
using MediatR;
using Microsoft.Extensions.Logging;
using RandaSports.Application.Common.Text;
using RandaSports.Application.Common.Wrappers;
using RandaSports.Application.Interfaces;
using RandaSports.Application.Interfaces.Repositories;
using RandaSports.Domain.Entities;

namespace RandaSports.Application.Features.Commentary.Command.AttachSpeaker;

public sealed class AttachSpeakerCommandHandler(
    IOpinionRepository opinionRepository,
    ICommentatorRepository commentatorRepository,
    IUnitOfWork unitOfWork,
    ILogger<AttachSpeakerCommandHandler> logger)
    : IRequestHandler<AttachSpeakerCommand, Result<bool>>
{
    private const int NameMaxLength = 150;

    /// <summary>Bu süreye kadar olan videolar tek konuşmacılı klip sayılıyor.</summary>

    public async Task<Result<bool>> Handle(AttachSpeakerCommand request, CancellationToken cancellationToken)
    {
        var opinion = await opinionRepository.GetWithContextAsync(request.OpinionId, cancellationToken);

        if (opinion is null)
            return Result<bool>.Fail("Görüş bulunamadı.", HttpStatusCode.NotFound);

        var name = (request.FullName ?? opinion.SpeakerLabel)?.Trim();

        if (string.IsNullOrEmpty(name))
            return Result<bool>.Fail("Konuşmacı adı yok; elle girilmesi gerekiyor.");

        if (name.Length > NameMaxLength)
            name = name[..NameMaxLength];

        var slug = TextNormalizer.Slugify(name, NameMaxLength);

        if (string.IsNullOrEmpty(slug))
            return Result<bool>.Fail("Konuşmacı adından adres üretilemedi.");

        var commentator = await commentatorRepository.GetBySlugAsync(slug, cancellationToken);
        var created = false;

        if (commentator is null)
        {
            commentator = new Commentator { FullName = name, Slug = slug, IsVerified = true };
            await commentatorRepository.AddAsync(commentator, cancellationToken);
            created = true;
        }

        // Videodan otomatik eklenmiş olabilir; onayı sen verdiğine göre artık doğrulanmış.
        commentator.IsVerified = true;

        opinion.CommentatorId = commentator.Id;
        opinion.SpeakerLabel = name;

        // İsmi insan onayladı; modelin kendi güveni artık belirleyici değil.
        opinion.AttributionConfidence = 1;

        // Kanalın kadrosuna da ekliyoruz: aynı kanalın sonraki videolarında bu isim
        // modele aday olarak veriliyor, eşleşme ilk seferde tutuyor.
        var channel = opinion.Video.Channel;

        if (channel.RegularCommentators.All(x => x.Id != commentator.Id))
            channel.RegularCommentators.Add(commentator);

        await unitOfWork.SaveChangesAsync(cancellationToken);

        logger.LogInformation(
            "Konuşmacı bağlandı: {Name} ({State}) — {Channel}",
            name,
            created ? "sözlüğe eklendi" : "zaten vardı",
            channel.Name);

        return Result<bool>.Ok(true, created ? "Sözlüğe eklendi ve bağlandı." : "Mevcut yorumcuya bağlandı.");
    }

}

using System.Net;
using MediatR;
using Microsoft.Extensions.Logging;
using RandaSports.Application.Common.Text;
using RandaSports.Application.Common.Wrappers;
using RandaSports.Application.Interfaces;
using RandaSports.Application.Interfaces.Repositories;
using RandaSports.Application.Interfaces.Services;
using RandaSports.Domain.Entities;

namespace RandaSports.Application.Features.Commentary.Command.CreateChannel;

public sealed class CreateChannelCommandHandler(
    IChannelRepository channelRepository,
    ISportsReadRepository sportsReadRepository,
    IYouTubeChannelResolver channelResolver,
    IUnitOfWork unitOfWork,
    ILogger<CreateChannelCommandHandler> logger)
    : IRequestHandler<CreateChannelCommand, Result<Guid>>
{
    public async Task<Result<Guid>> Handle(CreateChannelCommand request, CancellationToken cancellationToken)
    {
        var handle = ChannelReference.Normalize(request.Reference);

        if (handle.Length == 0)
            return Result<Guid>.Fail("Kanal adresi anlaşılamadı.");

        if (await channelRepository.GetByHandleAsync(handle, cancellationToken) is not null)
            return Result<Guid>.Fail("Bu kanal zaten kayıtlı.", HttpStatusCode.Conflict);

        var slug = TextNormalizer.Slugify(request.Name, 150);

        if (string.IsNullOrEmpty(slug))
            return Result<Guid>.Fail("Kanal adından adres üretilemedi.");

        if (await channelRepository.GetBySlugAsync(slug, cancellationToken) is not null)
            return Result<Guid>.Fail("Bu adla bir kanal zaten var.", HttpStatusCode.Conflict);

        var channel = new Channel
        {
            Name = request.Name.Trim(),
            Slug = slug,
            Handle = handle
        };

        foreach (var sportSlug in request.SportSlugs ?? [])
        {
            var sport = await sportsReadRepository.GetSportBySlugAsync(sportSlug, cancellationToken);

            if (sport is null)
                return Result<Guid>.Fail($"Branş bulunamadı: {sportSlug}");

            channel.Sports.Add(sport);
        }

        // Kimliği şimdi çözebilirsek kanal ilk taramayı beklemeden hazır oluyor.
        var resolved = await channelResolver.ResolveChannelIdAsync(handle, cancellationToken);

        if (string.IsNullOrWhiteSpace(resolved))
            channel.LastError = "Kanal kimliği henüz çözülemedi, ilk taramada yeniden denenecek.";
        else
            channel.YouTubeChannelId = resolved;

        await channelRepository.AddAsync(channel, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        logger.LogInformation("Kanal eklendi: {Name} ({Handle})", channel.Name, channel.Handle);

        return Result<Guid>.Created(channel.Id, $"/api/commentary/channels/{channel.Id}");
    }
}

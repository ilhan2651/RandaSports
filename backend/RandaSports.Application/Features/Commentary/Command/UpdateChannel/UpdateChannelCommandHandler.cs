using System.Net;
using MediatR;
using RandaSports.Application.Common.Text;
using RandaSports.Application.Common.Wrappers;
using RandaSports.Application.Interfaces;
using RandaSports.Application.Interfaces.Repositories;
using RandaSports.Domain.Entities;

namespace RandaSports.Application.Features.Commentary.Command.UpdateChannel;

public sealed class UpdateChannelCommandHandler(
    IChannelRepository channelRepository,
    ISportsReadRepository sportsReadRepository,
    IUnitOfWork unitOfWork)
    : IRequestHandler<UpdateChannelCommand, Result<bool>>
{
    public async Task<Result<bool>> Handle(UpdateChannelCommand request, CancellationToken cancellationToken)
    {
        var channel = await channelRepository.GetByIdAsync(request.ChannelId, cancellationToken);

        if (channel is null)
            return Result<bool>.Fail("Kanal bulunamadı.", HttpStatusCode.NotFound);

        if (!string.IsNullOrWhiteSpace(request.Name))
            channel.Name = request.Name.Trim();

        if (!string.IsNullOrWhiteSpace(request.Reference))
        {
            var handle = ChannelReference.Normalize(request.Reference);

            if (handle.Length == 0)
                return Result<bool>.Fail("Kanal adresi anlaşılamadı.");

            if (!string.Equals(handle, channel.Handle, StringComparison.Ordinal))
            {
                var other = await channelRepository.GetByHandleAsync(handle, cancellationToken);

                if (other is not null && other.Id != channel.Id)
                    return Result<bool>.Fail("Bu kanal zaten kayıtlı.", HttpStatusCode.Conflict);

                channel.Handle = handle;

                // Referans değiştiyse eski kimlik geçersiz: ilk taramada yeniden çözülecek.
                channel.YouTubeChannelId = null;
                channel.LastError = null;
            }
        }

        if (request.IsActive is not null)
            channel.IsActive = request.IsActive.Value;

        // Gönderilen liste mevcut seçimin yerine geçiyor; boş liste etiketleri siliyor.
        // null ise dokunulmuyor.
        if (request.SportSlugs is not null)
        {
            var secilen = new List<Sport>();

            foreach (var sportSlug in request.SportSlugs)
            {
                var sport = await sportsReadRepository.GetSportBySlugAsync(sportSlug, cancellationToken);

                if (sport is null)
                    return Result<bool>.Fail($"Branş bulunamadı: {sportSlug}");

                secilen.Add(sport);
            }

            channel.Sports.Clear();

            foreach (var sport in secilen)
                channel.Sports.Add(sport);
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result<bool>.Ok(true);
    }
}

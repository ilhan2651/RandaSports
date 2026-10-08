using MediatR;
using RandaSports.Application.Common.Wrappers;

namespace RandaSports.Application.Features.Commentary.Command.DiscoverChannelVideos;

/// <param name="MaxAgeDays">Bu günden eski videolar alınmıyor.</param>
/// <param name="MaxPerRun">Bir taramada en fazla kaç yeni video eklenecek.</param>
/// <param name="MinDurationSeconds">Bundan kısa videolar alınmıyor (jenerik, fragman).</param>
/// <param name="MaxDurationSeconds">Bundan uzun videolar alınmıyor; model kotasını tek başına bitiriyorlar.</param>
/// <param name="SkipLive">Devam eden yayınları atla — model yarım yayını düzgün izleyemiyor.</param>
public sealed record DiscoverChannelVideosCommand(
    Guid ChannelId,
    int MaxAgeDays = 3,
    int MaxPerRun = 5,
    int MinDurationSeconds = 20,
    int MaxDurationSeconds = 3600,
    bool SkipLive = true) : IRequest<Result<int>>;

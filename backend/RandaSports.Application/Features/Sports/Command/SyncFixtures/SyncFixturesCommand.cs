using MediatR;
using RandaSports.Application.Common.Wrappers;

namespace RandaSports.Application.Features.Sports.Command.SyncFixtures;

/// <summary>Geçmiş sonuçları ve yaklaşan maçları sağlayıcıdan çekip günceller.</summary>
public sealed record SyncFixturesCommand(
    string LeagueExternalId,
    int Season,
    int DaysBack = 7,
    int DaysAhead = 14) : IRequest<Result<FixtureSyncResult>>;

public sealed record FixtureSyncResult(int Added, int Updated);

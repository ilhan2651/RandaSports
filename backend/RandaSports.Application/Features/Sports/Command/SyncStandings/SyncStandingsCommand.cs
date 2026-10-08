using MediatR;
using RandaSports.Application.Common.Wrappers;

namespace RandaSports.Application.Features.Sports.Command.SyncStandings;

/// <summary>Ligin puan durumunu çeker ve mevcut satırları günceller.</summary>
public sealed record SyncStandingsCommand(string LeagueExternalId, int Season) : IRequest<Result<int>>;

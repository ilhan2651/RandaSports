using MediatR;
using RandaSports.Application.Common.Wrappers;

namespace RandaSports.Application.Features.Sports.Command.SyncLeagueReference;

/// <summary>Ligi, sezonu ve takımları sağlayıcıdan çekip kendi kayıtlarımızla eşler.</summary>
public sealed record SyncLeagueReferenceCommand(string LeagueExternalId, int Season)
    : IRequest<Result<LeagueSyncResult>>;

/// <param name="Matched">Sözlükteki takımla eşleşen sağlayıcı takımı.</param>
/// <param name="Created">Sözlükte olmadığı için yeni açılan takım.</param>
public sealed record LeagueSyncResult(string CompetitionName, int Matched, int Created);

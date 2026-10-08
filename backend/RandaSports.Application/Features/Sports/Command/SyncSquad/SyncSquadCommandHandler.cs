using System.Net;
using MediatR;
using Microsoft.Extensions.Logging;
using RandaSports.Application.Common.Text;
using RandaSports.Application.Common.Wrappers;
using RandaSports.Application.Interfaces;
using RandaSports.Application.Interfaces.Repositories;
using RandaSports.Application.Interfaces.Services;
using RandaSports.Domain.Entities;

namespace RandaSports.Application.Features.Sports.Command.SyncSquad;

public sealed class SyncSquadCommandHandler(
    ISportsDataProvider provider,
    ISportsRepository sportsRepository,
    IUnitOfWork unitOfWork,
    TimeProvider timeProvider,
    ILogger<SyncSquadCommandHandler> logger)
    : IRequestHandler<SyncSquadCommand, Result<int>>
{
    public async Task<Result<int>> Handle(SyncSquadCommand request, CancellationToken cancellationToken)
    {
        var team = await sportsRepository.GetTeamAsync(request.TeamId, cancellationToken);
        if (team is null)
            return Result<int>.Fail("Takım bulunamadı.", HttpStatusCode.NotFound);

        if (string.IsNullOrWhiteSpace(team.ExternalId))
            return Result<int>.Fail("Takımın sağlayıcı kimliği yok.", HttpStatusCode.BadRequest);

        var squad = await provider.GetSquadAsync(team.ExternalId, cancellationToken);

        // Boş cevap kota ya da sağlayıcı hatası olabilir; damgayı basmıyoruz ki
        // takım sıradaki turda tekrar denensin.
        if (squad.Count == 0)
            return Result<int>.Ok(0, "Kadro alınamadı.");

        var externalIds = squad.Select(x => x.ExternalId).ToList();

        var byExternalId = (await sportsRepository.GetAthletesByExternalIdsAsync(externalIds, cancellationToken))
            .Where(x => x.ExternalId is not null)
            .ToDictionary(x => x.ExternalId!, StringComparer.Ordinal);

        var slugs = await sportsRepository.GetAthleteSlugsAsync(team.SportId, cancellationToken);

        var added = 0;

        foreach (var player in squad)
        {
            if (byExternalId.TryGetValue(player.ExternalId, out var athlete))
            {
                Apply(athlete, player, team);
                continue;
            }

            athlete = new Athlete
            {
                SportId = team.SportId,
                FullName = player.FullName,
                Slug = BuildSlug(player, slugs),
                ExternalId = player.ExternalId
            };

            Apply(athlete, player, team);

            await sportsRepository.AddAthleteAsync(athlete, cancellationToken);
            added++;
        }

        team.SquadSyncedAt = timeProvider.GetUtcNow();

        await unitOfWork.SaveChangesAsync(cancellationToken);

        logger.LogInformation(
            "{Team}: {Total} oyuncu geldi, {Added} yeni kayıt.",
            team.Name, squad.Count, added);

        return Result<int>.Ok(added);
    }

    /// <summary>Kadro ucu sadece temel künyeyi veriyor; boy, kilo ve istatistik profil senkronunda geliyor.</summary>
    private static void Apply(Athlete athlete, ProviderPlayer player, Team team)
    {
        athlete.FullName = player.FullName;
        athlete.TeamId = team.Id;
        athlete.Position ??= player.Position;
        athlete.ShirtNumber = player.ShirtNumber ?? athlete.ShirtNumber;
        athlete.PhotoUrl ??= player.PhotoUrl;
    }

    /// <summary>Aynı isimli iki oyuncu çakışmasın diye slug'a sağlayıcı kimliğinden ek koyuyoruz.</summary>
    private static string BuildSlug(ProviderPlayer player, HashSet<string> taken)
    {
        var baseSlug = TextNormalizer.Slugify(player.FullName, 140);
        if (string.IsNullOrEmpty(baseSlug))
            baseSlug = "oyuncu";

        var slug = baseSlug;
        if (taken.Contains(slug))
            slug = $"{baseSlug}-{player.ExternalId}";

        taken.Add(slug);
        return slug;
    }
}

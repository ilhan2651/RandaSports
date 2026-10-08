using System.Net;
using System.Text.Json;
using MediatR;
using Microsoft.Extensions.Logging;
using RandaSports.Application.Common.Wrappers;
using RandaSports.Application.Interfaces;
using RandaSports.Application.Interfaces.Repositories;
using RandaSports.Application.Interfaces.Services;

namespace RandaSports.Application.Features.Sports.Command.SyncAthleteProfile;

public sealed class SyncAthleteProfileCommandHandler(
    ISportsDataProvider provider,
    ISportsRepository sportsRepository,
    IUnitOfWork unitOfWork,
    TimeProvider timeProvider,
    ILogger<SyncAthleteProfileCommandHandler> logger)
    : IRequestHandler<SyncAthleteProfileCommand, Result<bool>>
{
    public async Task<Result<bool>> Handle(
        SyncAthleteProfileCommand request,
        CancellationToken cancellationToken)
    {
        var athlete = await sportsRepository.GetAthleteAsync(request.AthleteId, cancellationToken);
        if (athlete is null)
            return Result<bool>.Fail("Oyuncu bulunamadı.", HttpStatusCode.NotFound);

        if (string.IsNullOrWhiteSpace(athlete.ExternalId))
            return Result<bool>.Fail("Oyuncunun sağlayıcı kimliği yok.", HttpStatusCode.BadRequest);

        var result = await provider.GetPlayerAsync(athlete.ExternalId, request.Season, cancellationToken);

        if (result is null)
        {
            // Bu sezon kaydı olmayan oyuncu kuyruğu tıkamasın diye damgayı yine de basıyoruz.
            athlete.StatsUpdatedAt = timeProvider.GetUtcNow();
            athlete.StatsSeason = request.Season;

            await unitOfWork.SaveChangesAsync(cancellationToken);
            return Result<bool>.Ok(false, "Oyuncu verisi gelmedi.");
        }

        var (player, stats) = result.Value;

        athlete.FullName = player.FullName;
        athlete.Nationality ??= player.Nationality;
        athlete.BirthDate ??= player.BirthDate;
        athlete.BirthPlace ??= player.BirthPlace;
        athlete.HeightCm = player.HeightCm ?? athlete.HeightCm;
        athlete.WeightKg = player.WeightKg ?? athlete.WeightKg;
        athlete.PhotoUrl ??= player.PhotoUrl;

        if (stats is not null)
        {
            athlete.Appearances = stats.Appearances;
            athlete.Goals = stats.Goals;
            athlete.Assists = stats.Assists;
            athlete.MinutesPlayed = stats.Minutes;
            athlete.Rating = stats.Rating;

            // Turnuva kırılımı: kulüp ligi, Avrupa kupası ve millî takım ayrı ayrı duruyor.
            athlete.StatsJson = JsonSerializer.Serialize(stats.Competitions);

            var position = stats.Competitions
                .OrderByDescending(x => x.Minutes)
                .Select(x => x.TeamName)
                .FirstOrDefault();

            logger.LogInformation(
                "{Player}: {Goals} gol / {Assists} asist, {Count} turnuva ({Team}).",
                athlete.FullName, stats.Goals, stats.Assists, stats.Competitions.Count, position);
        }

        athlete.StatsSeason = request.Season;
        athlete.StatsUpdatedAt = timeProvider.GetUtcNow();

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result<bool>.Ok(true);
    }
}

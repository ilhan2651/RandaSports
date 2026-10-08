using System.Net;
using MediatR;
using RandaSports.Application.Common.Wrappers;
using RandaSports.Application.Interfaces;
using RandaSports.Application.Interfaces.Repositories;
using RandaSports.Application.Features.Commentary.Dtos;
using RandaSports.Application.Features.Stories.Dtos;
using RandaSports.Domain.Entities;
using RandaSports.Domain.Enums;

namespace RandaSports.Application.Features.Users;

public sealed class GetMeQueryHandler(IUserRepository userRepository)
    : IRequestHandler<GetMeQuery, Result<MeDto>>
{
    public async Task<Result<MeDto>> Handle(GetMeQuery request, CancellationToken cancellationToken)
    {
        var user = await userRepository.GetWithRolesAsync(request.UserId, cancellationToken);

        if (user is null)
            return Result<MeDto>.Fail("Kullanıcı bulunamadı.", HttpStatusCode.NotFound);

        return Result<MeDto>.Ok(new MeDto(
            user.Id,
            user.Email,
            user.FullName,
            [.. user.UserRoles.Select(x => x.Role.Code)],
            user.CreatedAt,
            user.OnboardingCompletedAt is not null));
    }
}

public sealed class GetPreferencesQueryHandler(IUserRepository userRepository)
    : IRequestHandler<GetPreferencesQuery, Result<PreferencesDto>>
{
    public async Task<Result<PreferencesDto>> Handle(
        GetPreferencesQuery request,
        CancellationToken cancellationToken)
    {
        var user = await userRepository.GetWithPreferencesAsync(request.UserId, cancellationToken);

        return user is null
            ? Result<PreferencesDto>.Fail("Kullanıcı bulunamadı.", HttpStatusCode.NotFound)
            : Result<PreferencesDto>.Ok(PreferenceMapper.ToDto(user));
    }
}

public sealed class UpdatePreferencesCommandHandler(
    IUserRepository userRepository,
    ISportsReadRepository sportsReadRepository,
    ICommentatorRepository commentatorRepository,
    IUnitOfWork unitOfWork,
    TimeProvider timeProvider)
    : IRequestHandler<UpdatePreferencesCommand, Result<PreferencesDto>>
{
    /// <summary>Takip listesi bir ekran dolusu seçim; sınırsız bırakmıyoruz.</summary>
    private const int MaxPerKind = 50;

    public async Task<Result<PreferencesDto>> Handle(
        UpdatePreferencesCommand request,
        CancellationToken cancellationToken)
    {
        if (request.TeamIds.Count > MaxPerKind
            || request.SportIds.Count > MaxPerKind
            || request.CommentatorIds.Count > MaxPerKind)
            return Result<PreferencesDto>.Fail(
                $"Her listede en fazla {MaxPerKind} seçim yapılabilir.",
                HttpStatusCode.BadRequest);

        var user = await userRepository.GetWithPreferencesAsync(request.UserId, cancellationToken);

        if (user is null)
            return Result<PreferencesDto>.Fail("Kullanıcı bulunamadı.", HttpStatusCode.NotFound);

        // Gönderilen kimliklerin gerçekten var olanları alınıyor; uydurulan bir
        // kimlik sessizce düşüyor, hata vermiyoruz çünkü arayüz zaten listeden seçiyor.
        var teams = await sportsReadRepository.GetTeamsByIdsAsync(request.TeamIds, cancellationToken);
        var sports = await sportsReadRepository.GetSportsByIdsAsync(request.SportIds, cancellationToken);
        var people = await commentatorRepository.GetByIdsAsync(request.CommentatorIds, cancellationToken);

        Replace(user.FollowedTeams, teams);
        Replace(user.FollowedSports, sports);
        Replace(user.FollowedCommentators, people);

        if (request.CompleteOnboarding && user.OnboardingCompletedAt is null)
            user.OnboardingCompletedAt = timeProvider.GetUtcNow();

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result<PreferencesDto>.Ok(PreferenceMapper.ToDto(user), "Tercihlerin kaydedildi.");
    }

    private static void Replace<T>(ICollection<T> current, List<T> wanted)
    {
        current.Clear();

        foreach (var item in wanted)
            current.Add(item);
    }
}

internal static class PreferenceMapper
{
    public static PreferencesDto ToDto(User user) =>
        new(
            [.. user.FollowedTeams.Select(x => new FollowedItemDto(x.Id, x.Name, x.Slug, x.LogoUrl))],
            [.. user.FollowedSports.Select(x => new FollowedItemDto(x.Id, x.Name, x.Slug, null))],
            [.. user.FollowedCommentators.Select(x => new FollowedItemDto(x.Id, x.FullName, x.Slug, x.PhotoUrl))]);
}


/// <summary>
/// Sihirbazın üç adımının verisi tek istekte. Branşlar ve yorumcular yalnızca
/// GÖRÜŞÜ OLANLARDAN geliyor: boş bir branş seçtirip sonra "burada içerik yok"
/// demek kullanıcıyı boşa yoruyor.
/// </summary>
public sealed class GetOnboardingOptionsQueryHandler(
    IOpinionRepository opinionRepository,
    ICommentatorRepository commentatorRepository,
    ISportsReadRepository sportsReadRepository)
    : IRequestHandler<GetOnboardingOptionsQuery, Result<OnboardingOptionsDto>>
{
    public async Task<Result<OnboardingOptionsDto>> Handle(
        GetOnboardingOptionsQuery request,
        CancellationToken cancellationToken)
    {
        var sportFacets = await opinionRepository.GetApprovedSportsAsync(cancellationToken);
        var allSports = await sportsReadRepository.GetAllSportsAsync(cancellationToken);
        var profiles = await commentatorRepository.GetProfilesAsync(cancellationToken);
        var teamFacets = await opinionRepository.GetApprovedTeamsAsync(cancellationToken);
        var allTeams = await sportsReadRepository.GetAllTeamsAsync(cancellationToken);

        var sportIdBySlug = allSports.ToDictionary(x => x.Slug, x => x.Id);
        var teamIdBySlug = allTeams.ToDictionary(x => x.Slug, x => x.Id);

        var sports = sportFacets
            .Where(x => sportIdBySlug.ContainsKey(x.Slug))
            .Select(x => new OnboardingSport(sportIdBySlug[x.Slug], x.Name, x.Slug, x.OpinionCount))
            .ToList();

        // Yorumcu olmayanlar listelenmiyor: sporcu ve teknik direktör demeçleri
        // takip edilecek bir "yorumcu" değil.
        var commentators = profiles
            .Where(x => x.Commentator.PersonRole is PersonRole.Commentator or PersonRole.Unknown)
            .Select(x => new OnboardingPerson(
                x.Commentator.Id,
                x.Commentator.FullName,
                x.Commentator.PhotoUrl,
                x.OpinionCount,
                x.SportSlugs))
            .ToList();

        var teams = teamFacets
            .Where(x => teamIdBySlug.ContainsKey(x.Slug))
            .Select(x => new OnboardingTeam(teamIdBySlug[x.Slug], x.Name, x.LogoUrl, x.SportSlug))
            .ToList();

        return Result<OnboardingOptionsDto>.Ok(new OnboardingOptionsDto(sports, commentators, teams));
    }
}

/// <summary>
/// Kişiye özel akış. Haber ve görüş ayrı sorgular ama tek istekte dönüyor;
/// sayfa açılışında iki ağ turu beklemesin.
/// </summary>
public sealed class GetFeedQueryHandler(
    IUserRepository userRepository,
    IStoryRepository storyRepository,
    IOpinionRepository opinionRepository)
    : IRequestHandler<GetFeedQuery, Result<FeedDto>>
{
    private const int MaxTake = 60;

    public async Task<Result<FeedDto>> Handle(GetFeedQuery request, CancellationToken cancellationToken)
    {
        var user = await userRepository.GetWithPreferencesAsync(request.UserId, cancellationToken);

        if (user is null)
            return Result<FeedDto>.Fail("Kullanıcı bulunamadı.", HttpStatusCode.NotFound);

        var tercihler = PreferenceMapper.ToDto(user);

        var sportIds = user.FollowedSports.Select(x => x.Id).ToList();
        var teamIds = user.FollowedTeams.Select(x => x.Id).ToList();
        var commentatorIds = user.FollowedCommentators.Select(x => x.Id).ToList();

        if (sportIds.Count == 0 && teamIds.Count == 0 && commentatorIds.Count == 0)
            return Result<FeedDto>.Ok(new FeedDto(
                false,
                tercihler.Sports,
                tercihler.Teams,
                tercihler.Commentators,
                [],
                []));

        // Haberde yorumcu ölçütü yok: yorumcu takibi görüş akışını belirliyor.
        var stories = await storyRepository.GetForFollowedAsync(
            teamIds,
            sportIds,
            Math.Clamp(request.StoryTake, 1, MaxTake),
            cancellationToken);

        var opinions = await opinionRepository.GetForFollowedAsync(
            commentatorIds,
            teamIds,
            sportIds,
            Math.Clamp(request.OpinionTake, 1, MaxTake),
            cancellationToken);

        return Result<FeedDto>.Ok(new FeedDto(
            true,
            tercihler.Sports,
            tercihler.Teams,
            tercihler.Commentators,
            [.. stories.Select(x => x.ToListItem())],
            [.. opinions.Select(x => x.ToDto())]));
    }
}

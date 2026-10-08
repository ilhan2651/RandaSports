using MediatR;
using RandaSports.Application.Common.Wrappers;
using RandaSports.Application.Features.Commentary.Dtos;
using RandaSports.Application.Features.Stories.Dtos;

namespace RandaSports.Application.Features.Users;

public sealed record FollowedItemDto(Guid Id, string Name, string Slug, string? ImageUrl);

public sealed record MeDto(
    Guid Id,
    string Email,
    string? FullName,
    List<string> Roles,
    DateTimeOffset CreatedAt,
    bool OnboardingCompleted);

public sealed record PreferencesDto(
    List<FollowedItemDto> Teams,
    List<FollowedItemDto> Sports,
    List<FollowedItemDto> Commentators);

public sealed record GetMeQuery(Guid UserId) : IRequest<Result<MeDto>>;

public sealed record GetPreferencesQuery(Guid UserId) : IRequest<Result<PreferencesDto>>;

/// <param name="CompleteOnboarding">
/// Sihirbazın son adımından geliyorsa true: kullanıcı hiçbir şey seçmese bile
/// modal bir daha açılmıyor.
/// </param>
public sealed record UpdatePreferencesCommand(
    Guid UserId,
    List<Guid> TeamIds,
    List<Guid> SportIds,
    List<Guid> CommentatorIds,
    bool CompleteOnboarding = false) : IRequest<Result<PreferencesDto>>;

/// <param name="Sports">Görüşü olan branşlar; sihirbazın ilk adımı bunu gösteriyor.</param>
public sealed record OnboardingOptionsDto(
    List<OnboardingSport> Sports,
    List<OnboardingPerson> Commentators,
    List<OnboardingTeam> Teams);

public sealed record OnboardingSport(Guid Id, string Name, string Slug, int OpinionCount);

/// <param name="SportSlugs">Hangi branşlarda görüş verdiği; seçime göre süzülüyor.</param>
public sealed record OnboardingPerson(
    Guid Id,
    string FullName,
    string? PhotoUrl,
    int OpinionCount,
    List<string> SportSlugs);

public sealed record OnboardingTeam(Guid Id, string Name, string? LogoUrl, string? SportSlug);

/// <summary>Sihirbazın üç adımının verisi tek istekte; modal açılırken bekletmesin.</summary>
public sealed record GetOnboardingOptionsQuery : IRequest<Result<OnboardingOptionsDto>>;

/// <summary>
/// Kişiye özel akış: tercihlere göre süzülmüş haberler ve görüşler tek istekte.
/// Ayrı iki uç olsaydı sayfa iki ağ turu beklerdi.
/// </summary>
/// <param name="HasPreferences">
/// Hiç seçim yoksa false; sayfa o zaman "tercih seç" yönlendirmesi gösteriyor,
/// boş liste göstermiyor.
/// </param>
public sealed record FeedDto(
    bool HasPreferences,
    List<FollowedItemDto> Sports,
    List<FollowedItemDto> Teams,
    List<FollowedItemDto> Commentators,
    List<StoryListItemDto> Stories,
    List<OpinionDto> Opinions);

public sealed record GetFeedQuery(Guid UserId, int StoryTake = 24, int OpinionTake = 24)
    : IRequest<Result<FeedDto>>;

using MediatR;
using RandaSports.Application.Common.Wrappers;
using RandaSports.Application.Features.Commentary.Dtos;

namespace RandaSports.Application.Features.Commentary.Query.GetLatestOpinions;

/// <param name="Commentator">Yorumcu adresi (slug); boşsa hepsi.</param>
/// <param name="Team">Görüşün konusu olan takımın adresi (slug); boşsa hepsi.</param>
/// <param name="Sport">Branş adresi (slug); boşsa hepsi.</param>
/// <param name="Days">Son kaç günün görüşleri; 0 ise sınır yok.</param>
public sealed record GetLatestOpinionsQuery(
    string? Commentator = null,
    string? Team = null,
    string? Sport = null,
    int Days = 0,
    int Page = 1,
    int PageSize = 12) : IRequest<Result<Paged<OpinionDto>>>;

/// <summary>Filtre çubuğundaki takım listesi.</summary>
public sealed record GetOpinionTeamsQuery : IRequest<Result<List<TeamFacetDto>>>;

/// <summary>Filtre çubuğundaki branş listesi.</summary>
public sealed record GetOpinionSportsQuery : IRequest<Result<List<SportFacetDto>>>;

using MediatR;
using RandaSports.Application.Common.Wrappers;
using RandaSports.Application.Features.Sports.Dtos;

namespace RandaSports.Application.Features.Sports.Query.GetSports;

/// <param name="OnlyWithStories">
/// true: yalnızca yayınlanmış haberi olan branşlar. Menüde boş sekme göstermemek için.
/// </param>
public sealed record GetSportsQuery(bool OnlyWithStories = false) : IRequest<Result<List<SportDto>>>;

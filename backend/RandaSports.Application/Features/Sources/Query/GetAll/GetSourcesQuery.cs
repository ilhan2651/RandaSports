using MediatR;
using RandaSports.Application.Common.Wrappers;
using RandaSports.Application.Features.Sources.Dtos;

namespace RandaSports.Application.Features.Sources.Query.GetAll;

public sealed record GetSourcesQuery(bool? IsActive = null, Guid? SportId = null) : IRequest<Result<List<SourceDto>>>;

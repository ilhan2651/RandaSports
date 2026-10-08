using MediatR;
using RandaSports.Application.Common.Wrappers;
using RandaSports.Application.Features.Sources.Dtos;

namespace RandaSports.Application.Features.Sources.Query.GetById;

public sealed record GetSourceByIdQuery(Guid Id) : IRequest<Result<SourceDto>>;

using MediatR;
using RandaSports.Application.Common.Wrappers;
using RandaSports.Application.Features.Sources.Dtos;
using RandaSports.Domain.Enums;

namespace RandaSports.Application.Features.Sources.Command.Create;

public sealed record CreateSourceCommand(
    string Name,
    SourceType Type,
    string Url,
    Guid? SportId,
    string Language = "tr",
    int FetchIntervalMinutes = 10) : IRequest<Result<SourceDto>>;

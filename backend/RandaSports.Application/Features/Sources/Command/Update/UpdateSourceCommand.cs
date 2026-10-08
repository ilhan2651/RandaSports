using System.Text.Json.Serialization;
using MediatR;
using RandaSports.Application.Common.Wrappers;
using RandaSports.Application.Features.Sources.Dtos;
using RandaSports.Domain.Enums;

namespace RandaSports.Application.Features.Sources.Command.Update;

public sealed record UpdateSourceCommand(
    string Name,
    SourceType Type,
    string Url,
    Guid? SportId,
    string Language,
    int FetchIntervalMinutes,
    bool IsActive) : IRequest<Result<SourceDto>>
{
    [JsonIgnore]
    public Guid Id { get; init; }
}

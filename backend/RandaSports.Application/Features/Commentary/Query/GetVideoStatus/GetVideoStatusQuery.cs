using MediatR;
using RandaSports.Application.Common.Wrappers;
using RandaSports.Application.Features.Commentary.Dtos;

namespace RandaSports.Application.Features.Commentary.Query.GetVideoStatus;

/// <param name="Status">Boş geçilirse tüm durumlar döner.</param>
public sealed record GetVideoStatusQuery(string? Status, int Take)
    : IRequest<Result<VideoStatusReportDto>>;

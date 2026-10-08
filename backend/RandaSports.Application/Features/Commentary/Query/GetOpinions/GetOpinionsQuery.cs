using MediatR;
using RandaSports.Application.Common.Wrappers;
using RandaSports.Application.Features.Commentary.Dtos;

namespace RandaSports.Application.Features.Commentary.Query.GetOpinions;

/// <param name="Status">"Pending" | "Approved" | "Rejected"; boşsa hepsi.</param>
public sealed record GetOpinionsQuery(
    string? Status = null,
    int Page = 1,
    int PageSize = 20) : IRequest<Result<Paged<OpinionDto>>>;

using MediatR;
using RandaSports.Application.Common.Wrappers;

namespace RandaSports.Application.Features.Commentary.Command.ClassifyCommentatorRoles;

/// <param name="Take">Bir turda kaç kişiye sorulacak; kota için sınırlı.</param>
public sealed record ClassifyCommentatorRolesCommand(int Take) : IRequest<Result<int>>;

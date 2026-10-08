using MediatR;
using RandaSports.Application.Common.Wrappers;

namespace RandaSports.Application.Features.Commentary.Command.ReviewCommentator;

/// <param name="Approve">
/// true: kişi gerçekten yorumcu, doğrulanmış sayılıyor.
/// false: yanlış eklenmiş; kayıt siliniyor ve ona atfedilmiş görüşler onaya geri dönüyor.
/// </param>
public sealed record ReviewCommentatorCommand(Guid Id, bool Approve) : IRequest<Result<bool>>;

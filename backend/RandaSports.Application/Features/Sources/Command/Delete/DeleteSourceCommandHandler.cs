using System.Net;
using MediatR;
using RandaSports.Application.Common.Wrappers;
using RandaSports.Application.Interfaces;
using RandaSports.Application.Interfaces.Repositories;
using RandaSports.Domain.Entities;

namespace RandaSports.Application.Features.Sources.Command.Delete;

public sealed class DeleteSourceCommandHandler(
    ISourceRepository sourceRepository,
    IGenericRepository<Article> articleRepository,
    IUnitOfWork unitOfWork) : IRequestHandler<DeleteSourceCommand, Result>
{
    public async Task<Result> Handle(DeleteSourceCommand request, CancellationToken cancellationToken)
    {
        var source = await sourceRepository.GetByIdAsync(request.Id, cancellationToken);
        if (source is null)
            return Result.Fail("Kaynak bulunamadı.", HttpStatusCode.NotFound);

        if (await articleRepository.AnyAsync(x => x.SourceId == request.Id, cancellationToken))
            return Result.Fail(
                "Bu kaynaktan gelmiş haberler var. Silmek yerine pasif yapın.",
                "SOURCE_HAS_ARTICLES",
                HttpStatusCode.Conflict);

        sourceRepository.Delete(source);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Ok("Kaynak silindi.");
    }
}

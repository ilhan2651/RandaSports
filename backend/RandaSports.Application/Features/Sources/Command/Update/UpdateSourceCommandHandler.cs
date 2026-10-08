using System.Net;
using MediatR;
using RandaSports.Application.Common.Wrappers;
using RandaSports.Application.Features.Sources.Dtos;
using RandaSports.Application.Interfaces;
using RandaSports.Application.Interfaces.Repositories;
using RandaSports.Domain.Entities;

namespace RandaSports.Application.Features.Sources.Command.Update;

public sealed class UpdateSourceCommandHandler(
    ISourceRepository sourceRepository,
    IGenericRepository<Sport> sportRepository,
    IUnitOfWork unitOfWork) : IRequestHandler<UpdateSourceCommand, Result<SourceDto>>
{
    public async Task<Result<SourceDto>> Handle(UpdateSourceCommand request, CancellationToken cancellationToken)
    {
        var source = await sourceRepository.GetByIdAsync(request.Id, cancellationToken);
        if (source is null)
            return Result<SourceDto>.Fail("Kaynak bulunamadı.", HttpStatusCode.NotFound);

        var url = request.Url.Trim();

        if (await sourceRepository.AnyAsync(x => x.Url == url && x.Id != request.Id, cancellationToken))
            return Result<SourceDto>.Fail("Bu URL ile kayıtlı başka bir kaynak var.", HttpStatusCode.Conflict);

        if (request.SportId is { } sportId && !await sportRepository.AnyAsync(x => x.Id == sportId, cancellationToken))
            return Result<SourceDto>.Fail("Seçilen branş bulunamadı.", HttpStatusCode.NotFound);

        source.Name = request.Name.Trim();
        source.Type = request.Type;
        source.Url = url;
        source.Language = request.Language.Trim().ToLowerInvariant();
        source.SportId = request.SportId;
        source.FetchIntervalMinutes = request.FetchIntervalMinutes;
        source.IsActive = request.IsActive;

        await unitOfWork.SaveChangesAsync(cancellationToken);

        var updated = await sourceRepository.GetWithSportAsync(source.Id, cancellationToken);
        return Result<SourceDto>.Ok(updated!.ToDto());
    }
}

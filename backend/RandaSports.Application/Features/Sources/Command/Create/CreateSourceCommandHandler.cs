using System.Net;
using MediatR;
using RandaSports.Application.Common.Wrappers;
using RandaSports.Application.Features.Sources.Dtos;
using RandaSports.Application.Interfaces;
using RandaSports.Application.Interfaces.Repositories;
using RandaSports.Domain.Entities;

namespace RandaSports.Application.Features.Sources.Command.Create;

public sealed class CreateSourceCommandHandler(
    ISourceRepository sourceRepository,
    IGenericRepository<Sport> sportRepository,
    IUnitOfWork unitOfWork) : IRequestHandler<CreateSourceCommand, Result<SourceDto>>
{
    public async Task<Result<SourceDto>> Handle(CreateSourceCommand request, CancellationToken cancellationToken)
    {
        var url = request.Url.Trim();

        if (await sourceRepository.AnyAsync(x => x.Url == url, cancellationToken))
            return Result<SourceDto>.Fail("Bu URL ile kayıtlı bir kaynak zaten var.", HttpStatusCode.Conflict);

        if (request.SportId is { } sportId && !await sportRepository.AnyAsync(x => x.Id == sportId, cancellationToken))
            return Result<SourceDto>.Fail("Seçilen branş bulunamadı.", HttpStatusCode.NotFound);

        var source = new Source
        {
            Name = request.Name.Trim(),
            Type = request.Type,
            Url = url,
            Language = request.Language.Trim().ToLowerInvariant(),
            SportId = request.SportId,
            FetchIntervalMinutes = request.FetchIntervalMinutes
        };

        await sourceRepository.AddAsync(source, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        var created = await sourceRepository.GetWithSportAsync(source.Id, cancellationToken);
        return Result<SourceDto>.Created(created!.ToDto(), $"/api/sources/{source.Id}");
    }
}

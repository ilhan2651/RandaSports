using RandaSports.Domain.Entities;

namespace RandaSports.Application.Interfaces.Repositories;

public interface ISourceRepository : IGenericRepository<Source>
{
    Task<List<Source>> GetListAsync(bool? isActive, Guid? sportId, CancellationToken cancellationToken = default);
    Task<Source?> GetWithSportAsync(Guid id, CancellationToken cancellationToken = default);
    Task<List<Source>> GetActiveRssSourcesAsync(CancellationToken cancellationToken = default);
}

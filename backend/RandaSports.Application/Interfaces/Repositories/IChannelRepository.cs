using RandaSports.Domain.Entities;

namespace RandaSports.Application.Interfaces.Repositories;

public interface IChannelRepository : IGenericRepository<Channel>
{
    /// <summary>Taranma sırası gelen kanallar: hiç bakılmamışlar önce.</summary>
    Task<List<Channel>> GetDueForCheckAsync(
        int take,
        DateTimeOffset cutoff,
        CancellationToken cancellationToken = default);

    Task<Channel?> GetBySlugAsync(string slug, CancellationToken cancellationToken = default);

    Task<Channel?> GetByHandleAsync(string handle, CancellationToken cancellationToken = default);

    /// <summary>Kanalı, konuşması beklenen yorumcularla birlikte getirir.</summary>
    Task<Channel?> GetWithCommentatorsAsync(Guid id, CancellationToken cancellationToken = default);

    Task<List<Channel>> GetAllWithCommentatorsAsync(CancellationToken cancellationToken = default);
}

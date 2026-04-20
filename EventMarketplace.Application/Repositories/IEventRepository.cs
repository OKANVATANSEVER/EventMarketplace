using EventMarketplace.Application.DTOs;
using EventMarketplace.Domain.Entities;

namespace EventMarketplace.Application.Repositories;

public interface IEventRepository
{
    Task AddAsync(Event eventEntity, CancellationToken cancellationToken = default);
    Task<Event?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task SaveChangesAsync(CancellationToken cancellationToken = default);

    Task<PagedResult<EventListItemDto>> GetFilteredAsync(
        string? city,
        Guid? categoryId,
        bool? isFeatured,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default);
}
using EventMarketplace.Application.DTOs;
using EventMarketplace.Application.Repositories;
using EventMarketplace.Domain.Entities;
using EventMarketplace.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace EventMarketplace.Infrastructure.Repositories;

public sealed class EventRepository(ApplicationDbContext dbContext) : IEventRepository
{
    public async Task AddAsync(Event eventEntity, CancellationToken cancellationToken = default)
    {
        await dbContext.Events.AddAsync(eventEntity, cancellationToken);
    }

    public async Task<Event?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await dbContext.Events
            .Include(e => e.Category)
            .FirstOrDefaultAsync(e => e.Id == id, cancellationToken);
    }

    public async Task SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        await dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task<PagedResult<EventListItemDto>> GetFilteredAsync(
        string? city,
        Guid? categoryId,
        bool? isFeatured,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default)
    {
        var query = dbContext.Events.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(city))
            query = query.Where(e => e.City == city);

        if (categoryId.HasValue)
            query = query.Where(e => e.CategoryId == categoryId.Value);

        if (isFeatured.HasValue)
            query = query.Where(e => e.IsFeatured == isFeatured.Value);

        var totalCount = await query.CountAsync(cancellationToken);

        var items = await query
            .OrderBy(e => e.StartDate)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(e => new EventListItemDto(
                e.Id,
                e.Title,
                e.Price,
                e.StartDate,
                e.City,
                e.Category != null ? e.Category.Name : string.Empty,
                e.IsFeatured))
            .ToListAsync(cancellationToken);

            return new PagedResult<EventListItemDto>(items, page, pageSize, totalCount);
    }
}
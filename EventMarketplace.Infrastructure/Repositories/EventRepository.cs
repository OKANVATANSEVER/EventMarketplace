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
        bool? isApproved,
        string? title,
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

        if (isApproved.HasValue)
            query = query.Where(e => e.IsApproved == isApproved.Value);

        if (!string.IsNullOrWhiteSpace(title))
            query = query.Where(e => e.Title.Contains(title));

        var totalCount = await query.CountAsync(cancellationToken);

        var items = await query
            .OrderByDescending(e => e.StartDate)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(e => new EventListItemDto(
                e.Id,
                e.Title,
                e.Price,
                e.StartDate,
                e.City,
                e.Category != null ? e.Category.Name : string.Empty,
                e.IsFeatured,
                e.IsApproved))
            .ToListAsync(cancellationToken);

            return new PagedResult<EventListItemDto>(items, page, pageSize, totalCount);
    }

    public async Task<IList<EventListItemDto>> GetEventsStartingAfterAsync(
        DateTime after,
        CancellationToken cancellationToken = default)
    {
        return await dbContext.Events
            .AsNoTracking()
            .Where(e => e.StartDate >= after)
            .Select(e => new EventListItemDto(
                e.Id,
                e.Title,
                e.Price,
                e.StartDate,
                e.City,
                e.Category != null ? e.Category.Name : string.Empty,
                e.IsFeatured,
                e.IsApproved))
            .ToListAsync(cancellationToken);
    }

    public async Task<IList<CategoryEventCountDto>> GetEventCountByCategoryAsync(
        DateTime after,
        CancellationToken cancellationToken = default)
    {
        return await dbContext.Events
            .AsNoTracking()
            .Where(e => e.StartDate >= after)
            .GroupBy(e => e.Category != null ? e.Category.Name : "Kategorisiz")
            .Select(g => new CategoryEventCountDto(g.Key, g.Count()))
            .OrderByDescending(x => x.EventCount)
            .ToListAsync(cancellationToken);
    }

    public async Task UpdateAsync(Event eventEntity, CancellationToken cancellationToken = default)
    {
        dbContext.Events.Update(eventEntity);
        await dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task<IList<CategoryDto>> GetCategoriesAsync(CancellationToken cancellationToken = default)
    {
        return await dbContext.Categories
            .AsNoTracking()
            .OrderBy(c => c.Name)
            .Select(c => new CategoryDto(c.Id, c.Name))
            .ToListAsync(cancellationToken);
    }

    public async Task<IList<MonthlySummaryDto>> GetMonthlyEventSummaryAsync(
        DateTime from,
        CancellationToken cancellationToken = default)
    {
        var raw = await dbContext.Events
            .AsNoTracking()
            .Where(e => e.StartDate >= from)
            .GroupBy(e => new { e.StartDate.Year, e.StartDate.Month })
            .Select(g => new { g.Key.Year, g.Key.Month, Count = g.Count() })
            .ToListAsync(cancellationToken);

        return raw
            .Select(m => new MonthlySummaryDto(
                m.Year,
                m.Month,
                new DateTime(m.Year, m.Month, 1).ToString("MMM yyyy"),
                m.Count))
            .OrderBy(m => m.Year).ThenBy(m => m.Month)
            .ToList<MonthlySummaryDto>();
    }
}
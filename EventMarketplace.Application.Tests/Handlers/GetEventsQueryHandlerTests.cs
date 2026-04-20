using EventMarketplace.Application.DTOs;
using EventMarketplace.Application.Handlers.Events;
using EventMarketplace.Application.Queries.Events;
using EventMarketplace.Application.Repositories;
using EventMarketplace.Domain.Entities;
using FluentAssertions;
using Xunit;

namespace EventMarketplace.Application.Tests.Handlers;

public class GetEventsQueryHandlerTests
{
    [Fact]
    public async Task Handle_Should_ReturnPagedResult_WithAppliedFilters()
    {
        var repository = new FakeEventRepository();
        var handler = new GetEventsQueryHandler(repository);

        var filter = new GetEventsFilter("Istanbul", Guid.NewGuid(), true, true, null, 2, 5);

        var result = await handler.Handle(new GetEventsQuery(filter), CancellationToken.None);

        result.Page.Should().Be(2);
        result.PageSize.Should().Be(5);
        result.TotalCount.Should().Be(1);
        repository.LastCity.Should().Be("Istanbul");
        repository.LastFeatured.Should().BeTrue();
    }

    private sealed class FakeEventRepository : IEventRepository
    {
        public string? LastCity { get; private set; }
        public bool? LastFeatured { get; private set; }

        public Task AddAsync(Event eventEntity, CancellationToken cancellationToken = default)
            => Task.CompletedTask;

        public Task<Event?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
            => Task.FromResult<Event?>(null);

        public Task SaveChangesAsync(CancellationToken cancellationToken = default)
            => Task.CompletedTask;

        public Task<PagedResult<EventListItemDto>> GetFilteredAsync(
            string? city,
            Guid? categoryId,
            bool? isFeatured,
            bool? isApproved,
            string? title,
            int page,
            int pageSize,
            CancellationToken cancellationToken = default)
        {
            LastCity = city;
            LastFeatured = isFeatured;

            var items = new List<EventListItemDto>
            {
                new(Guid.NewGuid(), "Fake", 10, DateTime.UtcNow, city ?? "Unknown", "Concert", isFeatured ?? false, false)
            };

            return Task.FromResult(new PagedResult<EventListItemDto>(items, page, pageSize, 1));
        }

        public Task<IList<EventListItemDto>> GetEventsStartingAfterAsync(DateTime after, CancellationToken cancellationToken = default)
            => Task.FromResult<IList<EventListItemDto>>([]);

        public Task<IList<CategoryEventCountDto>> GetEventCountByCategoryAsync(DateTime after, CancellationToken cancellationToken = default)
            => Task.FromResult<IList<CategoryEventCountDto>>([]);

        public Task<IList<MonthlySummaryDto>> GetMonthlyEventSummaryAsync(DateTime from, CancellationToken cancellationToken = default)
            => Task.FromResult<IList<MonthlySummaryDto>>([]);
        
        public Task UpdateAsync(Event eventEntity, CancellationToken cancellationToken = default)
            => Task.CompletedTask;

        public Task<IList<CategoryDto>> GetCategoriesAsync(CancellationToken cancellationToken = default)
            => Task.FromResult<IList<CategoryDto>>([]);
    }
}

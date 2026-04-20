using EventMarketplace.Application.DTOs;
using EventMarketplace.Application.Queries.Events;
using EventMarketplace.Application.Repositories;
using MediatR;

namespace EventMarketplace.Application.Handlers.Events;

public sealed class GetEventsQueryHandler(IEventRepository eventRepository)
    : IRequestHandler<GetEventsQuery, PagedResult<EventListItemDto>>
{
    public async Task<PagedResult<EventListItemDto>> Handle(
        GetEventsQuery request,
        CancellationToken cancellationToken)
    {
        return await eventRepository.GetFilteredAsync(
            request.Filter.City,
            request.Filter.CategoryId,
            request.Filter.IsFeatured,
            request.Filter.Page,
            request.Filter.PageSize,
            cancellationToken);
    }
}

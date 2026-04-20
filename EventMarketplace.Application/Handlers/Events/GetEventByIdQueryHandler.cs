using EventMarketplace.Application.DTOs;
using EventMarketplace.Application.Queries.Events;
using EventMarketplace.Application.Repositories;
using MediatR;

namespace EventMarketplace.Application.Handlers.Events;

public sealed class GetEventByIdQueryHandler(IEventRepository eventRepository)
    : IRequestHandler<GetEventByIdQuery, EventDto?>
{
    public async Task<EventDto?> Handle(GetEventByIdQuery request, CancellationToken cancellationToken)
    {
        var eventEntity = await eventRepository.GetByIdAsync(request.Id, cancellationToken);
        if (eventEntity is null)
        {
            return null;
        }

        return new EventDto(
            eventEntity.Id,
            eventEntity.Title,
            eventEntity.Description,
            eventEntity.Price,
            eventEntity.StartDate,
            eventEntity.EndDate,
            eventEntity.City,
            eventEntity.CategoryId,
            eventEntity.Category?.Name ?? string.Empty,
            eventEntity.OrganizerId,
            eventEntity.IsFeatured,
            eventEntity.IsApproved);
    }
}
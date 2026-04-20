using EventMarketplace.Application.DTOs;
using MediatR;

namespace EventMarketplace.Application.Queries.Events;

public sealed record GetEventsQuery(GetEventsFilter Filter) : IRequest<PagedResult<EventListItemDto>>;

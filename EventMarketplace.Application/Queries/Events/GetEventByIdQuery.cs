using EventMarketplace.Application.DTOs;
using MediatR;

namespace EventMarketplace.Application.Queries.Events;

public sealed record GetEventByIdQuery(Guid Id) : IRequest<EventDto?>;
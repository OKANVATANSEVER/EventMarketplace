using MediatR;

namespace EventMarketplace.Application.Commands.Events;

public sealed record CreateEventCommand(
    string Title,
    string Description,
    decimal Price,
    DateTime StartDate,
    DateTime EndDate,
    string City,
    Guid CategoryId,
    string OrganizerId,
    bool IsFeatured,
    bool IsApproved) : IRequest<Guid>;
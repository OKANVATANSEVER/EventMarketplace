using MediatR;

namespace EventMarketplace.Application.Commands.Events;

public sealed record UpdateEventCommand(
    Guid Id,
    string Title,
    string Description,
    decimal Price,
    DateTime StartDate,
    DateTime EndDate,
    string City,
    Guid CategoryId,
    bool IsFeatured,
    bool IsApproved) : IRequest<bool>;

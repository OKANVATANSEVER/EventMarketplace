using MediatR;

namespace EventMarketplace.Application.Commands.Notifications;

public record NotifyMembersAboutEventCommand(
    Guid EventId,
    string EventTitle,
    string EventDescription,
    DateTime StartDate,
    string City,
    decimal Price) : IRequest<bool>;

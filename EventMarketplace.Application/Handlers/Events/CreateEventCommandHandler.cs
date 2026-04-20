using EventMarketplace.Application.Commands.Events;
using EventMarketplace.Application.Repositories;
using EventMarketplace.Domain.Entities;
using MediatR;

namespace EventMarketplace.Application.Handlers.Events;

public sealed class CreateEventCommandHandler(IEventRepository eventRepository)
    : IRequestHandler<CreateEventCommand, Guid>
{
    public async Task<Guid> Handle(CreateEventCommand request, CancellationToken cancellationToken)
    {
        var eventEntity = new Event
        {
            Title = request.Title,
            Description = request.Description,
            Price = request.Price,
            StartDate = request.StartDate,
            EndDate = request.EndDate,
            City = request.City,
            CategoryId = request.CategoryId,
            OrganizerId = request.OrganizerId,
            IsFeatured = request.IsFeatured
        };

        await eventRepository.AddAsync(eventEntity, cancellationToken);
        await eventRepository.SaveChangesAsync(cancellationToken);

        return eventEntity.Id;
    }
}
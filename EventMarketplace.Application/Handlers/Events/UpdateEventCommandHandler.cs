using EventMarketplace.Application.Commands.Events;
using EventMarketplace.Application.Exceptions;
using EventMarketplace.Application.Repositories;
using MediatR;

namespace EventMarketplace.Application.Handlers.Events;

public sealed class UpdateEventCommandHandler(IEventRepository eventRepository)
    : IRequestHandler<UpdateEventCommand, bool>
{
    public async Task<bool> Handle(UpdateEventCommand request, CancellationToken cancellationToken)
    {
        var ev = await eventRepository.GetByIdAsync(request.Id, cancellationToken)
            ?? throw new NotFoundException($"Event {request.Id} not found.");

        ev.Title = request.Title;
        ev.Description = request.Description;
        ev.Price = request.Price;
        ev.StartDate = request.StartDate;
        ev.EndDate = request.EndDate;
        ev.City = request.City;
        ev.CategoryId = request.CategoryId;
        ev.IsFeatured = request.IsFeatured;

        await eventRepository.UpdateAsync(ev, cancellationToken);
        return true;
    }
}

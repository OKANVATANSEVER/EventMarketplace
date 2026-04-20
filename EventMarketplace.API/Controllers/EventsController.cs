using EventMarketplace.Application.Commands.Events;
using EventMarketplace.Application.Queries.Events;
using EventMarketplace.API.Contracts.Events;
using EventMarketplace.API.Responses;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace EventMarketplace.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class EventsController(IMediator mediator) : CustomBaseController
{
    [HttpGet]
    [AllowAnonymous]
    public async Task<IActionResult> GetAll(
        [FromQuery] string? city,
        [FromQuery] Guid? categoryId,
        [FromQuery] bool? isFeatured,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        var filter = new GetEventsFilter(city, categoryId, isFeatured, page, pageSize);
        var events = await mediator.Send(new GetEventsQuery(filter), cancellationToken);
        return ActionResultInstance(CustomResponse<object>.Success(events, 200));
    }

    [HttpPost]
    [Authorize(Roles = "Organizer,Admin")]
    public async Task<IActionResult> Create(
        [FromBody] CreateEventRequest request,
        CancellationToken cancellationToken)
    {
        var organizerId = User.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? User.FindFirstValue("sub");

        if (string.IsNullOrWhiteSpace(organizerId))
            return ActionResultInstance(CustomResponse<NoContent>.Fail("Organizer identity is missing.", 401, true));

        var command = new CreateEventCommand(
            request.Title,
            request.Description,
            request.Price,
            request.StartDate,
            request.EndDate,
            request.City,
            request.CategoryId,
            organizerId,
            request.IsFeatured);

        var id = await mediator.Send(command, cancellationToken);
        return ActionResultInstance(CustomResponse<object>.Success(new { id }, 201));
    }

    [HttpPut("{id:guid}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Update(
        Guid id,
        [FromBody] UpdateEventRequest request,
        CancellationToken cancellationToken)
    {
        await mediator.Send(new UpdateEventCommand(
            id,
            request.Title,
            request.Description,
            request.Price,
            request.StartDate,
            request.EndDate,
            request.City,
            request.CategoryId,
            request.IsFeatured), cancellationToken);

        return ActionResultInstance(CustomResponse<NoContent>.Success(200));
    }

    [HttpGet("{id:guid}")]
    [AllowAnonymous]
    public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
    {
        var eventDto = await mediator.Send(new GetEventByIdQuery(id), cancellationToken);
        if (eventDto is null)
            return ActionResultInstance(CustomResponse<NoContent>.Fail("Event not found.", 404, true));

        return ActionResultInstance(CustomResponse<object>.Success(eventDto, 200));
    }
}
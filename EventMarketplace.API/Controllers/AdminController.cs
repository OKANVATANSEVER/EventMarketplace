using EventMarketplace.API.Responses;
using EventMarketplace.Application.Commands.Notifications;
using EventMarketplace.Application.Queries.Admin;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using EventMarketplace.Application.DTOs;

namespace EventMarketplace.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Admin")]
public class AdminController(IMediator mediator) : CustomBaseController
{
    /// <summary>Returns admin dashboard statistics for the current month.</summary>
    [HttpGet("dashboard")]
    public async Task<IActionResult> GetDashboardStats(CancellationToken cancellationToken)
    {
        var stats = await mediator.Send(new GetAdminDashboardStatsQuery(), cancellationToken);
        return ActionResultInstance(CustomResponse<object>.Success(stats, 200));
    }

    /// <summary>Returns all registered members.</summary>
    [HttpGet("members")]
    public async Task<IActionResult> GetAllMembers(CancellationToken cancellationToken)
    {
        var members = await mediator.Send(new GetAllMembersQuery(), cancellationToken);
        return ActionResultInstance(CustomResponse<IList<MemberDto>>.Success(members, 200));
    }

    /// <summary>Sends an email notification to all members about the specified event.</summary>
    [HttpPost("events/{eventId:guid}/notify")]
    public async Task<IActionResult> NotifyMembers(
        Guid eventId,
        [FromBody] NotifyMembersRequest request,
        CancellationToken cancellationToken)
    {
        var command = new NotifyMembersAboutEventCommand(
            eventId,
            request.EventTitle,
            request.EventDescription,
            request.StartDate,
            request.City,
            request.Price);

        var sent = await mediator.Send(command, cancellationToken);
        if (!sent)
            return ActionResultInstance(CustomResponse<NoContent>.Fail("No members to notify.", 404, true));

        return ActionResultInstance(CustomResponse<NoContent>.Success(200));
    }
}

public record NotifyMembersRequest(
    string EventTitle,
    string EventDescription,
    DateTime StartDate,
    string City,
    decimal Price);

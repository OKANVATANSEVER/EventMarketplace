using EventMarketplace.API.Responses;
using EventMarketplace.Domain.Entities;
using EventMarketplace.Infrastructure.Persistence;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace EventMarketplace.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Organizer,Admin")]
public class OrganizerController(ApplicationDbContext dbContext) : CustomBaseController
{
    [HttpGet("events")]
    public async Task<IActionResult> GetMyEvents(CancellationToken cancellationToken)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub");
        if (string.IsNullOrWhiteSpace(userId))
            return ActionResultInstance(CustomResponse<NoContent>.Fail("Organizer identity is missing.", 401, true));

        var items = await dbContext.Events
            .AsNoTracking()
            .Where(x => x.OrganizerId == userId)
            .OrderByDescending(x => x.StartDate)
            .Select(x => new
            {
                x.Id,
                x.Title,
                x.Price,
                x.StartDate,
                x.EndDate,
                x.City,
                x.CategoryId,
                CategoryName = x.Category != null ? x.Category.Name : string.Empty,
                x.IsFeatured,
                x.IsApproved
            })
            .ToListAsync(cancellationToken);

        return ActionResultInstance(CustomResponse<object>.Success(items, 200));
    }

    [HttpPut("events/{id:guid}")]
    public async Task<IActionResult> UpdateMyEvent(Guid id, [FromBody] UpdateOrganizerEventRequest request, CancellationToken cancellationToken)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub");
        if (string.IsNullOrWhiteSpace(userId))
            return ActionResultInstance(CustomResponse<NoContent>.Fail("Organizer identity is missing.", 401, true));

        var eventEntity = await dbContext.Events.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
        if (eventEntity is null)
            return ActionResultInstance(CustomResponse<NoContent>.Fail("Event not found.", 404, true));

        if (eventEntity.OrganizerId != userId && !User.IsInRole("Admin"))
            return ActionResultInstance(CustomResponse<NoContent>.Fail("Forbidden.", 403, true));

        eventEntity.Title = request.Title;
        eventEntity.Description = request.Description;
        eventEntity.Price = request.Price;
        eventEntity.StartDate = request.StartDate;
        eventEntity.EndDate = request.EndDate;
        eventEntity.City = request.City;
        eventEntity.CategoryId = request.CategoryId;
        eventEntity.IsFeatured = request.IsFeatured;

        if (!User.IsInRole("Admin"))
            eventEntity.IsApproved = false;

        await dbContext.SaveChangesAsync(cancellationToken);
        return ActionResultInstance(CustomResponse<NoContent>.Success(200));
    }

    [HttpGet("ad-slots")]
    public async Task<IActionResult> GetMyAdSlots(CancellationToken cancellationToken)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub");
        if (string.IsNullOrWhiteSpace(userId))
            return ActionResultInstance(CustomResponse<NoContent>.Fail("Organizer identity is missing.", 401, true));

        var items = await dbContext.AdvertisementSlots
            .AsNoTracking()
            .Where(x => x.CreatedByUserId == userId)
            .OrderByDescending(x => x.Priority)
            .Select(x => new
            {
                x.Id,
                x.Placement,
                x.Title,
                x.Subtitle,
                x.LinkUrl,
                x.ImageUrl,
                x.IsActive,
                x.Priority,
                x.StartDateUtc,
                x.EndDateUtc
            })
            .ToListAsync(cancellationToken);

        return ActionResultInstance(CustomResponse<object>.Success(items, 200));
    }

    [HttpPost("ad-slots")]
    public async Task<IActionResult> CreateMyAdSlot([FromBody] UpsertOrganizerAdSlotRequest request, CancellationToken cancellationToken)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub");
        if (string.IsNullOrWhiteSpace(userId))
            return ActionResultInstance(CustomResponse<NoContent>.Fail("Organizer identity is missing.", 401, true));

        var slot = new AdvertisementSlot
        {
            CreatedByUserId = userId,
            Placement = request.Placement,
            Title = request.Title,
            Subtitle = request.Subtitle,
            LinkUrl = request.LinkUrl,
            ImageUrl = request.ImageUrl,
            IsActive = request.IsActive,
            Priority = request.Priority,
            StartDateUtc = request.StartDateUtc,
            EndDateUtc = request.EndDateUtc
        };

        await dbContext.AdvertisementSlots.AddAsync(slot, cancellationToken);
        await dbContext.SaveChangesAsync(cancellationToken);
        return ActionResultInstance(CustomResponse<object>.Success(new { slot.Id }, 201));
    }

    [HttpPut("ad-slots/{id:guid}")]
    public async Task<IActionResult> UpdateMyAdSlot(Guid id, [FromBody] UpsertOrganizerAdSlotRequest request, CancellationToken cancellationToken)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub");
        if (string.IsNullOrWhiteSpace(userId))
            return ActionResultInstance(CustomResponse<NoContent>.Fail("Organizer identity is missing.", 401, true));

        var slot = await dbContext.AdvertisementSlots.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
        if (slot is null)
            return ActionResultInstance(CustomResponse<NoContent>.Fail("Slot not found.", 404, true));

        if (slot.CreatedByUserId != userId && !User.IsInRole("Admin"))
            return ActionResultInstance(CustomResponse<NoContent>.Fail("Forbidden.", 403, true));

        slot.Placement = request.Placement;
        slot.Title = request.Title;
        slot.Subtitle = request.Subtitle;
        slot.LinkUrl = request.LinkUrl;
        slot.ImageUrl = request.ImageUrl;
        slot.IsActive = request.IsActive;
        slot.Priority = request.Priority;
        slot.StartDateUtc = request.StartDateUtc;
        slot.EndDateUtc = request.EndDateUtc;

        await dbContext.SaveChangesAsync(cancellationToken);
        return ActionResultInstance(CustomResponse<NoContent>.Success(200));
    }

    [HttpDelete("ad-slots/{id:guid}")]
    public async Task<IActionResult> DeleteMyAdSlot(Guid id, CancellationToken cancellationToken)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub");
        if (string.IsNullOrWhiteSpace(userId))
            return ActionResultInstance(CustomResponse<NoContent>.Fail("Organizer identity is missing.", 401, true));

        var slot = await dbContext.AdvertisementSlots.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
        if (slot is null)
            return ActionResultInstance(CustomResponse<NoContent>.Fail("Slot not found.", 404, true));

        if (slot.CreatedByUserId != userId && !User.IsInRole("Admin"))
            return ActionResultInstance(CustomResponse<NoContent>.Fail("Forbidden.", 403, true));

        dbContext.AdvertisementSlots.Remove(slot);
        await dbContext.SaveChangesAsync(cancellationToken);
        return ActionResultInstance(CustomResponse<NoContent>.Success(200));
    }
}

public record UpdateOrganizerEventRequest(
    string Title,
    string Description,
    decimal Price,
    DateTime StartDate,
    DateTime EndDate,
    string City,
    Guid CategoryId,
    bool IsFeatured);

public record UpsertOrganizerAdSlotRequest(
    string Placement,
    string Title,
    string Subtitle,
    string? LinkUrl,
    string? ImageUrl,
    bool IsActive,
    int Priority,
    DateTime? StartDateUtc,
    DateTime? EndDateUtc);

using EventMarketplace.API.Responses;
using EventMarketplace.Application.Commands.Admin;
using EventMarketplace.Application.Commands.Notifications;
using EventMarketplace.Application.Queries.Admin;
using EventMarketplace.Domain.Entities;
using EventMarketplace.Infrastructure.Persistence;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using EventMarketplace.Application.DTOs;
using System.Security.Claims;

namespace EventMarketplace.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Admin")]
public class AdminController(IMediator mediator, ApplicationDbContext dbContext) : CustomBaseController
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

    /// <summary>Returns all categories.</summary>
    [HttpGet("categories")]
    public async Task<IActionResult> GetCategories(CancellationToken cancellationToken)
    {
        var categories = await mediator.Send(new GetCategoriesQuery(), cancellationToken);
        return ActionResultInstance(CustomResponse<IList<CategoryDto>>.Success(categories, 200));
    }

    /// <summary>Creates a new member.</summary>
    [HttpPost("members")]
    public async Task<IActionResult> CreateMember(
        [FromBody] CreateMemberRequest request,
        CancellationToken cancellationToken)
    {
        var (success, error) = await mediator.Send(
            new CreateMemberCommand(request.FirstName, request.LastName, request.Email, request.Password, request.Role),
            cancellationToken);

        if (!success)
            return ActionResultInstance(CustomResponse<NoContent>.Fail(error, 400, true));

        return ActionResultInstance(CustomResponse<NoContent>.Success(201));
    }

    /// <summary>Activates or deactivates a member.</summary>
    [HttpPut("members/{userId}/status")]
    public async Task<IActionResult> SetMemberStatus(
        string userId,
        [FromBody] SetMemberStatusRequest request,
        CancellationToken cancellationToken)
    {
        var success = await mediator.Send(
            new SetMemberActiveStatusCommand(userId, request.IsActive),
            cancellationToken);

        if (!success)
            return ActionResultInstance(CustomResponse<NoContent>.Fail("Member not found.", 404, true));

        return ActionResultInstance(CustomResponse<NoContent>.Success(200));
    }

    [HttpGet("site-pages/{slug}")]
    public async Task<IActionResult> GetSitePage(string slug, CancellationToken cancellationToken)
    {
        var page = await dbContext.SitePageContents
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Slug == slug, cancellationToken);

        if (page is null)
            return ActionResultInstance(CustomResponse<NoContent>.Fail("Page not found.", 404, true));

        return ActionResultInstance(CustomResponse<object>.Success(new
        {
            page.Id,
            page.Slug,
            page.Title,
            page.Body,
            page.UpdatedAtUtc
        }, 200));
    }

    [HttpPut("site-pages/{slug}")]
    public async Task<IActionResult> UpsertSitePage(
        string slug,
        [FromBody] UpsertSitePageRequest request,
        CancellationToken cancellationToken)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub");

        var page = await dbContext.SitePageContents
            .FirstOrDefaultAsync(x => x.Slug == slug, cancellationToken);

        if (page is null)
        {
            page = new SitePageContent
            {
                Slug = slug,
                Title = request.Title,
                Body = request.Body,
                UpdatedByUserId = userId,
                UpdatedAtUtc = DateTime.UtcNow
            };

            await dbContext.SitePageContents.AddAsync(page, cancellationToken);
        }
        else
        {
            page.Title = request.Title;
            page.Body = request.Body;
            page.UpdatedByUserId = userId;
            page.UpdatedAtUtc = DateTime.UtcNow;
        }

        await dbContext.SaveChangesAsync(cancellationToken);
        return ActionResultInstance(CustomResponse<NoContent>.Success(200));
    }

    [HttpGet("ad-slots")]
    public async Task<IActionResult> GetAdSlots(CancellationToken cancellationToken)
    {
        var items = await dbContext.AdvertisementSlots
            .AsNoTracking()
            .OrderBy(x => x.Placement)
            .ThenBy(x => x.Priority)
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
    public async Task<IActionResult> CreateAdSlot([FromBody] UpsertAdSlotRequest request, CancellationToken cancellationToken)
    {
        var slot = new AdvertisementSlot
        {
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
    public async Task<IActionResult> UpdateAdSlot(
        Guid id,
        [FromBody] UpsertAdSlotRequest request,
        CancellationToken cancellationToken)
    {
        var slot = await dbContext.AdvertisementSlots.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
        if (slot is null)
            return ActionResultInstance(CustomResponse<NoContent>.Fail("Slot not found.", 404, true));

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
    public async Task<IActionResult> DeleteAdSlot(Guid id, CancellationToken cancellationToken)
    {
        var slot = await dbContext.AdvertisementSlots.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
        if (slot is null)
            return ActionResultInstance(CustomResponse<NoContent>.Fail("Slot not found.", 404, true));

        dbContext.AdvertisementSlots.Remove(slot);
        await dbContext.SaveChangesAsync(cancellationToken);
        return ActionResultInstance(CustomResponse<NoContent>.Success(200));
    }

    [HttpGet("featured-listings")]
    public async Task<IActionResult> GetFeaturedListings(CancellationToken cancellationToken)
    {
        var list = await dbContext.FeaturedListings
            .AsNoTracking()
            .Include(x => x.Event)
            .OrderBy(x => x.Placement)
            .ThenBy(x => x.Priority)
            .Select(x => new
            {
                x.Id,
                x.EventId,
                EventTitle = x.Event != null ? x.Event.Title : string.Empty,
                x.Placement,
                x.Priority,
                x.ExpiresAtUtc,
                x.IsActive
            })
            .ToListAsync(cancellationToken);

        return ActionResultInstance(CustomResponse<object>.Success(list, 200));
    }

    [HttpPost("featured-listings")]
    public async Task<IActionResult> CreateFeaturedListing(
        [FromBody] CreateFeaturedListingRequest request,
        CancellationToken cancellationToken)
    {
        var eventExists = await dbContext.Events.AnyAsync(x => x.Id == request.EventId, cancellationToken);
        if (!eventExists)
            return ActionResultInstance(CustomResponse<NoContent>.Fail("Event not found.", 404, true));

        var featured = new FeaturedListing
        {
            EventId = request.EventId,
            Placement = request.Placement,
            Priority = request.Priority,
            ExpiresAtUtc = request.ExpiresAtUtc,
            IsActive = request.IsActive
        };

        await dbContext.FeaturedListings.AddAsync(featured, cancellationToken);
        await dbContext.SaveChangesAsync(cancellationToken);
        return ActionResultInstance(CustomResponse<object>.Success(new { featured.Id }, 201));
    }

    [HttpDelete("featured-listings/{id:guid}")]
    public async Task<IActionResult> DeleteFeaturedListing(Guid id, CancellationToken cancellationToken)
    {
        var row = await dbContext.FeaturedListings.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
        if (row is null)
            return ActionResultInstance(CustomResponse<NoContent>.Fail("Featured listing not found.", 404, true));

        dbContext.FeaturedListings.Remove(row);
        await dbContext.SaveChangesAsync(cancellationToken);
        return ActionResultInstance(CustomResponse<NoContent>.Success(200));
    }

    [HttpPost("events/{eventId:guid}/gallery")]
    public async Task<IActionResult> AddGalleryItem(
        Guid eventId,
        [FromBody] CreateGalleryItemRequest request,
        CancellationToken cancellationToken)
    {
        var eventExists = await dbContext.Events.AnyAsync(x => x.Id == eventId, cancellationToken);
        if (!eventExists)
            return ActionResultInstance(CustomResponse<NoContent>.Fail("Event not found.", 404, true));

        var item = new EventGalleryItem
        {
            EventId = eventId,
            ImageUrl = request.ImageUrl,
            Caption = request.Caption,
            SortOrder = request.SortOrder
        };

        await dbContext.EventGalleryItems.AddAsync(item, cancellationToken);
        await dbContext.SaveChangesAsync(cancellationToken);
        return ActionResultInstance(CustomResponse<object>.Success(new { item.Id }, 201));
    }

    [HttpGet("messages/inbox")]
    public async Task<IActionResult> GetInbox(CancellationToken cancellationToken)
    {
        var items = await dbContext.EventMessages
            .AsNoTracking()
            .Include(x => x.Event)
            .OrderByDescending(x => x.CreatedAtUtc)
            .Select(x => new
            {
                x.Id,
                x.EventId,
                EventTitle = x.Event != null ? x.Event.Title : string.Empty,
                x.SenderName,
                x.SenderEmail,
                x.MessageText,
                x.CreatedAtUtc
            })
            .ToListAsync(cancellationToken);

        return ActionResultInstance(CustomResponse<object>.Success(items, 200));
    }
}

public record NotifyMembersRequest(
    string EventTitle,
    string EventDescription,
    DateTime StartDate,
    string City,
    decimal Price);

public record CreateMemberRequest(
    string FirstName,
    string LastName,
    string Email,
    string Password,
    string Role);

public record SetMemberStatusRequest(bool IsActive);
public record UpsertSitePageRequest(string Title, string Body);
public record UpsertAdSlotRequest(
    string Placement,
    string Title,
    string Subtitle,
    string? LinkUrl,
    string? ImageUrl,
    bool IsActive,
    int Priority,
    DateTime? StartDateUtc,
    DateTime? EndDateUtc);
public record CreateFeaturedListingRequest(
    Guid EventId,
    string Placement,
    int Priority,
    DateTime? ExpiresAtUtc,
    bool IsActive);
public record CreateGalleryItemRequest(string ImageUrl, string? Caption, int SortOrder);

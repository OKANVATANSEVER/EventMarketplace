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
    private static readonly HashSet<string> AllowedAdSlotPlacements = new(StringComparer.OrdinalIgnoreCase)
    {
        "home-hero",
        "detail-sidebar"
    };

    private static readonly HashSet<string> AllowedFeaturedPlacements = new(StringComparer.OrdinalIgnoreCase)
    {
        "home-carousel"
    };

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
                x.CreatedAtUtc,
                x.ActiveSinceUtc,
                x.LastStatusChangedAtUtc,
                TotalPublishedDurationSeconds = GetPublishedDurationSeconds(x.IsActive, x.ActiveSinceUtc, x.TotalActiveDurationSeconds),
                x.StartDateUtc,
                x.EndDateUtc
            })
            .ToListAsync(cancellationToken);

        return ActionResultInstance(CustomResponse<object>.Success(items, 200));
    }

    [HttpPost("ad-slots")]
    public async Task<IActionResult> CreateAdSlot([FromBody] UpsertAdSlotRequest request, CancellationToken cancellationToken)
    {
        var placement = request.Placement.Trim();
        if (!AllowedAdSlotPlacements.Contains(placement))
            return ActionResultInstance(CustomResponse<NoContent>.Fail($"Invalid placement. Allowed values: {string.Join(", ", AllowedAdSlotPlacements)}", 400, true));

        var now = DateTime.UtcNow;
        var slot = new AdvertisementSlot
        {
            Placement = placement,
            Title = request.Title,
            Subtitle = request.Subtitle,
            LinkUrl = request.LinkUrl,
            ImageUrl = request.ImageUrl,
            IsActive = request.IsActive,
            CreatedAtUtc = now,
            ActiveSinceUtc = request.IsActive ? now : null,
            LastStatusChangedAtUtc = now,
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
        var placement = request.Placement.Trim();
        if (!AllowedAdSlotPlacements.Contains(placement))
            return ActionResultInstance(CustomResponse<NoContent>.Fail($"Invalid placement. Allowed values: {string.Join(", ", AllowedAdSlotPlacements)}", 400, true));

        var slot = await dbContext.AdvertisementSlots.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
        if (slot is null)
            return ActionResultInstance(CustomResponse<NoContent>.Fail("Slot not found.", 404, true));

        slot.Placement = placement;
        slot.Title = request.Title;
        slot.Subtitle = request.Subtitle;
        slot.LinkUrl = request.LinkUrl;
        slot.ImageUrl = request.ImageUrl;
        slot.Priority = request.Priority;
        slot.StartDateUtc = request.StartDateUtc;
        slot.EndDateUtc = request.EndDateUtc;
        UpdateActivationTracking(slot, request.IsActive);

        await dbContext.SaveChangesAsync(cancellationToken);
        return ActionResultInstance(CustomResponse<NoContent>.Success(200));
    }

    [HttpDelete("ad-slots/{id:guid}")]
    public async Task<IActionResult> DeleteAdSlot(Guid id, CancellationToken cancellationToken)
    {
        var slot = await dbContext.AdvertisementSlots.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
        if (slot is null)
            return ActionResultInstance(CustomResponse<NoContent>.Fail("Slot not found.", 404, true));

        // Soft delete: keep the history, only deactivate the slot.
        UpdateActivationTracking(slot, false);
        slot.EndDateUtc ??= DateTime.UtcNow;
        await dbContext.SaveChangesAsync(cancellationToken);
        return ActionResultInstance(CustomResponse<NoContent>.Success(200));
    }

    [HttpGet("ad-slots/{id:guid}/status-history")]
    public async Task<IActionResult> GetAdSlotStatusHistory(Guid id, CancellationToken cancellationToken)
    {
        var history = await dbContext.SlotStatusAudits
            .AsNoTracking()
            .Where(x => x.SlotType == "AdvertisementSlot" && x.SlotId == id)
            .OrderByDescending(x => x.ChangedAtUtc)
            .Select(x => new
            {
                x.Id,
                x.SlotType,
                x.SlotId,
                x.PreviousIsActive,
                x.NewIsActive,
                x.ChangedAtUtc,
                x.ChangedByUserId,
                x.ChangedByEmail,
                x.ChangedByRole,
                x.RequestIp,
                ChangedBy = !string.IsNullOrWhiteSpace(x.ChangedByEmail)
                    ? x.ChangedByEmail
                    : (!string.IsNullOrWhiteSpace(x.ChangedByUserId) ? x.ChangedByUserId : "System"),
                ActionLabel = x.NewIsActive ? "Aktife Alindi" : "Pasife Alindi"
            })
            .ToListAsync(cancellationToken);

        return ActionResultInstance(CustomResponse<object>.Success(history, 200));
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
                x.IsActive,
                x.CreatedAtUtc,
                x.ActiveSinceUtc,
                x.LastStatusChangedAtUtc,
                TotalPublishedDurationSeconds = GetPublishedDurationSeconds(x.IsActive, x.ActiveSinceUtc, x.TotalActiveDurationSeconds)
            })
            .ToListAsync(cancellationToken);

        return ActionResultInstance(CustomResponse<object>.Success(list, 200));
    }

    [HttpPost("featured-listings")]
    public async Task<IActionResult> CreateFeaturedListing(
        [FromBody] CreateFeaturedListingRequest request,
        CancellationToken cancellationToken)
    {
        var placement = request.Placement.Trim();
        if (!AllowedFeaturedPlacements.Contains(placement))
            return ActionResultInstance(CustomResponse<NoContent>.Fail($"Invalid placement. Allowed values: {string.Join(", ", AllowedFeaturedPlacements)}", 400, true));

        var eventExists = await dbContext.Events.AnyAsync(x => x.Id == request.EventId, cancellationToken);
        if (!eventExists)
            return ActionResultInstance(CustomResponse<NoContent>.Fail("Event not found.", 404, true));

        var now = DateTime.UtcNow;
        var featured = new FeaturedListing
        {
            EventId = request.EventId,
            Placement = placement,
            Priority = request.Priority,
            ExpiresAtUtc = request.ExpiresAtUtc,
            IsActive = request.IsActive,
            CreatedAtUtc = now,
            ActiveSinceUtc = request.IsActive ? now : null,
            LastStatusChangedAtUtc = now
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

        // Soft delete: keep the history, only deactivate the listing.
        UpdateActivationTracking(row, false);
        row.ExpiresAtUtc ??= DateTime.UtcNow;
        await dbContext.SaveChangesAsync(cancellationToken);
        return ActionResultInstance(CustomResponse<NoContent>.Success(200));
    }

    [HttpGet("featured-listings/{id:guid}/status-history")]
    public async Task<IActionResult> GetFeaturedListingStatusHistory(Guid id, CancellationToken cancellationToken)
    {
        var history = await dbContext.SlotStatusAudits
            .AsNoTracking()
            .Where(x => x.SlotType == "FeaturedListing" && x.SlotId == id)
            .OrderByDescending(x => x.ChangedAtUtc)
            .Select(x => new
            {
                x.Id,
                x.SlotType,
                x.SlotId,
                x.PreviousIsActive,
                x.NewIsActive,
                x.ChangedAtUtc,
                x.ChangedByUserId,
                x.ChangedByEmail,
                x.ChangedByRole,
                x.RequestIp,
                ChangedBy = !string.IsNullOrWhiteSpace(x.ChangedByEmail)
                    ? x.ChangedByEmail
                    : (!string.IsNullOrWhiteSpace(x.ChangedByUserId) ? x.ChangedByUserId : "System"),
                ActionLabel = x.NewIsActive ? "Aktife Alindi" : "Pasife Alindi"
            })
            .ToListAsync(cancellationToken);

        return ActionResultInstance(CustomResponse<object>.Success(history, 200));
    }

    [HttpPut("featured-listings/{id:guid}")]
    public async Task<IActionResult> UpdateFeaturedListing(
        Guid id,
        [FromBody] CreateFeaturedListingRequest request,
        CancellationToken cancellationToken)
    {
        var placement = request.Placement.Trim();
        if (!AllowedFeaturedPlacements.Contains(placement))
            return ActionResultInstance(CustomResponse<NoContent>.Fail($"Invalid placement. Allowed values: {string.Join(", ", AllowedFeaturedPlacements)}", 400, true));

        var row = await dbContext.FeaturedListings.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
        if (row is null)
            return ActionResultInstance(CustomResponse<NoContent>.Fail("Featured listing not found.", 404, true));

        var eventExists = await dbContext.Events.AnyAsync(x => x.Id == request.EventId, cancellationToken);
        if (!eventExists)
            return ActionResultInstance(CustomResponse<NoContent>.Fail("Event not found.", 404, true));

        row.EventId = request.EventId;
        row.Placement = placement;
        row.Priority = request.Priority;
        row.ExpiresAtUtc = request.ExpiresAtUtc;
        UpdateActivationTracking(row, request.IsActive);

        await dbContext.SaveChangesAsync(cancellationToken);
        return ActionResultInstance(CustomResponse<NoContent>.Success(200));
    }

    [HttpGet("popular-venues")]
    public async Task<IActionResult> GetPopularVenues(CancellationToken cancellationToken)
    {
        var venues = await dbContext.PopularVenues
            .AsNoTracking()
            .OrderBy(x => x.SortOrder)
            .ThenBy(x => x.Name)
            .Select(x => new
            {
                x.Id,
                x.Name,
                x.City,
                x.Tag,
                x.ImageUrl,
                x.SortOrder,
                x.IsActive,
                x.CreatedAtUtc,
                x.UpdatedAtUtc
            })
            .ToListAsync(cancellationToken);

        return ActionResultInstance(CustomResponse<object>.Success(venues, 200));
    }

    [HttpPost("popular-venues")]
    public async Task<IActionResult> CreatePopularVenue(
        [FromBody] UpsertPopularVenueRequest request,
        CancellationToken cancellationToken)
    {
        var venue = new PopularVenue
        {
            Name = request.Name,
            City = request.City,
            Tag = request.Tag,
            ImageUrl = request.ImageUrl,
            SortOrder = request.SortOrder,
            IsActive = request.IsActive,
            CreatedAtUtc = DateTime.UtcNow,
            UpdatedAtUtc = DateTime.UtcNow
        };

        await dbContext.PopularVenues.AddAsync(venue, cancellationToken);
        await dbContext.SaveChangesAsync(cancellationToken);
        return ActionResultInstance(CustomResponse<object>.Success(new { venue.Id }, 201));
    }

    [HttpPut("popular-venues/{id:guid}")]
    public async Task<IActionResult> UpdatePopularVenue(
        Guid id,
        [FromBody] UpsertPopularVenueRequest request,
        CancellationToken cancellationToken)
    {
        var venue = await dbContext.PopularVenues.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
        if (venue is null)
            return ActionResultInstance(CustomResponse<NoContent>.Fail("Venue not found.", 404, true));

        venue.Name = request.Name;
        venue.City = request.City;
        venue.Tag = request.Tag;
        venue.ImageUrl = request.ImageUrl;
        venue.SortOrder = request.SortOrder;
        venue.IsActive = request.IsActive;
        venue.UpdatedAtUtc = DateTime.UtcNow;

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

    private static long GetPublishedDurationSeconds(bool isActive, DateTime? activeSinceUtc, long totalActiveDurationSeconds)
    {
        if (!isActive || activeSinceUtc is null)
            return totalActiveDurationSeconds;

        return totalActiveDurationSeconds + (long)Math.Max(0, (DateTime.UtcNow - activeSinceUtc.Value).TotalSeconds);
    }

    private void UpdateActivationTracking(AdvertisementSlot slot, bool newIsActive)
    {
        if (slot.IsActive == newIsActive)
            return;

        var previousIsActive = slot.IsActive;
        var now = DateTime.UtcNow;
        slot.LastStatusChangedAtUtc = now;

        if (newIsActive)
        {
            slot.IsActive = true;
            slot.ActiveSinceUtc = now;
            AddSlotStatusAudit("AdvertisementSlot", slot.Id, previousIsActive, true, now);
            return;
        }

        if (slot.ActiveSinceUtc is not null)
            slot.TotalActiveDurationSeconds += (long)Math.Max(0, (now - slot.ActiveSinceUtc.Value).TotalSeconds);

        slot.IsActive = false;
        slot.ActiveSinceUtc = null;
        AddSlotStatusAudit("AdvertisementSlot", slot.Id, previousIsActive, false, now);
    }

    private void UpdateActivationTracking(FeaturedListing row, bool newIsActive)
    {
        if (row.IsActive == newIsActive)
            return;

        var previousIsActive = row.IsActive;
        var now = DateTime.UtcNow;
        row.LastStatusChangedAtUtc = now;

        if (newIsActive)
        {
            row.IsActive = true;
            row.ActiveSinceUtc = now;
            AddSlotStatusAudit("FeaturedListing", row.Id, previousIsActive, true, now);
            return;
        }

        if (row.ActiveSinceUtc is not null)
            row.TotalActiveDurationSeconds += (long)Math.Max(0, (now - row.ActiveSinceUtc.Value).TotalSeconds);

        row.IsActive = false;
        row.ActiveSinceUtc = null;
        AddSlotStatusAudit("FeaturedListing", row.Id, previousIsActive, false, now);
    }

    private void AddSlotStatusAudit(
        string slotType,
        Guid slotId,
        bool previousIsActive,
        bool newIsActive,
        DateTime changedAtUtc)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub");
        var email = User.FindFirstValue(ClaimTypes.Email) ?? User.FindFirstValue("email");
        var role = string.Join(", ", User.FindAll(ClaimTypes.Role).Select(x => x.Value).Distinct());
        if (string.IsNullOrWhiteSpace(role))
        {
            role = string.Join(", ", User.FindAll("role").Select(x => x.Value).Distinct());
        }

        var requestIp = HttpContext.Connection.RemoteIpAddress?.ToString();

        dbContext.SlotStatusAudits.Add(new SlotStatusAudit
        {
            SlotType = slotType,
            SlotId = slotId,
            PreviousIsActive = previousIsActive,
            NewIsActive = newIsActive,
            ChangedAtUtc = changedAtUtc,
            ChangedByUserId = userId,
            ChangedByEmail = email,
            ChangedByRole = string.IsNullOrWhiteSpace(role) ? null : role,
            RequestIp = requestIp
        });
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
public record UpsertPopularVenueRequest(
    string Name,
    string City,
    string Tag,
    string? ImageUrl,
    int SortOrder,
    bool IsActive);
public record CreateGalleryItemRequest(string ImageUrl, string? Caption, int SortOrder);

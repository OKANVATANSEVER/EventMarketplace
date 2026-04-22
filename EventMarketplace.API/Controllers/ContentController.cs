using EventMarketplace.API.Responses;
using EventMarketplace.Infrastructure.Persistence;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EventMarketplace.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ContentController(ApplicationDbContext dbContext) : CustomBaseController
{
    [HttpGet("pages/{slug}")]
    [AllowAnonymous]
    public async Task<IActionResult> GetPage(string slug, CancellationToken cancellationToken)
    {
        var page = await dbContext.SitePageContents
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Slug == slug, cancellationToken);

        if (page is null)
            return ActionResultInstance(CustomResponse<NoContent>.Fail("Page not found.", 404, true));

        return ActionResultInstance(CustomResponse<object>.Success(new
        {
            page.Slug,
            page.Title,
            page.Body,
            page.UpdatedAtUtc
        }, 200));
    }

    [HttpGet("ad-slots")]
    [AllowAnonymous]
    public async Task<IActionResult> GetAdSlots([FromQuery] string? placement, CancellationToken cancellationToken)
    {
        var now = DateTime.UtcNow;

        var query = dbContext.AdvertisementSlots
            .AsNoTracking()
            .Where(x => x.IsActive)
            .Where(x => x.StartDateUtc == null || x.StartDateUtc <= now)
            .Where(x => x.EndDateUtc == null || x.EndDateUtc >= now);

        if (!string.IsNullOrWhiteSpace(placement))
            query = query.Where(x => x.Placement == placement);

        var items = await query
            .OrderBy(x => x.Priority)
            .Select(x => new
            {
                x.Id,
                x.Placement,
                x.Title,
                x.Subtitle,
                x.LinkUrl,
                x.ImageUrl,
                x.Priority
            })
            .ToListAsync(cancellationToken);

        return ActionResultInstance(CustomResponse<object>.Success(items, 200));
    }

    [HttpGet("featured")]
    [AllowAnonymous]
    public async Task<IActionResult> GetFeatured([FromQuery] string placement = "home-carousel", CancellationToken cancellationToken = default)
    {
        var now = DateTime.UtcNow;

        var items = await dbContext.FeaturedListings
            .AsNoTracking()
            .Include(x => x.Event)
            .Where(x => x.IsActive && x.Placement == placement)
            .Where(x => x.ExpiresAtUtc == null || x.ExpiresAtUtc >= now)
            .Where(x => x.Event != null && x.Event.IsApproved)
            .OrderBy(x => x.Priority)
            .Select(x => new
            {
                x.EventId,
                EventTitle = x.Event!.Title,
                x.Event!.City,
                x.Event.StartDate,
                x.Event.Price,
                CategoryName = x.Event.Category != null ? x.Event.Category.Name : string.Empty
            })
            .ToListAsync(cancellationToken);

        return ActionResultInstance(CustomResponse<object>.Success(items, 200));
    }

    [HttpGet("events/{eventId:guid}/interactions")]
    [AllowAnonymous]
    public async Task<IActionResult> GetEventInteractions(Guid eventId, CancellationToken cancellationToken)
    {
        var eventExists = await dbContext.Events.AnyAsync(x => x.Id == eventId, cancellationToken);
        if (!eventExists)
            return ActionResultInstance(CustomResponse<NoContent>.Fail("Event not found.", 404, true));

        var comments = await dbContext.EventComments
            .AsNoTracking()
            .Where(x => x.EventId == eventId && x.IsApproved)
            .OrderByDescending(x => x.CreatedAtUtc)
            .Take(20)
            .Select(x => new
            {
                x.Id,
                x.DisplayName,
                x.Content,
                x.CreatedAtUtc
            })
            .ToListAsync(cancellationToken);

        var gallery = await dbContext.EventGalleryItems
            .AsNoTracking()
            .Where(x => x.EventId == eventId)
            .OrderBy(x => x.SortOrder)
            .Select(x => new
            {
                x.Id,
                x.ImageUrl,
                x.Caption,
                x.SortOrder
            })
            .ToListAsync(cancellationToken);

        return ActionResultInstance(CustomResponse<object>.Success(new
        {
            comments,
            gallery
        }, 200));
    }

    [HttpPost("events/{eventId:guid}/comments")]
    [AllowAnonymous]
    public async Task<IActionResult> AddComment(
        Guid eventId,
        [FromBody] AddCommentRequest request,
        CancellationToken cancellationToken)
    {
        var eventExists = await dbContext.Events.AnyAsync(x => x.Id == eventId, cancellationToken);
        if (!eventExists)
            return ActionResultInstance(CustomResponse<NoContent>.Fail("Event not found.", 404, true));

        var comment = new Domain.Entities.EventComment
        {
            EventId = eventId,
            DisplayName = request.DisplayName,
            Content = request.Content,
            IsApproved = true,
            CreatedAtUtc = DateTime.UtcNow
        };

        await dbContext.EventComments.AddAsync(comment, cancellationToken);
        await dbContext.SaveChangesAsync(cancellationToken);
        return ActionResultInstance(CustomResponse<NoContent>.Success(201));
    }

    [HttpPost("events/{eventId:guid}/messages")]
    [AllowAnonymous]
    public async Task<IActionResult> SendMessage(
        Guid eventId,
        [FromBody] SendMessageRequest request,
        CancellationToken cancellationToken)
    {
        var eventExists = await dbContext.Events.AnyAsync(x => x.Id == eventId, cancellationToken);
        if (!eventExists)
            return ActionResultInstance(CustomResponse<NoContent>.Fail("Event not found.", 404, true));

        var message = new Domain.Entities.EventMessage
        {
            EventId = eventId,
            SenderName = request.SenderName,
            SenderEmail = request.SenderEmail,
            MessageText = request.MessageText,
            CreatedAtUtc = DateTime.UtcNow
        };

        await dbContext.EventMessages.AddAsync(message, cancellationToken);
        await dbContext.SaveChangesAsync(cancellationToken);

        return ActionResultInstance(CustomResponse<NoContent>.Success(201));
    }
}

public record AddCommentRequest(string DisplayName, string Content);
public record SendMessageRequest(string SenderName, string SenderEmail, string MessageText);

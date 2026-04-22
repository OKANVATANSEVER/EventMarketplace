using EventMarketplace.Web.Models;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;

namespace EventMarketplace.Web.Services;

public sealed class ApiClient(HttpClient http, TokenStore tokenStore)
{
    private static readonly JsonSerializerOptions Json = new(JsonSerializerDefaults.Web);

    private void AttachAuth()
    {
        http.DefaultRequestHeaders.Authorization = tokenStore.AccessToken is not null
            ? new AuthenticationHeaderValue("Bearer", tokenStore.AccessToken)
            : null;
    }

    // ── AUTH ────────────────────────────────────────────────────────────────

    public async Task<(bool Success, string? Token, string? Error)> LoginAsync(
        string email, string password)
    {
        var response = await http.PostAsJsonAsync("api/auth/login",
            new { email, password }, Json);

        if (!response.IsSuccessStatusCode)
            return (false, null, "E-posta veya şifre hatalı.");

        var envelope = await response.Content
            .ReadFromJsonAsync<ApiEnvelope<AuthTokensModel>>(Json);

        return envelope?.IsSuccessful == true && envelope.Data is not null
            ? (true, envelope.Data.AccessToken, null)
            : (false, null, "Giriş başarısız.");
    }

    public async Task<(bool Success, string? Error)> RegisterAsync(
        string firstName, string lastName, string email, string password)
    {
        var response = await http.PostAsJsonAsync("api/auth/register",
            new { firstName, lastName, email, password }, Json);

        if (response.IsSuccessStatusCode) return (true, null);

        var body = await response.Content.ReadAsStringAsync();
        return (false, "Kayıt başarısız. E-posta zaten kayıtlı olabilir.");
    }

    // ── EVENTS (public) ─────────────────────────────────────────────────────

    public async Task<PagedResultModel<EventListItemModel>?> GetEventsAsync(
        string? city = null, Guid? categoryId = null, bool? isFeatured = null, bool? isApproved = null, string? title = null, int page = 1, int pageSize = 9)
    {
        var url = $"api/events?page={page}&pageSize={pageSize}";
        if (!string.IsNullOrWhiteSpace(city)) url += $"&city={Uri.EscapeDataString(city)}";
        if (categoryId.HasValue) url += $"&categoryId={categoryId.Value}";
        if (isFeatured.HasValue) url += $"&isFeatured={isFeatured.Value}";
        if (isApproved.HasValue) url += $"&isApproved={isApproved.Value}";
        if (!string.IsNullOrWhiteSpace(title)) url += $"&title={Uri.EscapeDataString(title)}";

        var response = await http.GetAsync(url);
        if (!response.IsSuccessStatusCode) return null;

        var envelope = await response.Content
            .ReadFromJsonAsync<ApiEnvelope<PagedResultModel<EventListItemModel>>>(Json);
        return envelope?.Data;
    }

    public async Task<EventDetailModel?> GetEventByIdAsync(Guid id)
    {
        var response = await http.GetAsync($"api/events/{id}");
        if (!response.IsSuccessStatusCode) return null;

        var envelope = await response.Content
            .ReadFromJsonAsync<ApiEnvelope<EventDetailModel>>(Json);
        return envelope?.Data;
    }

    // ── ADMIN ────────────────────────────────────────────────────────────────

    public async Task<AdminDashboardStatsModel?> GetDashboardStatsAsync(
        CancellationToken ct = default)
    {
        AttachAuth();
        var response = await http.GetAsync("api/admin/dashboard", ct);
        if (!response.IsSuccessStatusCode) return null;

        var envelope = await response.Content
            .ReadFromJsonAsync<ApiEnvelope<AdminDashboardStatsModel>>(Json, ct);
        return envelope?.Data;
    }

    public async Task<List<MemberModel>> GetAllMembersAsync(CancellationToken ct = default)
    {
        AttachAuth();
        var response = await http.GetAsync("api/admin/members", ct);
        if (!response.IsSuccessStatusCode) return [];

        var envelope = await response.Content
            .ReadFromJsonAsync<ApiEnvelope<List<MemberModel>>>(Json, ct);
        return envelope?.Data ?? [];
    }

    public async Task<List<EventListItemModel>> GetEventsThisMonthAsync(
        CancellationToken ct = default)
    {
        AttachAuth();
        var startOfMonth = new DateTime(DateTime.UtcNow.Year, DateTime.UtcNow.Month, 1,
            0, 0, 0, DateTimeKind.Utc);

        // Fetch a large page and filter client-side (API has no date filter yet)
        var response = await http.GetAsync("api/events?page=1&pageSize=200", ct);
        if (!response.IsSuccessStatusCode) return [];

        var envelope = await response.Content
            .ReadFromJsonAsync<ApiEnvelope<PagedResultModel<EventListItemModel>>>(Json, ct);

        return envelope?.Data?.Items
            .Where(e => e.StartDate >= startOfMonth)
            .ToList() ?? [];
    }

    public async Task<bool> NotifyMembersAsync(
        Guid eventId, string eventTitle, string eventDescription,
        DateTime startDate, string city, decimal price,
        CancellationToken ct = default)
    {
        AttachAuth();
        var body = new { eventTitle, eventDescription, startDate, city, price };
        var response = await http.PostAsJsonAsync(
            $"api/admin/events/{eventId}/notify", body, Json, ct);
        return response.IsSuccessStatusCode;
    }

    // ── ADMIN: EVENTS ────────────────────────────────────────────────────────

    public async Task<List<EventListItemModel>> GetAllEventsAdminAsync(CancellationToken ct = default)
    {
        AttachAuth();
        var response = await http.GetAsync("api/events?page=1&pageSize=500", ct);
        if (!response.IsSuccessStatusCode) return [];
        var envelope = await response.Content
            .ReadFromJsonAsync<ApiEnvelope<PagedResultModel<EventListItemModel>>>(Json, ct);
        return envelope?.Data?.Items.ToList() ?? [];
    }

    public async Task<(bool Success, string Error)> CreateEventAsync(
        string title, string description, decimal price,
        DateTime startDate, DateTime endDate, string city,
        Guid categoryId, bool isFeatured, bool isApproved,
        CancellationToken ct = default)
    {
        AttachAuth();
        var body = new { title, description, price, startDate, endDate, city, categoryId, isFeatured, isApproved };
        var response = await http.PostAsJsonAsync("api/events", body, Json, ct);
        if (response.IsSuccessStatusCode) return (true, string.Empty);
        var err = await response.Content.ReadAsStringAsync(ct);
        return (false, err);
    }

    public async Task<(bool Success, string Error)> UpdateEventAsync(
        Guid id, string title, string description, decimal price,
        DateTime startDate, DateTime endDate, string city,
        Guid categoryId, bool isFeatured, bool isApproved,
        CancellationToken ct = default)
    {
        AttachAuth();
        var body = new { title, description, price, startDate, endDate, city, categoryId, isFeatured, isApproved };
        var response = await http.PutAsJsonAsync($"api/events/{id}", body, Json, ct);
        if (response.IsSuccessStatusCode) return (true, string.Empty);
        var err = await response.Content.ReadAsStringAsync(ct);
        return (false, err);
    }

    public async Task<List<CategoryModel>> GetCategoriesAsync(CancellationToken ct = default)
    {
        AttachAuth();
        var response = await http.GetAsync("api/admin/categories", ct);
        if (!response.IsSuccessStatusCode) return [];
        var envelope = await response.Content
            .ReadFromJsonAsync<ApiEnvelope<List<CategoryModel>>>(Json, ct);
        return envelope?.Data ?? [];
    }

    // ── ADMIN: MEMBERS ───────────────────────────────────────────────────────

    public async Task<(bool Success, string Error)> CreateMemberAsync(
        string firstName, string lastName, string email, string password, string role,
        CancellationToken ct = default)
    {
        AttachAuth();
        var body = new { firstName, lastName, email, password, role };
        var response = await http.PostAsJsonAsync("api/admin/members", body, Json, ct);
        if (response.IsSuccessStatusCode) return (true, string.Empty);
        var err = await response.Content.ReadAsStringAsync(ct);
        return (false, err);
    }

    public async Task<bool> SetMemberActiveStatusAsync(string userId, bool isActive, CancellationToken ct = default)
    {
        AttachAuth();
        var body = new { isActive };
        var response = await http.PutAsJsonAsync($"api/admin/members/{userId}/status", body, Json, ct);
        return response.IsSuccessStatusCode;
    }

    // ── CONTENT (public) ───────────────────────────────────────────────────

    public async Task<SitePageContentModel?> GetSitePageAsync(string slug, CancellationToken ct = default)
    {
        var response = await http.GetAsync($"api/content/pages/{slug}", ct);
        if (!response.IsSuccessStatusCode) return null;

        var envelope = await response.Content
            .ReadFromJsonAsync<ApiEnvelope<SitePageContentModel>>(Json, ct);
        return envelope?.Data;
    }

    public async Task<List<AdvertisementSlotModel>> GetPublicAdSlotsAsync(string? placement = null, CancellationToken ct = default)
    {
        var url = string.IsNullOrWhiteSpace(placement)
            ? "api/content/ad-slots"
            : $"api/content/ad-slots?placement={Uri.EscapeDataString(placement)}";

        var response = await http.GetAsync(url, ct);
        if (!response.IsSuccessStatusCode) return [];

        var envelope = await response.Content
            .ReadFromJsonAsync<ApiEnvelope<List<AdvertisementSlotModel>>>(Json, ct);
        return envelope?.Data ?? [];
    }

    public async Task<List<PublicFeaturedEventModel>> GetPublicFeaturedAsync(string placement, CancellationToken ct = default)
    {
        var response = await http.GetAsync($"api/content/featured?placement={Uri.EscapeDataString(placement)}", ct);
        if (!response.IsSuccessStatusCode) return [];

        var envelope = await response.Content
            .ReadFromJsonAsync<ApiEnvelope<List<PublicFeaturedEventModel>>>(Json, ct);
        return envelope?.Data ?? [];
    }

    public async Task<EventInteractionsModel?> GetEventInteractionsAsync(Guid eventId, CancellationToken ct = default)
    {
        var response = await http.GetAsync($"api/content/events/{eventId}/interactions", ct);
        if (!response.IsSuccessStatusCode) return null;

        var envelope = await response.Content
            .ReadFromJsonAsync<ApiEnvelope<EventInteractionsModel>>(Json, ct);
        return envelope?.Data;
    }

    public async Task<bool> AddEventCommentAsync(Guid eventId, string displayName, string content, CancellationToken ct = default)
    {
        var body = new { displayName, content };
        var response = await http.PostAsJsonAsync($"api/content/events/{eventId}/comments", body, Json, ct);
        return response.IsSuccessStatusCode;
    }

    public async Task<bool> SendEventMessageAsync(
        Guid eventId,
        string senderName,
        string senderEmail,
        string messageText,
        CancellationToken ct = default)
    {
        var body = new { senderName, senderEmail, messageText };
        var response = await http.PostAsJsonAsync($"api/content/events/{eventId}/messages", body, Json, ct);
        return response.IsSuccessStatusCode;
    }

    // ── CONTENT (admin) ────────────────────────────────────────────────────

    public async Task<SitePageContentModel?> GetAdminSitePageAsync(string slug, CancellationToken ct = default)
    {
        AttachAuth();
        var response = await http.GetAsync($"api/admin/site-pages/{slug}", ct);
        if (!response.IsSuccessStatusCode) return null;

        var envelope = await response.Content
            .ReadFromJsonAsync<ApiEnvelope<SitePageContentModel>>(Json, ct);
        return envelope?.Data;
    }

    public async Task<bool> UpsertAdminSitePageAsync(string slug, string title, string body, CancellationToken ct = default)
    {
        AttachAuth();
        var response = await http.PutAsJsonAsync($"api/admin/site-pages/{slug}", new { title, body }, Json, ct);
        return response.IsSuccessStatusCode;
    }

    public async Task<List<AdvertisementSlotModel>> GetAdminAdSlotsAsync(CancellationToken ct = default)
    {
        AttachAuth();
        var response = await http.GetAsync("api/admin/ad-slots", ct);
        if (!response.IsSuccessStatusCode) return [];

        var envelope = await response.Content
            .ReadFromJsonAsync<ApiEnvelope<List<AdvertisementSlotModel>>>(Json, ct);
        return envelope?.Data ?? [];
    }

    public async Task<bool> CreateAdSlotAsync(
        string placement,
        string title,
        string subtitle,
        string? linkUrl,
        string? imageUrl,
        bool isActive,
        int priority,
        DateTime? startDateUtc,
        DateTime? endDateUtc,
        CancellationToken ct = default)
    {
        AttachAuth();
        var body = new { placement, title, subtitle, linkUrl, imageUrl, isActive, priority, startDateUtc, endDateUtc };
        var response = await http.PostAsJsonAsync("api/admin/ad-slots", body, Json, ct);
        return response.IsSuccessStatusCode;
    }

    public async Task<bool> DeleteAdSlotAsync(Guid id, CancellationToken ct = default)
    {
        AttachAuth();
        var response = await http.DeleteAsync($"api/admin/ad-slots/{id}", ct);
        return response.IsSuccessStatusCode;
    }


    public async Task<bool> UpdateAdSlotAsync(
        Guid id,
        string placement,
        string title,
        string subtitle,
        string? linkUrl,
        string? imageUrl,
        bool isActive,
        int priority,
        DateTime? startDateUtc,
        DateTime? endDateUtc,
        CancellationToken ct = default)
    {
        AttachAuth();
        var body = new { placement, title, subtitle, linkUrl, imageUrl, isActive, priority, startDateUtc, endDateUtc };
        var response = await http.PutAsJsonAsync($"api/admin/ad-slots/{id}", body, Json, ct);
        return response.IsSuccessStatusCode;
    }

    public async Task<List<FeaturedListingModel>> GetFeaturedListingsAdminAsync(CancellationToken ct = default)
    {
        AttachAuth();
        var response = await http.GetAsync("api/admin/featured-listings", ct);
        if (!response.IsSuccessStatusCode) return [];

        var envelope = await response.Content
            .ReadFromJsonAsync<ApiEnvelope<List<FeaturedListingModel>>>(Json, ct);
        return envelope?.Data ?? [];
    }

    public async Task<bool> CreateFeaturedListingAsync(
        Guid eventId,
        string placement,
        int priority,
        DateTime? expiresAtUtc,
        bool isActive,
        CancellationToken ct = default)
    {
        AttachAuth();
        var body = new { eventId, placement, priority, expiresAtUtc, isActive };
        var response = await http.PostAsJsonAsync("api/admin/featured-listings", body, Json, ct);
        return response.IsSuccessStatusCode;
    }

    public async Task<bool> DeleteFeaturedListingAsync(Guid id, CancellationToken ct = default)
    {
        AttachAuth();
        var response = await http.DeleteAsync($"api/admin/featured-listings/{id}", ct);
        return response.IsSuccessStatusCode;
    }

    public async Task<bool> UpdateFeaturedListingAsync(
        Guid id,
        Guid eventId,
        string placement,
        int priority,
        DateTime? expiresAtUtc,
        bool isActive,
        CancellationToken ct = default)
    {
        AttachAuth();
        var body = new { eventId, placement, priority, expiresAtUtc, isActive };
        var response = await http.PutAsJsonAsync($"api/admin/featured-listings/{id}", body, Json, ct);
        return response.IsSuccessStatusCode;
    }


    public async Task<List<AdminInboxMessageModel>> GetAdminInboxAsync(CancellationToken ct = default)
    {
        AttachAuth();
        var response = await http.GetAsync("api/admin/messages/inbox", ct);
        if (!response.IsSuccessStatusCode) return [];

        var envelope = await response.Content
            .ReadFromJsonAsync<ApiEnvelope<List<AdminInboxMessageModel>>>(Json, ct);
        return envelope?.Data ?? [];
    }

    public async Task<bool> AddGalleryItemAsync(Guid eventId, string imageUrl, string? caption, int sortOrder, CancellationToken ct = default)
    {
        AttachAuth();
        var body = new { imageUrl, caption, sortOrder };
        var response = await http.PostAsJsonAsync($"api/admin/events/{eventId}/gallery", body, Json, ct);
        return response.IsSuccessStatusCode;
    }

    // ── ACCOUNT ───────────────────────────────────────────────────────────

    public async Task<AccountProfileModel?> GetMyProfileAsync(CancellationToken ct = default)
    {
        AttachAuth();
        var response = await http.GetAsync("api/account/profile", ct);
        if (!response.IsSuccessStatusCode) return null;

        var envelope = await response.Content
            .ReadFromJsonAsync<ApiEnvelope<AccountProfileModel>>(Json, ct);
        return envelope?.Data;
    }

    public async Task<bool> UpdateMyProfileAsync(string firstName, string lastName, string? phoneNumber, CancellationToken ct = default)
    {
        AttachAuth();
        var response = await http.PutAsJsonAsync("api/account/profile", new { firstName, lastName, phoneNumber }, Json, ct);
        return response.IsSuccessStatusCode;
    }

    public async Task<bool> ChangeMyPasswordAsync(string currentPassword, string newPassword, CancellationToken ct = default)
    {
        AttachAuth();
        var response = await http.PostAsJsonAsync("api/account/change-password", new { currentPassword, newPassword }, Json, ct);
        return response.IsSuccessStatusCode;
    }

    public async Task<List<NotificationPreferenceModel>> GetMyPreferencesAsync(CancellationToken ct = default)
    {
        AttachAuth();
        var response = await http.GetAsync("api/account/preferences", ct);
        if (!response.IsSuccessStatusCode) return [];

        var envelope = await response.Content
            .ReadFromJsonAsync<ApiEnvelope<List<NotificationPreferenceModel>>>(Json, ct);
        return envelope?.Data ?? [];
    }

    public async Task<bool> SaveMyPreferencesAsync(List<NotificationPreferenceModel> prefs, CancellationToken ct = default)
    {
        AttachAuth();
        var body = prefs.Select(x => new { categoryId = x.CategoryId, wantsEmail = x.WantsEmail, wantsSms = x.WantsSms }).ToList();
        var response = await http.PutAsJsonAsync("api/account/preferences", body, Json, ct);
        return response.IsSuccessStatusCode;
    }

    // ── ORGANIZER ─────────────────────────────────────────────────────────

    public async Task<List<OrganizerEventModel>> GetMyOrganizerEventsAsync(CancellationToken ct = default)
    {
        AttachAuth();
        var response = await http.GetAsync("api/organizer/events", ct);
        if (!response.IsSuccessStatusCode) return [];

        var envelope = await response.Content
            .ReadFromJsonAsync<ApiEnvelope<List<OrganizerEventModel>>>(Json, ct);
        return envelope?.Data ?? [];
    }

    public async Task<bool> UpdateMyOrganizerEventAsync(
        Guid id,
        string title,
        string description,
        decimal price,
        DateTime startDate,
        DateTime endDate,
        string city,
        Guid categoryId,
        bool isFeatured,
        CancellationToken ct = default)
    {
        AttachAuth();
        var body = new { title, description, price, startDate, endDate, city, categoryId, isFeatured };
        var response = await http.PutAsJsonAsync($"api/organizer/events/{id}", body, Json, ct);
        return response.IsSuccessStatusCode;
    }

    public async Task<List<AdvertisementSlotModel>> GetMyOrganizerAdSlotsAsync(CancellationToken ct = default)
    {
        AttachAuth();
        var response = await http.GetAsync("api/organizer/ad-slots", ct);
        if (!response.IsSuccessStatusCode) return [];

        var envelope = await response.Content
            .ReadFromJsonAsync<ApiEnvelope<List<AdvertisementSlotModel>>>(Json, ct);
        return envelope?.Data ?? [];
    }

    public async Task<bool> CreateMyOrganizerAdSlotAsync(
        string placement,
        string title,
        string subtitle,
        string? linkUrl,
        string? imageUrl,
        bool isActive,
        int priority,
        DateTime? startDateUtc,
        DateTime? endDateUtc,
        CancellationToken ct = default)
    {
        AttachAuth();
        var body = new { placement, title, subtitle, linkUrl, imageUrl, isActive, priority, startDateUtc, endDateUtc };
        var response = await http.PostAsJsonAsync("api/organizer/ad-slots", body, Json, ct);
        return response.IsSuccessStatusCode;
    }

    public async Task<bool> UpdateMyOrganizerAdSlotAsync(
        Guid id,
        string placement,
        string title,
        string subtitle,
        string? linkUrl,
        string? imageUrl,
        bool isActive,
        int priority,
        DateTime? startDateUtc,
        DateTime? endDateUtc,
        CancellationToken ct = default)
    {
        AttachAuth();
        var body = new { placement, title, subtitle, linkUrl, imageUrl, isActive, priority, startDateUtc, endDateUtc };
        var response = await http.PutAsJsonAsync($"api/organizer/ad-slots/{id}", body, Json, ct);
        return response.IsSuccessStatusCode;
    }

    public async Task<bool> DeleteMyOrganizerAdSlotAsync(Guid id, CancellationToken ct = default)
    {
        AttachAuth();
        var response = await http.DeleteAsync($"api/organizer/ad-slots/{id}", ct);
        return response.IsSuccessStatusCode;
    }

}

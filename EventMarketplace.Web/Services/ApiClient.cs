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
        string? city = null, bool? isFeatured = null, int page = 1, int pageSize = 9)
    {
        var url = $"api/events?page={page}&pageSize={pageSize}";
        if (!string.IsNullOrWhiteSpace(city)) url += $"&city={Uri.EscapeDataString(city)}";
        if (isFeatured.HasValue) url += $"&isFeatured={isFeatured.Value}";

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
}

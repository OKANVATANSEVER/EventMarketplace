using FluentAssertions;
using EventMarketplace.API.IntegrationTests.Infrastructure;
using Microsoft.AspNetCore.Mvc.Testing;
using System.Net;
using System.Net.Http.Json;
using Xunit;

namespace EventMarketplace.API.IntegrationTests.Authorization;

public class EventsAuthorizationTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly CustomWebApplicationFactory _factory;

    public EventsAuthorizationTests(CustomWebApplicationFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task CreateEvent_Should_Return401_WhenNoToken()
    {
        var client = _factory.CreateClient();

        var response = await client.PostAsJsonAsync("/api/events", new
        {
            title = "Unauthorized",
            description = "test",
            price = 10,
            startDate = DateTime.UtcNow.AddDays(1),
            endDate = DateTime.UtcNow.AddDays(1).AddHours(2),
            city = "Istanbul",
            categoryId = Guid.NewGuid(),
            isFeatured = false
        });

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task CreateEvent_Should_Return403_WhenRoleIsNotOrganizer()
    {
        var client = _factory.CreateClient();
        client.DefaultRequestHeaders.Add("X-Test-User", "integration-user");
        client.DefaultRequestHeaders.Add("X-Test-Role", "Customer");

        var response = await client.PostAsJsonAsync("/api/events", new
        {
            title = "Forbidden",
            description = "test",
            price = 10,
            startDate = DateTime.UtcNow.AddDays(1),
            endDate = DateTime.UtcNow.AddDays(1).AddHours(2),
            city = "Istanbul",
            categoryId = Guid.NewGuid(),
            isFeatured = false
        });

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }
}

using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using HotelBooking.Application.Features.Auth.DTOs;
using HotelBooking.Application.Features.Bookings.DTOs;
using HotelBooking.Application.Features.Rooms.DTOs;
using HotelBooking.Domain.Enums;
using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;

namespace HotelBooking.ApiTests;

public class AdvancedApiBehaviorTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;

    public AdvancedApiBehaviorTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory.WithWebHostBuilder(builder =>
        {
            builder.UseSetting("JwtSettings:Key", "API_TEST_SECRET_KEY_12345678901234567890");
            builder.UseSetting("JwtSettings:Issuer", "HotelBookingSystem");
            builder.UseSetting("JwtSettings:Audience", "HotelBookingSystem");
        });
    }

    [Fact]
    public async Task RoomsPagination_DefaultsAndStableEnvelope_AreReturned()
    {
        var client = _factory.CreateClient();
        await LoginAsAsync(client, "admin@hotel.com", "Admin123!");

        var response = await client.GetAsync("/api/rooms");
        var payload = await response.Content.ReadFromJsonAsync<ApiEnvelope<RoomsPagedResult>>();

        response.EnsureSuccessStatusCode();
        Assert.True(payload!.Success);
        Assert.Equal(1, payload.Data.Page);
        Assert.Equal(20, payload.Data.PageSize);
        Assert.NotNull(payload.Message);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(101)]
    public async Task Rooms_InvalidPageSize_Returns400(int pageSize)
    {
        var client = _factory.CreateClient();
        await LoginAsAsync(client, "admin@hotel.com", "Admin123!");
        var response = await client.GetAsync($"/api/rooms?pageSize={pageSize}");
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task RoomsFilteringByStatus_ReturnsOnlyMatchingRooms()
    {
        var client = _factory.CreateClient();
        await LoginAsAsync(client, "admin@hotel.com", "Admin123!");

        var response = await client.GetAsync("/api/rooms?status=Available&pageSize=100");
        var payload = await response.Content.ReadFromJsonAsync<ApiEnvelope<RoomsPagedResult>>();

        response.EnsureSuccessStatusCode();
        Assert.All(payload!.Data.Items, room => Assert.Equal(RoomStatus.Available.ToString(), room.Status));
    }

    [Fact]
    public async Task RoomsAreSortedByFloorThenRoomNumber()
    {
        var client = _factory.CreateClient();
        await LoginAsAsync(client, "admin@hotel.com", "Admin123!");

        var response = await client.GetAsync("/api/rooms?pageSize=100");
        var payload = await response.Content.ReadFromJsonAsync<ApiEnvelope<RoomsPagedResult>>();
        var sorted = payload!.Data.Items.OrderBy(r => r.FloorNumber).ThenBy(r => r.RoomNumber).Select(r => r.Id);

        Assert.Equal(sorted, payload.Data.Items.Select(r => r.Id));
    }

    [Fact]
    public async Task RoomMissingId_Returns404()
    {
        var client = _factory.CreateClient();
        await LoginAsAsync(client, "admin@hotel.com", "Admin123!");
        var response = await client.GetAsync($"/api/rooms/{Guid.NewGuid()}");
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task CreateRoom_ReturnsCreatedAndDeleteReturnsNoContent()
    {
        var client = _factory.CreateClient();
        await LoginAsAsync(client, "admin@hotel.com", "Admin123!");
        var roomId = await CreateRoomAsync(client);
        var delete = await client.DeleteAsync($"/api/rooms/{roomId}");
        Assert.Equal(HttpStatusCode.NoContent, delete.StatusCode);
    }

    [Fact]
    public async Task DeleteRoom_IsNotIdempotentForMissingRoom()
    {
        var client = _factory.CreateClient();
        await LoginAsAsync(client, "admin@hotel.com", "Admin123!");
        var roomId = await CreateRoomAsync(client);
        (await client.DeleteAsync($"/api/rooms/{roomId}")).EnsureSuccessStatusCode();
        var secondDelete = await client.DeleteAsync($"/api/rooms/{roomId}");
        Assert.Equal(HttpStatusCode.NotFound, secondDelete.StatusCode);
    }

    [Fact]
    public async Task BookingFilterInvalidPageSize_Returns400()
    {
        var client = _factory.CreateClient();
        await LoginAsAsync(client, "admin@hotel.com", "Admin123!");
        var response = await client.GetAsync("/api/bookings?pageSize=1000");
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task BookingConflictResponse_UsesConflictStatus()
    {
        var client = _factory.CreateClient();
        await LoginAsAsync(client, "admin@hotel.com", "Admin123!");
        var roomId = await CreateRoomAsync(client);
        await LoginAsAsync(client, "james.carter@example.com", "Guest123!");
        var request = new CreateBookingRequest(null, roomId, TodayPlus(50), TodayPlus(52), PaymentMethod.Card, 1);

        (await client.PostAsJsonAsync("/api/bookings", request)).EnsureSuccessStatusCode();
        var duplicate = await client.PostAsJsonAsync("/api/bookings", request);

        Assert.Equal(HttpStatusCode.Conflict, duplicate.StatusCode);
    }

    [Fact]
    public async Task ValidationErrorResponse_ContainsErrorsShape()
    {
        var client = _factory.CreateClient();
        await LoginAsAsync(client, "admin@hotel.com", "Admin123!");
        var response = await client.PostAsJsonAsync("/api/rooms", new CreateRoomRequest("", 1, RoomType.Standard, -1, 0, []));
        var body = await response.Content.ReadAsStringAsync();

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Contains("errors", body, StringComparison.OrdinalIgnoreCase);
    }

    private static async Task LoginAsAsync(HttpClient client, string email, string password)
    {
        var response = await client.PostAsJsonAsync("/api/auth/login", new LoginRequest(email, password));
        response.EnsureSuccessStatusCode();
        var payload = await response.Content.ReadFromJsonAsync<ApiEnvelope<AuthResponse>>();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", payload!.Data.Token);
    }

    private static async Task<Guid> CreateRoomAsync(HttpClient client)
    {
        var roomNumber = $"A{Guid.NewGuid():N}"[..8].ToUpperInvariant();
        var create = await client.PostAsJsonAsync("/api/rooms", new CreateRoomRequest(roomNumber, 9, RoomType.Standard, 100m, 2, ["WiFi"]));
        Assert.Equal(HttpStatusCode.Created, create.StatusCode);
        var payload = await create.Content.ReadFromJsonAsync<ApiEnvelope<RoomResponse>>();
        return payload!.Data.Id;
    }

    private static DateOnly TodayPlus(int days) => DateOnly.FromDateTime(DateTime.UtcNow).AddDays(days);

    private sealed record ApiEnvelope<T>(bool Success, string Message, T Data);
}

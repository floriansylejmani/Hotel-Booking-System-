using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Security.Claims;
using System.Text;
using HotelBooking.Application.Features.Auth.DTOs;
using HotelBooking.Application.Features.Bookings.DTOs;
using HotelBooking.Application.Features.Rooms.DTOs;
using HotelBooking.Domain.Constants;
using HotelBooking.Domain.Enums;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.IdentityModel.Tokens;
using Xunit;

namespace HotelBooking.ApiTests;

public class SecurityTests : IClassFixture<WebApplicationFactory<Program>>
{
    private const string JwtKey = "API_TEST_SECRET_KEY_12345678901234567890";
    private readonly WebApplicationFactory<Program> _factory;

    public SecurityTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory.WithWebHostBuilder(builder =>
        {
            builder.UseSetting("JwtSettings:Key", JwtKey);
            builder.UseSetting("JwtSettings:Issuer", "HotelBookingSystem");
            builder.UseSetting("JwtSettings:Audience", "HotelBookingSystem");
        });
    }

    [Fact]
    public async Task MissingToken_Returns401()
    {
        var response = await _factory.CreateClient().GetAsync("/api/dashboard/summary");
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task MalformedToken_Returns401()
    {
        var client = _factory.CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", "malformed.token.value");
        var response = await client.GetAsync("/api/dashboard/summary");
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task ExpiredToken_Returns401()
    {
        var client = _factory.CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", CreateToken(RoleNames.Admin, DateTime.UtcNow.AddMinutes(-5)));
        var response = await client.GetAsync("/api/dashboard/summary");
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task GuestCannotAccessAdminDashboard()
    {
        var client = _factory.CreateClient();
        await LoginAsAsync(client, "james.carter@example.com", "Guest123!");
        var response = await client.GetAsync("/api/dashboard/summary");
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task HousekeeperCannotCreateRooms()
    {
        var client = _factory.CreateClient();
        await LoginAsAsync(client, "maria.santos@hotel.com", "Staff123!");
        var response = await client.PostAsJsonAsync("/api/rooms", new CreateRoomRequest("SEC1", 1, RoomType.Standard, 100m, 1, []));
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task GuestCannotAccessStaffBookingList()
    {
        var client = _factory.CreateClient();
        await LoginAsAsync(client, "james.carter@example.com", "Guest123!");
        var response = await client.GetAsync("/api/bookings");
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task GuestCannotAccessInvoiceEndpoint()
    {
        var client = _factory.CreateClient();
        await LoginAsAsync(client, "james.carter@example.com", "Guest123!");
        var response = await client.GetAsync($"/api/invoices/{Guid.NewGuid()}");
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task GuestCannotProcessPayments()
    {
        var client = _factory.CreateClient();
        await LoginAsAsync(client, "james.carter@example.com", "Guest123!");
        var response = await client.PostAsJsonAsync("/api/payments", new { bookingId = Guid.NewGuid(), paymentMethod = "Card", amount = 10m });
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task UserCannotCancelAnotherUsersBooking()
    {
        var client = _factory.CreateClient();
        await LoginAsAsync(client, "admin@hotel.com", "Admin123!");
        var roomId = await CreateRoomAsync(client);
        await LoginAsAsync(client, "james.carter@example.com", "Guest123!");
        var booking = await CreateBookingAsync(client, roomId);

        await LoginAsAsync(client, "the.williams@example.com", "Guest123!");
        var response = await client.PutAsync($"/api/bookings/{booking.Id}/cancel", null);

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task SqlInjectionLikeSearch_DoesNotBreakRoomsEndpoint()
    {
        var client = _factory.CreateClient();
        await LoginAsAsync(client, "admin@hotel.com", "Admin123!");
        var response = await client.GetAsync("/api/rooms?search=%27%3Bdrop%20table%20Rooms%3B--");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task XssLikeSpecialRequest_IsRejected()
    {
        var client = _factory.CreateClient();
        await LoginAsAsync(client, "admin@hotel.com", "Admin123!");
        var roomId = await CreateRoomAsync(client);
        await LoginAsAsync(client, "james.carter@example.com", "Guest123!");

        var response = await client.PostAsJsonAsync("/api/bookings", new CreateBookingRequest(null, roomId, TodayPlus(30), TodayPlus(31), PaymentMethod.Card, 1, "<script>alert(1)</script>"));

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task OverPostingRoleDuringRegistration_IsRejected()
    {
        var client = _factory.CreateClient();
        var response = await client.PostAsJsonAsync("/api/auth/register", new RegisterRequest("Mallory", $"mallory-{Guid.NewGuid():N}@test.local", "Password123!", "Admin"));
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task PasswordHashIsNeverReturnedFromAuthResponse()
    {
        var client = _factory.CreateClient();
        var response = await client.PostAsJsonAsync("/api/auth/login", new LoginRequest("admin@hotel.com", "Admin123!"));
        var body = await response.Content.ReadAsStringAsync();
        Assert.DoesNotContain("PasswordHash", body, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("Admin123!", body, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task InvalidGuidFormat_Returns404WithoutServerError()
    {
        var client = _factory.CreateClient();
        await LoginAsAsync(client, "admin@hotel.com", "Admin123!");
        var response = await client.GetAsync("/api/rooms/not-a-guid");
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public void JwtSecret_IsConfiguredForTestHost()
    {
        Assert.True(JwtKey.Length >= 32);
    }

    private static string CreateToken(string role, DateTime expires)
    {
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(JwtKey));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
        var token = new JwtSecurityToken(
            issuer: "HotelBookingSystem",
            audience: "HotelBookingSystem",
            claims:
            [
                new Claim(ClaimTypes.NameIdentifier, Guid.NewGuid().ToString()),
                new Claim(ClaimTypes.Email, "expired@test.local"),
                new Claim(ClaimTypes.Name, "Expired User"),
                new Claim(ClaimTypes.Role, role)
            ],
            expires: expires,
            signingCredentials: credentials);
        return new JwtSecurityTokenHandler().WriteToken(token);
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
        var roomNumber = $"S{Guid.NewGuid():N}"[..8].ToUpperInvariant();
        var create = await client.PostAsJsonAsync("/api/rooms", new CreateRoomRequest(roomNumber, 9, RoomType.Standard, 100m, 2, ["WiFi"]));
        create.EnsureSuccessStatusCode();
        var payload = await create.Content.ReadFromJsonAsync<ApiEnvelope<RoomResponse>>();
        return payload!.Data.Id;
    }

    private static async Task<BookingResponse> CreateBookingAsync(HttpClient client, Guid roomId)
    {
        var create = await client.PostAsJsonAsync("/api/bookings", new CreateBookingRequest(null, roomId, TodayPlus(40), TodayPlus(41), PaymentMethod.Card, 1));
        create.EnsureSuccessStatusCode();
        var payload = await create.Content.ReadFromJsonAsync<ApiEnvelope<BookingResponse>>();
        return payload!.Data;
    }

    private static DateOnly TodayPlus(int days) => DateOnly.FromDateTime(DateTime.UtcNow).AddDays(days);

    private sealed record ApiEnvelope<T>(bool Success, string Message, T Data);
}

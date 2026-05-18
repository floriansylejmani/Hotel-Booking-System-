using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using HotelBooking.Application.Features.Auth.DTOs;
using HotelBooking.Application.Features.Bookings.DTOs;
using HotelBooking.Application.Features.Dashboard.DTOs;
using HotelBooking.Application.Features.Notifications.DTOs;
using HotelBooking.Application.Features.Payments.DTOs;
using HotelBooking.Application.Features.Rooms.DTOs;
using HotelBooking.Domain.Enums;
using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;

namespace HotelBooking.ApiTests;

public class ApiIntegrationTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;

    public ApiIntegrationTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory.WithWebHostBuilder(builder =>
        {
            builder.UseSetting("JwtSettings:Key", "API_TEST_SECRET_KEY_12345678901234567890");
            builder.UseSetting("JwtSettings:Issuer", "HotelBookingSystem");
            builder.UseSetting("JwtSettings:Audience", "HotelBookingSystem");
        });
    }

    [Fact]
    public async Task RegisterAndLogin_ReturnToken()
    {
        var client = _factory.CreateClient();
        var email = $"guest-{Guid.NewGuid():N}@example.com";

        var register = await client.PostAsJsonAsync("/api/auth/register", new RegisterRequest("Test Guest", email, "Password123!", "Guest"));
        register.EnsureSuccessStatusCode();

        var login = await client.PostAsJsonAsync("/api/auth/login", new LoginRequest(email, "Password123!"));
        login.EnsureSuccessStatusCode();
        var payload = await login.Content.ReadFromJsonAsync<ApiEnvelope<AuthResponse>>();

        Assert.False(string.IsNullOrWhiteSpace(payload?.Data.Token));
    }

    [Fact]
    public async Task RegisterRejectsDuplicateEmailAndLoginRejectsWrongPassword()
    {
        var client = _factory.CreateClient();
        var email = $"duplicate-{Guid.NewGuid():N}@example.com";
        var request = new RegisterRequest("Duplicate Guest", email, "Password123!", "Guest");

        var first = await client.PostAsJsonAsync("/api/auth/register", request);
        var duplicate = await client.PostAsJsonAsync("/api/auth/register", request);
        var wrongPassword = await client.PostAsJsonAsync("/api/auth/login", new LoginRequest(email, "WrongPassword123!"));

        first.EnsureSuccessStatusCode();
        Assert.Equal(HttpStatusCode.Conflict, duplicate.StatusCode);
        Assert.Equal(HttpStatusCode.Unauthorized, wrongPassword.StatusCode);
    }

    [Fact]
    public async Task ProtectedEndpoint_WithoutToken_Returns401()
    {
        var response = await _factory.CreateClient().GetAsync("/api/bookings");
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task ProtectedEndpoint_WithInvalidToken_Returns401()
    {
        var client = _factory.CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", "not-a-valid-jwt");

        var response = await client.GetAsync("/api/rooms");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task GuestAccessingAdminRoomCreate_Returns403()
    {
        var client = _factory.CreateClient();
        await LoginAsAsync(client, "james.carter@example.com", "Guest123!");

        var response = await client.PostAsJsonAsync("/api/rooms", new CreateRoomRequest($"T{Random.Shared.Next(1000, 9999)}", 8, RoomType.Standard, 100m, 1, ["WiFi"]));

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task AdminCanCreateUpdateDeleteRoom()
    {
        var client = _factory.CreateClient();
        await LoginAsAsync(client, "admin@hotel.com", "Admin123!");
        var roomNumber = $"T{Random.Shared.Next(1000, 9999)}";

        var create = await client.PostAsJsonAsync("/api/rooms", new CreateRoomRequest(roomNumber, 9, RoomType.Standard, 100m, 1, ["WiFi"]));
        create.EnsureSuccessStatusCode();
        var created = await create.Content.ReadFromJsonAsync<ApiEnvelope<RoomResponse>>();

        var update = await client.PutAsJsonAsync($"/api/rooms/{created!.Data.Id}", new UpdateRoomRequest(null, null, null, 125m, null, null, RoomStatus.Available));
        update.EnsureSuccessStatusCode();

        var delete = await client.DeleteAsync($"/api/rooms/{created.Data.Id}");
        Assert.Equal(HttpStatusCode.NoContent, delete.StatusCode);
    }

    [Fact]
    public async Task RoomReadAndValidationEndpoints_Work()
    {
        var client = _factory.CreateClient();
        await LoginAsAsync(client, "admin@hotel.com", "Admin123!");

        var list = await client.GetAsync("/api/rooms?pageSize=5");
        list.EnsureSuccessStatusCode();
        var listPayload = await list.Content.ReadFromJsonAsync<ApiEnvelope<RoomsPagedResult>>();

        var getById = await client.GetAsync($"/api/rooms/{listPayload!.Data.Items[0].Id}");
        var invalidPayload = await client.PostAsJsonAsync("/api/rooms", new CreateRoomRequest("", 1, RoomType.Standard, -1m, 0, []));

        getById.EnsureSuccessStatusCode();
        Assert.NotEmpty(listPayload.Data.Items);
        Assert.Equal(HttpStatusCode.BadRequest, invalidPayload.StatusCode);
    }

    [Fact]
    public async Task BookingValidationAndDoubleBookingRules_Work()
    {
        var client = _factory.CreateClient();
        await LoginAsAsync(client, "admin@hotel.com", "Admin123!");
        var roomId = await CreateRoomAsync(client);

        await LoginAsAsync(client, "james.carter@example.com", "Guest123!");
        var invalid = await client.PostAsJsonAsync("/api/bookings", new CreateBookingRequest(null, roomId, new DateOnly(2026, 9, 3), new DateOnly(2026, 9, 1), PaymentMethod.Card, 1));
        Assert.Equal(HttpStatusCode.BadRequest, invalid.StatusCode);

        var validRequest = new CreateBookingRequest(null, roomId, new DateOnly(2026, 9, 1), new DateOnly(2026, 9, 3), PaymentMethod.Card, 1);
        var first = await client.PostAsJsonAsync("/api/bookings", validRequest);
        first.EnsureSuccessStatusCode();
        var second = await client.PostAsJsonAsync("/api/bookings", validRequest);
        Assert.Equal(HttpStatusCode.Conflict, second.StatusCode);
    }

    [Fact]
    public async Task AnonymousUserCannotCreateBookingAndGuestCanCancelOwnBooking()
    {
        var client = _factory.CreateClient();
        await LoginAsAsync(client, "admin@hotel.com", "Admin123!");
        var roomId = await CreateRoomAsync(client);
        client.DefaultRequestHeaders.Authorization = null;

        var anonymous = await client.PostAsJsonAsync("/api/bookings", new CreateBookingRequest(null, roomId, new DateOnly(2026, 11, 1), new DateOnly(2026, 11, 3), PaymentMethod.Card, 1));
        Assert.Equal(HttpStatusCode.Unauthorized, anonymous.StatusCode);

        await LoginAsAsync(client, "james.carter@example.com", "Guest123!");
        var booking = await CreateBookingAsync(client, roomId, new DateOnly(2026, 11, 1), new DateOnly(2026, 11, 3));
        var cancel = await client.PutAsync($"/api/bookings/{booking.Id}/cancel", null);

        cancel.EnsureSuccessStatusCode();
    }

    [Fact]
    public async Task UserCannotCancelAnotherUsersBooking()
    {
        var client = _factory.CreateClient();
        await LoginAsAsync(client, "admin@hotel.com", "Admin123!");
        var roomId = await CreateRoomAsync(client);

        await LoginAsAsync(client, "james.carter@example.com", "Guest123!");
        var booking = await CreateBookingAsync(client, roomId, new DateOnly(2026, 10, 1), new DateOnly(2026, 10, 3));

        await LoginAsAsync(client, "the.williams@example.com", "Guest123!");
        var cancel = await client.PutAsync($"/api/bookings/{booking.Id}/cancel", null);

        Assert.Equal(HttpStatusCode.Forbidden, cancel.StatusCode);
    }

    [Fact]
    public async Task CheckInCheckOutFlow_Works()
    {
        var client = _factory.CreateClient();
        await LoginAsAsync(client, "admin@hotel.com", "Admin123!");
        var roomId = await CreateRoomAsync(client);

        await LoginAsAsync(client, "james.carter@example.com", "Guest123!");
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var booking = await CreateBookingAsync(client, roomId, today, today.AddDays(1));

        await LoginAsAsync(client, "admin@hotel.com", "Admin123!");
        var checkIn = await client.PutAsync($"/api/checkin/{booking.Id}", null);
        checkIn.EnsureSuccessStatusCode();

        var checkOut = await client.PutAsync($"/api/checkout/{booking.Id}", null);
        checkOut.EnsureSuccessStatusCode();
    }

    [Fact]
    public async Task CheckInOutAuthorizationAndValidation_Work()
    {
        var client = _factory.CreateClient();
        await LoginAsAsync(client, "admin@hotel.com", "Admin123!");
        var roomId = await CreateRoomAsync(client);

        await LoginAsAsync(client, "james.carter@example.com", "Guest123!");
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var booking = await CreateBookingAsync(client, roomId, today, today.AddDays(1));
        var guestCheckIn = await client.PutAsync($"/api/checkin/{booking.Id}", null);
        Assert.Equal(HttpStatusCode.Forbidden, guestCheckIn.StatusCode);

        await LoginAsAsync(client, "admin@hotel.com", "Admin123!");
        var checkoutBeforeCheckin = await client.PutAsync($"/api/checkout/{booking.Id}", null);
        var missing = await client.PutAsync($"/api/checkin/{Guid.NewGuid()}", null);

        Assert.Equal(HttpStatusCode.BadRequest, checkoutBeforeCheckin.StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, missing.StatusCode);
    }

    [Fact]
    public async Task PaymentAndInvoiceFlow_WorkForNewBooking()
    {
        var client = _factory.CreateClient();
        await LoginAsAsync(client, "admin@hotel.com", "Admin123!");
        var roomId = await CreateRoomAsync(client);

        await LoginAsAsync(client, "james.carter@example.com", "Guest123!");
        var booking = await CreateBookingAsync(client, roomId, new DateOnly(2026, 12, 15), new DateOnly(2026, 12, 17));

        await LoginAsAsync(client, "admin@hotel.com", "Admin123!");
        var invoice = await client.GetAsync($"/api/invoices/{booking.Id}");
        invoice.EnsureSuccessStatusCode();
        var invoicePayload = await invoice.Content.ReadFromJsonAsync<ApiEnvelope<InvoiceResponse>>();

        var payment = await client.PostAsJsonAsync("/api/payments", new ProcessPaymentRequest(booking.Id, PaymentMethod.Card.ToString(), invoicePayload!.Data.TotalAmount));
        payment.EnsureSuccessStatusCode();

        Assert.True(invoicePayload.Data.TotalAmount > 0);
    }

    [Fact]
    public async Task PaymentInvoiceDashboardHousekeepingAndNotificationEndpoints_EnforceBehavior()
    {
        var client = _factory.CreateClient();
        await LoginAsAsync(client, "james.carter@example.com", "Guest123!");

        var guestDashboard = await client.GetAsync("/api/dashboard/summary");
        var guestHousekeepingUpdate = await client.PutAsJsonAsync($"/api/housekeeping/{Guid.NewGuid()}", new { status = "Clean", notes = "done" });
        Assert.Equal(HttpStatusCode.Forbidden, guestDashboard.StatusCode);
        Assert.Equal(HttpStatusCode.Forbidden, guestHousekeepingUpdate.StatusCode);

        var notifications = await client.GetAsync("/api/notifications");
        notifications.EnsureSuccessStatusCode();
        var notificationPayload = await notifications.Content.ReadFromJsonAsync<ApiEnvelope<NotificationsPagedResult>>();
        if (notificationPayload!.Data.Items.Count > 0)
        {
            var markRead = await client.PutAsync($"/api/notifications/{notificationPayload.Data.Items[0].Id}/read", null);
            Assert.Equal(HttpStatusCode.NoContent, markRead.StatusCode);
        }

        await LoginAsAsync(client, "admin@hotel.com", "Admin123!");
        var summary = await client.GetAsync("/api/dashboard/summary");
        summary.EnsureSuccessStatusCode();
        var summaryPayload = await summary.Content.ReadFromJsonAsync<ApiEnvelope<DashboardSummaryResponse>>();
        Assert.True(summaryPayload!.Data.TotalRooms >= 0);
        Assert.True(summaryPayload.Data.MonthlyRevenue >= 0);

        var housekeepingList = await client.GetAsync("/api/housekeeping");
        housekeepingList.EnsureSuccessStatusCode();

        var invalidPayment = await client.PostAsJsonAsync("/api/payments", new ProcessPaymentRequest(Guid.NewGuid(), PaymentMethod.Card.ToString(), -1m));
        Assert.Equal(HttpStatusCode.BadRequest, invalidPayment.StatusCode);
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
        var roomNumber = $"T{Guid.NewGuid():N}"[..8].ToUpperInvariant();
        var create = await client.PostAsJsonAsync("/api/rooms", new CreateRoomRequest(roomNumber, 9, RoomType.Standard, 100m, 1, ["WiFi"]));
        create.EnsureSuccessStatusCode();
        var payload = await create.Content.ReadFromJsonAsync<ApiEnvelope<RoomResponse>>();
        return payload!.Data.Id;
    }

    private static async Task<BookingResponse> CreateBookingAsync(HttpClient client, Guid roomId, DateOnly checkIn, DateOnly checkOut)
    {
        var create = await client.PostAsJsonAsync("/api/bookings", new CreateBookingRequest(null, roomId, checkIn, checkOut, PaymentMethod.Card, 1));
        create.EnsureSuccessStatusCode();
        var payload = await create.Content.ReadFromJsonAsync<ApiEnvelope<BookingResponse>>();
        return payload!.Data;
    }

    private sealed record ApiEnvelope<T>(bool Success, string Message, T Data);
}

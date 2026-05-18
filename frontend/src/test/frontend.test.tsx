import React from "react";
import { QueryClient, QueryClientProvider } from "@tanstack/react-query";
import { render, screen, waitFor } from "@testing-library/react";
import userEvent from "@testing-library/user-event";
import { describe, expect, it, vi } from "vitest";
import { AuthGuard } from "@/components/auth/auth-guard";
import { AppSidebar } from "@/components/layout/app-sidebar";
import { LoginWorkspace } from "@/features/auth/components/login-workspace";
import { RegisterWorkspace } from "@/features/auth/components/register-workspace";
import { RoomsWorkspace } from "@/features/rooms/components/rooms-workspace";
import { DashboardWorkspace } from "@/features/dashboard/components/dashboard-workspace";
import { BookingsWorkspace } from "@/features/bookings/components/bookings-workspace";
import { PaymentsWorkspace } from "@/features/payments/components/payments-workspace";
import { HousekeepingWorkspace } from "@/features/housekeeping/components/housekeeping-workspace";
import { NotificationPanel } from "@/components/shared/notification-panel";
import { login } from "@/features/auth/api";
import { createBooking, getBookings } from "@/features/bookings/api";
import { getRooms } from "@/features/rooms/api";
import { getDefaultInvoice, processPayment } from "@/features/payments/api";
import { getHousekeepingTasks, updateHousekeepingTask } from "@/features/housekeeping/api";
import { getNotifications, markNotificationRead } from "@/features/notifications/api";
import { ApiError, getApiErrorMessage } from "@/services/api-client";
import { apiClient, apiConfig } from "@/services/api-client";
import { useAuthStore } from "@/store/auth-store";

vi.mock("@/features/auth/api", () => ({
  login: vi.fn(async () => ({ token: "login-token", expiresAt: "2099-01-01T00:00:00Z", user: { id: "1", name: "Admin", email: "admin@hotel.com", role: "admin", avatar: "AD" } })),
  registerAccount: vi.fn(async () => ({ token: "register-token", expiresAt: "2099-01-01T00:00:00Z", user: { id: "2", name: "Guest", email: "guest@example.com", role: "guest", avatar: "GU" } })),
}));

vi.mock("@/hooks/use-toast", () => ({ useToast: () => ({ addToast: vi.fn() }) }));
vi.mock("@/hooks/use-app-shell", () => ({ useAppShell: () => ({ closeSidebar: vi.fn(), isSidebarOpen: true }) }));
vi.mock("@/features/rooms/api", () => ({
  roomsQueryKey: ["rooms"],
  supportedRoomTypes: ["Standard", "Deluxe", "Family", "Business Suite"],
  supportedRoomStatuses: ["available", "occupied", "maintenance"],
  getRooms: vi.fn(async () => ({ items: [{ id: "r1", roomNumber: "101", type: "Standard", status: "available", pricePerNight: 120, floor: 1, beds: 2, amenities: ["WiFi"], createdAt: "2026-01-01" }], totalCount: 1, page: 1, pageSize: 100 })),
  getAllAmenities: vi.fn(() => ["WiFi"]),
  createRoom: vi.fn(), updateRoom: vi.fn(), deleteRoom: vi.fn(),
}));
vi.mock("@/features/dashboard/api", () => ({
  getDashboardData: vi.fn(async () => ({
    kpis: [{ label: "Rooms", value: "10", sublabel: "5 occupied", delta: "50% occupied", tone: "blue" }],
    recentBookings: [],
    activities: [],
    quickActions: [],
    occupancy: [],
  })),
  dashboardSummaryQueryKey: ["summary"], dashboardOccupancyQueryKey: ["occupancy"], dashboardRecentBookingsQueryKey: ["recent"], dashboardActivityQueryKey: ["activity"],
  getDashboardSummary: vi.fn(async () => ({ occupancyRate: 50, availableRooms: 5, activeBookings: 3, revenueToday: 1200 })),
  getDashboardOccupancy: vi.fn(async () => []), getRecentBookings: vi.fn(async () => []), getDashboardActivity: vi.fn(async () => []),
}));
vi.mock("@/features/bookings/api", () => ({
  bookingsQueryKey: ["bookings"],
  getBookings: vi.fn(async () => ({ items: [], totalCount: 0, page: 1, pageSize: 20 })),
  getBooking: vi.fn(async () => ({ id: "BK-1", recordId: "b1", userId: "u1", roomId: "r1", guestName: "Guest", guestEmail: "g@test.com", roomNumber: "101", roomType: "Standard", checkIn: "2026-07-01", checkOut: "2026-07-02", amount: 120, paymentMethod: "credit-card", status: "confirmed", nights: 1, createdAt: "2026-01-01" })),
  searchBookingsByTerm: vi.fn(async () => ({ items: [], totalCount: 0, page: 1, pageSize: 20 })),
  createBooking: vi.fn(async () => ({ id: "BK-2", recordId: "b2", userId: "u1", roomId: "r1", guestName: "Guest", guestEmail: "g@test.com", roomNumber: "101", roomType: "Standard", checkIn: "2026-07-01", checkOut: "2026-07-02", amount: 120, paymentMethod: "credit-card", status: "confirmed", nights: 1, createdAt: "2026-01-01" })),
  cancelBooking: vi.fn(),
}));
vi.mock("@/features/payments/api", () => ({
  invoiceQueryKey: ["invoice"],
  getDefaultInvoice: vi.fn(async () => ({ invoiceId: "i1", invoiceNumber: "INV-1", issuedDate: "2026-01-01", guestName: "Guest", bookingId: "b1", bookingRecordId: "b1", roomNumber: "101", roomType: "Standard", checkIn: "2026-07-01", checkOut: "2026-07-02", hotelName: "Hotel", hotelAddress: "Address", items: [{ id: "line1", description: "Room stay", rateLabel: "120.00", total: 120 }], subtotal: 120, tax: 0, total: 120 })),
  processPayment: vi.fn(async () => ({ paymentId: "p1", invoiceId: "i1", status: "Completed" })),
  downloadInvoicePdf: vi.fn(async () => new Blob(["pdf"], { type: "application/pdf" })),
}));
vi.mock("@/features/housekeeping/api", () => ({
  housekeepingQueryKey: ["housekeeping"],
  getHousekeepingTasks: vi.fn(async () => [{ id: "h1", roomNumber: "101", floor: 1, assignedTo: "Cleaner", lastCleaned: "2026-01-01", status: "dirty", notes: "Turnover", priorityNote: "Turnover" }]),
  updateHousekeepingTask: vi.fn(async () => ({ id: "h1", roomNumber: "101", status: "Clean" })),
  assignHousekeepingTask: vi.fn(async () => ({ id: "h1", roomNumber: "101", assignedTo: "Cleaner" })),
  getAssignableHousekeepers: vi.fn(async () => [{ id: "s1", fullName: "Cleaner" }]),
}));
vi.mock("@/features/notifications/api", () => ({
  notificationsQueryKey: ["notifications"],
  getNotifications: vi.fn(async () => ({ items: [{ id: "n1", title: "Booking Confirmed", description: "Ready", type: "BookingCreated", createdAt: "2026-01-01", createdAtLabel: "now", tone: "info", read: false }], unreadCount: 1, totalCount: 1, pageNumber: 1, pageSize: 8 })),
  markNotificationRead: vi.fn(async () => undefined),
  markAllNotificationsRead: vi.fn(async () => undefined),
}));

function setSession(role: "admin" | "guest" = "admin") {
  useAuthStore.setState({
    user: { id: "u1", name: "Test User", email: "test@example.com", role, avatar: "TU" },
    token: "token-123",
    expiresAt: "2099-01-01T00:00:00Z",
    hasHydrated: true,
  });
}

function renderWithQuery(ui: React.ReactElement) {
  return render(<QueryClientProvider client={new QueryClient({ defaultOptions: { queries: { retry: false } } })}>{ui}</QueryClientProvider>);
}

describe("frontend auth and API behavior", () => {
  it("auth store saves and clears token", () => {
    useAuthStore.getState().login({ token: "abc", expiresAt: "2099-01-01", user: { id: "1", name: "A", email: "a@test.com", role: "admin", avatar: "A" } });
    expect(useAuthStore.getState().token).toBe("abc");
    useAuthStore.getState().logout();
    expect(useAuthStore.getState().token).toBeNull();
  });

  it("normalizes API error messages", () => {
    expect(getApiErrorMessage(new ApiError("Validation failed", 400))).toBe("Validation failed");
    expect(getApiErrorMessage("unknown", "Fallback message")).toBe("Fallback message");
  });

  it("apiClient sends Authorization header", async () => {
    setSession();
    const fetchMock = vi.spyOn(globalThis, "fetch").mockResolvedValue(new Response(JSON.stringify({ success: true, message: "ok", data: { ok: true } }), { status: 200, headers: { "content-type": "application/json" } }));
    await apiClient<{ ok: boolean }>("/secure");
    const headers = fetchMock.mock.calls[0][1]?.headers as Headers;
    expect(headers.get("Authorization")).toBe("Bearer token-123");
  });

  it("apiClient redirects to login on 401", async () => {
    setSession();
    vi.spyOn(globalThis, "fetch").mockResolvedValue(new Response(JSON.stringify({ message: "Unauthorized" }), { status: 401, headers: { "content-type": "application/json" } }));
    await expect(apiClient("/secure")).rejects.toThrow("Unauthorized");
    expect(window.location.href).toContain("/login?redirect=");
  });

  it("apiClient exposes a configured base URL", () => {
    expect(apiConfig.baseUrl).toContain("/api");
  });

  it("apiClient omits Authorization header when auth is false", async () => {
    setSession();
    const fetchMock = vi.spyOn(globalThis, "fetch").mockResolvedValue(new Response(JSON.stringify({ ok: true }), { status: 200, headers: { "content-type": "application/json" } }));
    await apiClient<{ ok: boolean }>("/public", { auth: false });
    const headers = fetchMock.mock.calls[0][1]?.headers as Headers;
    expect(headers.has("Authorization")).toBe(false);
  });

  it("apiClient normalizes 403 responses", async () => {
    vi.spyOn(globalThis, "fetch").mockResolvedValue(new Response(JSON.stringify({ message: "Forbidden" }), { status: 403, headers: { "content-type": "application/json" } }));
    await expect(apiClient("/admin")).rejects.toMatchObject({ status: 403, message: "Forbidden" });
  });

  it("apiClient returns first validation error message", () => {
    const error = new ApiError("Validation failed", 400, { roomNumber: ["Room number is required."] });
    expect(getApiErrorMessage(error)).toBe("Room number is required.");
  });

  it("apiClient propagates network errors", async () => {
    vi.spyOn(globalThis, "fetch").mockRejectedValue(new TypeError("Network failed"));
    await expect(apiClient("/offline")).rejects.toThrow("Network failed");
  });
});

describe("frontend components", () => {
  it("renders and submits login form", async () => {
    render(<LoginWorkspace />);
    await userEvent.click(screen.getByRole("button", { name: /sign in/i }));
    await waitFor(() => expect(useAuthStore.getState().token).toBe("login-token"));
  });

  it("login form renders server error state", async () => {
    vi.mocked(login).mockRejectedValueOnce(new ApiError("Invalid email or password.", 401));
    render(<LoginWorkspace />);
    await userEvent.click(screen.getByRole("button", { name: /sign in/i }));
    expect(await screen.findByText(/invalid email or password/i)).toBeInTheDocument();
  });

  it("login form validates required fields", async () => {
    render(<LoginWorkspace />);
    await userEvent.clear(screen.getByDisplayValue("admin@hotel.com"));
    await userEvent.clear(screen.getByDisplayValue("Admin123!"));
    await userEvent.click(screen.getByRole("button", { name: /sign in/i }));

    expect(await screen.findByText(/enter a valid email/i)).toBeInTheDocument();
    expect(screen.getByText(/password must contain at least 8 characters/i)).toBeInTheDocument();
  });

  it("renders register form", () => {
    render(<RegisterWorkspace />);
    expect(screen.getByRole("button", { name: /create account/i })).toBeInTheDocument();
    expect(screen.getByRole("combobox")).toBeDisabled();
  });

  it("register form validates password confirmation", async () => {
    render(<RegisterWorkspace />);
    const passwordFields = screen.getAllByDisplayValue("Welcome123!");
    await userEvent.clear(passwordFields[0]);
    await userEvent.type(passwordFields[0], "Different123!");
    await userEvent.click(screen.getByRole("button", { name: /create account/i }));

    expect(await screen.findByText(/passwords do not match/i)).toBeInTheDocument();
  });

  it("protected route redirects anonymous users", () => {
    useAuthStore.setState({ user: null, token: null, expiresAt: null, hasHydrated: true });
    render(<AuthGuard><div>Private</div></AuthGuard>);
    expect(screen.getByText(/redirecting to login/i)).toBeInTheDocument();
  });

  it("protected route renders children for authenticated users", () => {
    setSession("admin");
    render(<AuthGuard><div>Private</div></AuthGuard>);
    expect(screen.getByText("Private")).toBeInTheDocument();
  });

  it("role-based navigation hides admin-only rooms for guests", () => {
    setSession("guest");
    render(<AppSidebar />);
    expect(screen.queryByText("Rooms")).not.toBeInTheDocument();
  });

  it("role-based navigation shows admin areas for admins", () => {
    setSession("admin");
    render(<AppSidebar />);
    expect(screen.getByText("Rooms")).toBeInTheDocument();
    expect(screen.getByText("Dashboard")).toBeInTheDocument();
  });

  it("renders rooms page data", async () => {
    setSession("admin");
    renderWithQuery(<RoomsWorkspace />);
    expect(await screen.findByText("Room 101")).toBeInTheDocument();
  });

  it("rooms page renders loading state", () => {
    vi.mocked(getRooms).mockImplementationOnce(() => new Promise(() => undefined));
    setSession("admin");
    renderWithQuery(<RoomsWorkspace />);
    expect(screen.getByText(/loading rooms/i)).toBeInTheDocument();
  });

  it("rooms page renders API error state", async () => {
    vi.mocked(getRooms).mockRejectedValueOnce(new ApiError("Unable to load rooms.", 500));
    setSession("admin");
    renderWithQuery(<RoomsWorkspace />);
    expect(await screen.findByText(/unable to load rooms/i)).toBeInTheDocument();
  });

  it("renders empty rooms state", async () => {
    vi.mocked(getRooms).mockResolvedValueOnce({ items: [], totalCount: 0, page: 1, pageSize: 100 });
    setSession("admin");
    renderWithQuery(<RoomsWorkspace />);
    expect(await screen.findByText(/no rooms found/i)).toBeInTheDocument();
  });

  it("rooms page filters by room type", async () => {
    setSession("admin");
    renderWithQuery(<RoomsWorkspace />);
    expect(await screen.findByText("Room 101")).toBeInTheDocument();
    await userEvent.click(screen.getByRole("button", { name: "Deluxe" }));
    await waitFor(() => expect(getRooms).toHaveBeenCalledWith(expect.objectContaining({ type: "Deluxe" })));
  });

  it("room card displays price capacity type and status", async () => {
    setSession("admin");
    renderWithQuery(<RoomsWorkspace />);
    expect(await screen.findByText("$120")).toBeInTheDocument();
    expect(screen.getAllByText("Standard").length).toBeGreaterThan(0);
    expect(screen.getAllByText("available").length).toBeGreaterThan(0);
    expect(screen.getByText("beds")).toBeInTheDocument();
  });

  it("renders booking workspace", async () => {
    setSession("admin");
    renderWithQuery(<BookingsWorkspace />);
    expect(await screen.findByText(/bookings/i)).toBeInTheDocument();
  });

  it("booking workspace renders loading state", () => {
    vi.mocked(getBookings).mockImplementationOnce(() => new Promise(() => undefined));
    setSession("admin");
    renderWithQuery(<BookingsWorkspace />);
    expect(screen.getByText(/loading bookings/i)).toBeInTheDocument();
  });

  it("booking workspace renders server error", async () => {
    vi.mocked(getBookings).mockRejectedValueOnce(new ApiError("Unable to load bookings.", 500));
    setSession("admin");
    renderWithQuery(<BookingsWorkspace />);
    expect(await screen.findByText(/unable to load bookings/i)).toBeInTheDocument();
  });

  it("guest cannot open new booking action", async () => {
    setSession("guest");
    renderWithQuery(<BookingsWorkspace />);
    expect(await screen.findByRole("button", { name: /new booking/i })).toBeDisabled();
  });

  it("booking form validates missing room", async () => {
    setSession("admin");
    renderWithQuery(<BookingsWorkspace />);
    await userEvent.click(await screen.findByRole("button", { name: /new booking/i }));
    await userEvent.click(screen.getByRole("button", { name: /^create booking$/i }));
    expect(await screen.findByText(/select a room/i)).toBeInTheDocument();
  });

  it("booking form blocks end date before start date", async () => {
    setSession("admin");
    renderWithQuery(<BookingsWorkspace />);
    await userEvent.click(await screen.findByRole("button", { name: /new booking/i }));
    const dateInputs = screen.getAllByDisplayValue(/\d{4}-\d{2}-\d{2}/);
    await userEvent.clear(dateInputs[1]);
    await userEvent.type(dateInputs[1], "2020-01-01");
    await userEvent.click(screen.getByRole("button", { name: /^create booking$/i }));
    expect(await screen.findByText(/check-out must be after check-in/i)).toBeInTheDocument();
  });

  it("successful booking submit calls API", async () => {
    setSession("admin");
    renderWithQuery(<BookingsWorkspace />);
    await userEvent.click(await screen.findByRole("button", { name: /new booking/i }));
    await userEvent.selectOptions(await screen.findByRole("combobox", { name: "" }).catch(() => screen.getAllByRole("combobox")[0]), "r1");
    await userEvent.click(screen.getByRole("button", { name: /^create booking$/i }));
    await waitFor(() => expect(createBooking).toHaveBeenCalled());
  });

  it("renders admin dashboard", async () => {
    setSession("admin");
    renderWithQuery(<DashboardWorkspace />);
    expect(await screen.findByText(/dashboard/i)).toBeInTheDocument();
  });

  it("payments page renders invoice details", async () => {
    setSession("admin");
    vi.mocked(getBookings).mockResolvedValueOnce({ items: [{ id: "BK-1", recordId: "b1", userId: "u1", roomId: "r1", guestName: "Guest", guestEmail: "g@test.com", roomNumber: "101", roomType: "Standard", checkIn: "2026-07-01", checkOut: "2026-07-02", amount: 120, paymentMethod: "credit-card", status: "confirmed", nights: 1, createdAt: "2026-01-01" }], totalCount: 1, page: 1, pageSize: 100 });
    renderWithQuery(<PaymentsWorkspace />);
    expect(await screen.findByText("INV-1")).toBeInTheDocument();
  });

  it("payment submit calls API", async () => {
    setSession("admin");
    vi.mocked(getBookings).mockResolvedValueOnce({ items: [{ id: "BK-1", recordId: "b1", userId: "u1", roomId: "r1", guestName: "Guest", guestEmail: "g@test.com", roomNumber: "101", roomType: "Standard", checkIn: "2026-07-01", checkOut: "2026-07-02", amount: 120, paymentMethod: "credit-card", status: "confirmed", nights: 1, createdAt: "2026-01-01" }], totalCount: 1, page: 1, pageSize: 100 });
    renderWithQuery(<PaymentsWorkspace />);
    await userEvent.click(await screen.findByRole("button", { name: /process payment/i }));
    await waitFor(() => expect(processPayment).toHaveBeenCalled());
  });

  it("payment page renders invoice API error", async () => {
    setSession("admin");
    vi.mocked(getBookings).mockResolvedValueOnce({ items: [{ id: "BK-1", recordId: "b1", userId: "u1", roomId: "r1", guestName: "Guest", guestEmail: "g@test.com", roomNumber: "101", roomType: "Standard", checkIn: "2026-07-01", checkOut: "2026-07-02", amount: 120, paymentMethod: "credit-card", status: "confirmed", nights: 1, createdAt: "2026-01-01" }], totalCount: 1, page: 1, pageSize: 100 });
    vi.mocked(getDefaultInvoice).mockRejectedValueOnce(new ApiError("Unable to load invoice details.", 500));
    renderWithQuery(<PaymentsWorkspace />);
    expect(await screen.findByText(/unable to load invoice details/i)).toBeInTheDocument();
  });

  it("housekeeping list renders and updates status", async () => {
    setSession("admin");
    renderWithQuery(<HousekeepingWorkspace />);
    expect(await screen.findByText("Room 101")).toBeInTheDocument();
    await userEvent.click(screen.getByRole("button", { name: /^clean$/i }));
    await waitFor(() => expect(updateHousekeepingTask).toHaveBeenCalledWith("h1", "clean", "Turnover"));
  });

  it("housekeeping controls are disabled for guests", async () => {
    setSession("guest");
    renderWithQuery(<HousekeepingWorkspace />);
    expect(await screen.findByRole("button", { name: /^clean$/i })).toBeDisabled();
  });

  it("housekeeping renders empty state", async () => {
    vi.mocked(getHousekeepingTasks).mockResolvedValueOnce([]);
    setSession("admin");
    renderWithQuery(<HousekeepingWorkspace />);
    expect(await screen.findByText(/no housekeeping tasks/i)).toBeInTheDocument();
  });

  it("notification badge and list render unread notifications", async () => {
    setSession("admin");
    renderWithQuery(<NotificationPanel />);
    await userEvent.click(screen.getByRole("button", { name: /open notifications/i }));
    expect(await screen.findByText(/1 unread items/i)).toBeInTheDocument();
    expect(screen.getByText("Booking Confirmed")).toBeInTheDocument();
  });

  it("notification mark as read calls API", async () => {
    setSession("admin");
    renderWithQuery(<NotificationPanel />);
    await userEvent.click(screen.getByRole("button", { name: /open notifications/i }));
    await userEvent.click(await screen.findByText("Booking Confirmed"));
    await waitFor(() => expect(markNotificationRead).toHaveBeenCalled());
    expect(vi.mocked(markNotificationRead).mock.calls[0][0]).toBe("n1");
  });

  it("notification empty state renders", async () => {
    vi.mocked(getNotifications).mockResolvedValueOnce({ items: [], unreadCount: 0, totalCount: 0, pageNumber: 1, pageSize: 8 });
    setSession("admin");
    renderWithQuery(<NotificationPanel />);
    await userEvent.click(screen.getByRole("button", { name: /open notifications/i }));
    expect(await screen.findByText(/no notifications are available/i)).toBeInTheDocument();
  });
});

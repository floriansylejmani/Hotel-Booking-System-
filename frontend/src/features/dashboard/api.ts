import { toBookingStatus, toNotificationTone, toPaymentMethod, toRoomStatus, toRoomType } from "@/lib/hotel-mappers";
import { formatRelativeTime } from "@/lib/format-relative";
import { apiClient } from "@/services/api-client";
import type { ActivityItem, Booking, DashboardKpi, QuickAction, Room } from "@/types/hotel";

// ─── API response shapes ──────────────────────────────────────────────────────

type DashboardSummaryDto = {
  totalRooms: number;
  availableRooms: number;
  occupiedRooms: number;
  cleaningRooms: number;
  maintenanceRooms: number;
  totalBookings: number;
  activeBookings: number;
  confirmedBookings: number;
  monthlyRevenue: number;
  unreadNotificationsCount: number;
};

type BookingResponseDto = {
  id: string;
  bookingCode: string;
  userId: string;
  guestName: string;
  guestEmail: string;
  roomId: string;
  roomNumber: string;
  roomType: string;
  checkInDate: string;
  checkOutDate: string;
  totalAmount: number;
  paymentMethod: string;
  status: string;
  nights: number;
  createdAt: string;
};

type OccupancyRoomDto = {
  roomId: string;
  roomNumber: string;
  type: string;
  status: string;
  pricePerNight: number;
  floorNumber: number;
  bedCount: number;
  amenities: string[] | null;
  guestName: string | null;
};

type DashboardOccupancyDto = {
  occupancyPercentage: number;
  rooms: OccupancyRoomDto[];
};

type ActivityFeedItemDto = {
  id: string;
  title: string;
  description: string;
  type: string;
  createdAt: string;
};

type DashboardActivityFeedDto = {
  items: ActivityFeedItemDto[];
};

// ─── Public types ─────────────────────────────────────────────────────────────

export type DashboardData = {
  kpis: DashboardKpi[];
  recentBookings: Booking[];
  activities: ActivityItem[];
  quickActions: QuickAction[];
  occupancy: Room[];
};

// ─── Mappers ──────────────────────────────────────────────────────────────────

function mapBookingDto(dto: BookingResponseDto): Booking {
  return {
    id: dto.bookingCode,
    recordId: dto.id,
    guestName: dto.guestName,
    guestEmail: dto.guestEmail,
    userId: dto.userId,
    roomId: dto.roomId,
    roomNumber: dto.roomNumber,
    roomType: toRoomType(dto.roomType),
    checkIn: dto.checkInDate,
    checkOut: dto.checkOutDate,
    amount: dto.totalAmount,
    paymentMethod: toPaymentMethod(dto.paymentMethod),
    status: toBookingStatus(dto.status),
    nights: dto.nights,
    createdAt: dto.createdAt,
  };
}

function mapOccupancyRoom(dto: OccupancyRoomDto): Room {
  return {
    id: dto.roomId,
    roomNumber: dto.roomNumber,
    type: toRoomType(dto.type),
    status: toRoomStatus(dto.status),
    pricePerNight: dto.pricePerNight,
    floor: dto.floorNumber,
    beds: dto.bedCount,
    amenities: dto.amenities ?? [],
    guestName: dto.guestName ?? undefined,
  };
}

function buildKpis(summary: DashboardSummaryDto): DashboardKpi[] {
  const occupancyRate = summary.totalRooms > 0
    ? Math.round((summary.occupiedRooms / summary.totalRooms) * 100)
    : 0;

  return [
    {
      label: "Rooms",
      value: String(summary.totalRooms),
      sublabel: `${summary.occupiedRooms} occupied right now`,
      delta: `${occupancyRate}% occupied`,
      tone: "blue" as const,
    },
    {
      label: "Bookings",
      value: String(summary.totalBookings),
      sublabel: `${summary.activeBookings} active stays`,
      delta: `${summary.confirmedBookings} upcoming`,
      tone: "indigo" as const,
    },
    {
      label: "Room Readiness",
      value: String(summary.availableRooms),
      sublabel: "available rooms",
      delta: `${summary.cleaningRooms} cleaning`,
      tone: "green" as const,
    },
    {
      label: "Monthly Revenue",
      value: `$${Math.round(summary.monthlyRevenue).toLocaleString("en-US")}`,
      sublabel: "payments this month",
      delta: `${summary.unreadNotificationsCount} unread alerts`,
      tone: "orange" as const,
    },
  ];
}

const quickActions: QuickAction[] = [
  {
    id: "new-booking",
    label: "New booking",
    description: "Open the bookings workspace to add a reservation.",
    tone: "blue" as const,
  },
  {
    id: "checkin-desk",
    label: "Check-in desk",
    description: "Process arrivals and departures from the stay desk.",
    tone: "green" as const,
  },
  {
    id: "housekeeping-board",
    label: "Housekeeping board",
    description: "Review room readiness and pending maintenance tasks.",
    tone: "orange" as const,
  },
  {
    id: "billing",
    label: "Payments",
    description: "Review invoice details and settle open balances.",
    tone: "indigo" as const,
  },
];

// ─── Main fetch ───────────────────────────────────────────────────────────────

export async function getDashboardData(): Promise<DashboardData> {
  const [summary, recentBookingDtos, activityFeed, occupancyData] = await Promise.all([
    apiClient<DashboardSummaryDto>("/dashboard/summary"),
    apiClient<BookingResponseDto[]>("/dashboard/recent-bookings?count=5"),
    apiClient<DashboardActivityFeedDto>("/dashboard/activity-feed?count=6"),
    apiClient<DashboardOccupancyDto>("/dashboard/occupancy"),
  ]);

  return {
    kpis: buildKpis(summary),
    recentBookings: recentBookingDtos.map(mapBookingDto),
    activities: activityFeed.items.map((item) => ({
      id: item.id,
      title: item.title,
      description: item.description,
      timeAgo: formatRelativeTime(item.createdAt),
      tone: toNotificationTone(item.type),
    })),
    quickActions,
    occupancy: occupancyData.rooms.map(mapOccupancyRoom),
  };
}

import { getInitials } from "@/lib/format";
import type {
  BookingStatus,
  HousekeepingStatus,
  NotificationTone,
  PaymentMethod,
  RoomStatus,
  RoomType,
  UserRole,
} from "@/types/hotel";

export function toUserRole(role: string): UserRole {
  switch (role.trim().toLowerCase()) {
    case "admin":
      return "admin";
    case "manager":
      return "manager";
    case "receptionist":
      return "receptionist";
    case "housekeeper":
      return "housekeeper";
    default:
      return "guest";
  }
}

export function toAuthUserAvatar(name: string) {
  return getInitials(name);
}

export function toRoomType(type: string): RoomType {
  switch (type) {
    case "BusinessSuite":
    case "Business Suite":
      return "Business Suite";
    case "Deluxe":
    case "Family":
    case "Standard":
      return type;
    default:
      return "Standard";
  }
}

export function toRoomTypeEnum(type: RoomType) {
  switch (type) {
    case "Standard":
      return 1;
    case "Deluxe":
      return 2;
    case "Family":
      return 3;
    case "Business Suite":
      return 4;
    default:
      return 1;
  }
}

export function toRoomStatus(status: string): RoomStatus {
  switch (status.toLowerCase()) {
    case "occupied":
      return "occupied";
    case "cleaning":
      return "cleaning";
    case "maintenance":
      return "maintenance";
    default:
      return "available";
  }
}

export function toRoomStatusEnum(status: RoomStatus) {
  switch (status) {
    case "available":
      return 1;
    case "occupied":
      return 2;
    case "cleaning":
      return 3;
    case "maintenance":
      return 4;
    default:
      return 1;
  }
}

export function toBookingStatus(status: string): BookingStatus {
  switch (status.toLowerCase()) {
    case "active":
      return "active";
    case "checkedout":
    case "checked-out":
      return "checked-out";
    case "cancelled":
      return "cancelled";
    default:
      return "confirmed";
  }
}

export function toPaymentMethod(method: string): PaymentMethod {
  switch (method.toLowerCase()) {
    case "cash":
      return "cash";
    case "check":
      return "check";
    default:
      return "credit-card";
  }
}

export function toPaymentMethodApi(method: PaymentMethod) {
  switch (method) {
    case "cash":
      return "Cash";
    case "check":
      return "Check";
    default:
      return "Card";
  }
}

export function toHousekeepingStatus(status: string): HousekeepingStatus {
  switch (status.toLowerCase()) {
    case "dirty":
      return "dirty";
    case "inprogress":
    case "in-progress":
      return "in-progress";
    case "maintenance":
      return "maintenance";
    default:
      return "clean";
  }
}

export function toHousekeepingStatusEnum(status: HousekeepingStatus) {
  switch (status) {
    case "clean":
      return 1;
    case "dirty":
      return 2;
    case "in-progress":
      return 3;
    case "maintenance":
      return 4;
    default:
      return 1;
  }
}

export function toNotificationTone(type: string): NotificationTone {
  switch (type) {
    case "BookingCancelled":
    case "MaintenanceAlert":
      return "warning";
    case "PaymentCompleted":
    case "CheckInCompleted":
    case "CheckOutCompleted":
      return "success";
    default:
      return "info";
  }
}

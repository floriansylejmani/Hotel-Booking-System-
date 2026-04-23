export type UserRole =
  | "admin"
  | "manager"
  | "receptionist"
  | "housekeeper"
  | "guest";

export type RoomType = "Standard" | "Deluxe" | "Family" | "Business Suite";

export type RoomStatus =
  | "available"
  | "occupied"
  | "reserved"
  | "maintenance"
  | "cleaning";

export type BookingStatus =
  | "active"
  | "confirmed"
  | "checked-in"
  | "checked-out"
  | "pending"
  | "cancelled";

export type PaymentMethod = "credit-card" | "cash" | "check";

export type HousekeepingStatus =
  | "clean"
  | "dirty"
  | "in-progress"
  | "maintenance";

export type NotificationTone = "info" | "success" | "warning" | "danger";

export type Permission =
  | "dashboard.view"
  | "rooms.manage"
  | "bookings.manage"
  | "checkin.manage"
  | "housekeeping.manage"
  | "payments.manage"
  | "roles.manage";

export type Amenity = string;

export type AuthUser = {
  id: string;
  name: string;
  email: string;
  role: UserRole;
  avatar: string;
};

export type AuthSession = {
  user: AuthUser;
  token: string;
  expiresAt: string;
};

export type Room = {
  id: string;
  roomNumber: string;
  type: RoomType;
  pricePerNight: number;
  floor: number;
  beds: number;
  amenities: Amenity[];
  status: RoomStatus;
  guestName?: string;
  bookingId?: string;
  createdAt?: string;
};

export type Booking = {
  id: string;
  recordId: string;
  guestName: string;
  guestEmail?: string;
  userId?: string;
  roomId?: string;
  roomNumber: string;
  roomType: RoomType;
  checkIn: string;
  checkOut: string;
  amount: number;
  paymentMethod: PaymentMethod;
  status: BookingStatus;
  nights?: number;
  specialRequest?: string;
  actualCheckIn?: string;
  actualCheckOut?: string;
  createdAt?: string;
};

export type DashboardKpi = {
  label: string;
  value: string;
  sublabel: string;
  delta?: string;
  tone: "blue" | "green" | "indigo" | "orange";
};

export type ActivityItem = {
  id: string;
  title: string;
  description: string;
  timeAgo: string;
  tone: NotificationTone;
};

export type QuickAction = {
  id: string;
  label: string;
  description: string;
  tone: "blue" | "green" | "orange" | "indigo";
};

export type HousekeepingTask = {
  id: string;
  roomNumber: string;
  floor: number;
  assignedTo?: string;
  lastCleaned?: string;
  status: HousekeepingStatus;
  notes?: string;
  maintenanceNotes?: string;
  priorityNote?: string;
};

export type InvoiceLineItem = {
  id: string;
  description: string;
  rateLabel: string;
  total: number;
};

export type Invoice = {
  invoiceId: string;
  invoiceNumber: string;
  issuedDate: string;
  guestName: string;
  bookingId: string;
  bookingRecordId: string;
  roomNumber: string;
  roomType: RoomType;
  checkIn: string;
  checkOut: string;
  hotelName: string;
  hotelAddress: string;
  items: InvoiceLineItem[];
  subtotal: number;
  tax: number;
  total: number;
};

export type RoleDefinition = {
  role: UserRole;
  title: string;
  description: string;
  permissions: string[];
  tone: "indigo" | "blue" | "green" | "orange";
};

export type AppNotification = {
  id: string;
  title: string;
  description: string;
  type: string;
  createdAt: string;
  createdAtLabel: string;
  tone: NotificationTone;
  read: boolean;
};

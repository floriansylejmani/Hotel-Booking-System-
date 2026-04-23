import { formatRelativeTime } from "@/lib/format-relative";
import { toNotificationTone } from "@/lib/hotel-mappers";
import { apiClient, toQueryString } from "@/services/api-client";
import type { AppNotification } from "@/types/hotel";

type NotificationResponseDto = {
  id: string;
  title: string;
  message: string;
  type: string;
  isRead: boolean;
  createdAt: string;
};

type NotificationsPagedResultDto = {
  items: NotificationResponseDto[];
  unreadCount: number;
  totalCount: number;
  pageNumber: number;
  pageSize: number;
};

export const notificationsQueryKey = ["notifications"] as const;

function mapNotification(dto: NotificationResponseDto): AppNotification {
  return {
    id: dto.id,
    title: dto.title,
    description: dto.message,
    type: dto.type,
    createdAt: dto.createdAt,
    createdAtLabel: formatRelativeTime(dto.createdAt),
    tone: toNotificationTone(dto.type),
    read: dto.isRead,
  };
}

export async function getNotifications(params?: {
  unreadOnly?: boolean;
  pageNumber?: number;
  pageSize?: number;
}) {
  const response = await apiClient<NotificationsPagedResultDto>(
    `/notifications${toQueryString({
      unreadOnly: params?.unreadOnly,
      pageNumber: params?.pageNumber ?? 1,
      pageSize: params?.pageSize ?? 20,
    })}`,
  );

  return {
    items: response.items.map(mapNotification),
    unreadCount: response.unreadCount,
    totalCount: response.totalCount,
    pageNumber: response.pageNumber,
    pageSize: response.pageSize,
  };
}

export async function markNotificationRead(notificationId: string) {
  await apiClient<void>(`/notifications/${notificationId}/read`, {
    method: "PUT",
    responseType: "void",
  });
}

export async function markAllNotificationsRead() {
  await apiClient<void>("/notifications/read-all", {
    method: "PUT",
    responseType: "void",
  });
}

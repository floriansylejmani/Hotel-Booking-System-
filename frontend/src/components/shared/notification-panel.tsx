"use client";

import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import { Bell, CheckCheck } from "lucide-react";
import { useState } from "react";
import {
  getNotifications,
  markAllNotificationsRead,
  markNotificationRead,
  notificationsQueryKey,
} from "@/features/notifications/api";
import { Badge } from "@/components/ui/badge";
import { Button } from "@/components/ui/button";
import { cn } from "@/lib/utils";

export function NotificationPanel() {
  const queryClient = useQueryClient();
  const [open, setOpen] = useState(false);
  const notificationsQuery = useQuery({
    queryKey: [...notificationsQueryKey, "panel"],
    queryFn: () => getNotifications({ pageNumber: 1, pageSize: 8 }),
  });

  const markReadMutation = useMutation({
    mutationFn: markNotificationRead,
    onSuccess: async () => {
      await Promise.all([
        queryClient.invalidateQueries({ queryKey: notificationsQueryKey }),
        queryClient.invalidateQueries({ queryKey: ["dashboard"] }),
      ]);
    },
  });

  const markAllMutation = useMutation({
    mutationFn: markAllNotificationsRead,
    onSuccess: async () => {
      await Promise.all([
        queryClient.invalidateQueries({ queryKey: notificationsQueryKey }),
        queryClient.invalidateQueries({ queryKey: ["dashboard"] }),
      ]);
    },
  });

  const notifications = notificationsQuery.data?.items ?? [];
  const unreadCount = notificationsQuery.data?.unreadCount ?? 0;

  return (
    <div className="relative">
      <Button
        variant="ghost"
        size="icon"
        className="relative"
        onClick={() => setOpen((current) => !current)}
        aria-label="Open notifications"
      >
        <Bell className="size-4" />
        {unreadCount ? (
          <span className="absolute right-2 top-2 size-2 rounded-full bg-accent-red" />
        ) : null}
      </Button>

      {open ? (
        <div className="glass-surface absolute right-0 top-14 z-30 w-[22rem] rounded-[1.6rem] border border-surface-border bg-[linear-gradient(180deg,rgba(17,24,39,0.98),rgba(7,11,20,0.98))] p-4 shadow-[0_24px_60px_-36px_rgba(2,6,23,0.88)]">
          <div className="flex items-center justify-between gap-3">
            <div>
              <p className="text-sm font-semibold text-white">Notifications</p>
              <p className="text-xs text-muted-foreground">
                {unreadCount} unread items
              </p>
            </div>
            <Button
              variant="secondary"
              size="sm"
              onClick={() => void markAllMutation.mutateAsync()}
              disabled={!notifications.length || markAllMutation.isPending}
            >
              <CheckCheck className="size-4" />
              Read all
            </Button>
          </div>

          <div className="mt-4 space-y-3">
            {notificationsQuery.isLoading ? (
              <p className="text-sm text-muted-foreground">Loading notifications...</p>
            ) : notificationsQuery.isError ? (
              <p className="text-sm text-accent-red">
                Unable to load notifications right now.
              </p>
            ) : notifications.length ? (
              notifications.map((notification) => (
                <button
                  key={notification.id}
                  type="button"
                  onClick={() =>
                    notification.read
                      ? undefined
                      : void markReadMutation.mutateAsync(notification.id)
                  }
                  className={cn(
                    "w-full rounded-[1.25rem] border p-3 text-left transition",
                    notification.read
                      ? "border-surface-border bg-white/[0.02]"
                      : "border-surface-border-strong bg-white/[0.05]",
                  )}
                >
                  <div className="flex items-start justify-between gap-3">
                    <div>
                      <p className="text-sm font-semibold text-white">
                        {notification.title}
                      </p>
                      <p className="mt-1 text-sm leading-5 text-muted-foreground">
                        {notification.description}
                      </p>
                    </div>
                    {!notification.read ? (
                      <Badge variant="indigo" size="sm">
                        New
                      </Badge>
                    ) : null}
                  </div>
                  <p className="mt-3 text-xs uppercase tracking-[0.2em] text-muted-foreground">
                    {notification.createdAtLabel}
                  </p>
                </button>
              ))
            ) : (
              <p className="text-sm text-muted-foreground">
                No notifications are available yet.
              </p>
            )}
          </div>
        </div>
      ) : null}
    </div>
  );
}

"use client";

import { useEffect } from "react";
import { AlertCircle, BellRing, CheckCircle2, X } from "lucide-react";
import { useToast } from "@/hooks/use-toast";
import { cn } from "@/lib/utils";
import type { NotificationTone } from "@/types/hotel";

const toneStyles: Record<NotificationTone, string> = {
  info: "border-accent-blue/25 bg-accent-blue/12 text-accent-cyan",
  success: "border-accent-green/25 bg-accent-green/12 text-accent-green",
  warning: "border-accent-orange/25 bg-accent-orange/12 text-accent-orange",
  danger: "border-accent-red/25 bg-accent-red/12 text-accent-red",
};

const toneIcons = {
  info: BellRing,
  success: CheckCircle2,
  warning: AlertCircle,
  danger: AlertCircle,
} as const;

export function ToastViewport() {
  const { dismissToast, toasts } = useToast();

  useEffect(() => {
    const timers = toasts.map((toast) =>
      window.setTimeout(() => dismissToast(toast.id), 3200),
    );

    return () => {
      timers.forEach((timer) => window.clearTimeout(timer));
    };
  }, [dismissToast, toasts]);

  return (
    <div className="pointer-events-none fixed bottom-4 right-4 z-[60] flex w-full max-w-sm flex-col gap-3">
      {toasts.map((toast) => {
        const Icon = toneIcons[toast.tone];

        return (
          <div
            key={toast.id}
            className="glass-surface pointer-events-auto rounded-[1.4rem] border p-4 shadow-[0_20px_48px_-28px_rgba(2,6,23,0.8)]"
          >
            <div className="flex items-start gap-3">
              <div
                className={cn(
                  "flex size-10 items-center justify-center rounded-2xl border",
                  toneStyles[toast.tone],
                )}
              >
                <Icon className="size-4" />
              </div>
              <div className="min-w-0 flex-1">
                <p className="text-sm font-semibold text-white">{toast.title}</p>
                {toast.description ? (
                  <p className="mt-1 text-sm leading-5 text-muted-foreground">
                    {toast.description}
                  </p>
                ) : null}
              </div>
              <button
                type="button"
                onClick={() => dismissToast(toast.id)}
                className="rounded-full p-1 text-muted-foreground transition hover:bg-white/[0.05] hover:text-white"
                aria-label="Dismiss toast"
              >
                <X className="size-4" />
              </button>
            </div>
          </div>
        );
      })}
    </div>
  );
}

"use client";

import Link from "next/link";
import { usePathname } from "next/navigation";
import { LogOut, X } from "lucide-react";
import { BrandMark } from "@/components/shared/brand-mark";
import { Button } from "@/components/ui/button";
import { Badge } from "@/components/ui/badge";
import { useAuth } from "@/hooks/use-auth";
import { formatRoleLabel, getInitials } from "@/lib/format";
import { mainNavigation } from "@/lib/navigation";
import { hasPermission } from "@/lib/permissions";
import { useAppShell } from "@/hooks/use-app-shell";
import { cn } from "@/lib/utils";

export function AppSidebar() {
  const pathname = usePathname();
  const { closeSidebar, isSidebarOpen } = useAppShell();
  const { logout, user } = useAuth();
  const navigationItems = user
    ? mainNavigation.filter((item) => hasPermission(user.role, item.permission))
    : [];

  return (
    <>
      <div
        className={cn(
          "fixed inset-0 z-30 bg-[#030712]/72 backdrop-blur-sm transition-opacity lg:hidden",
          isSidebarOpen ? "opacity-100" : "pointer-events-none opacity-0",
        )}
        onClick={closeSidebar}
      />

      <aside
        className={cn(
          "glass-surface fixed inset-y-0 left-0 z-40 flex w-[18.5rem] flex-col rounded-r-[2rem] border-l-0 transition-transform duration-300 lg:translate-x-0",
          isSidebarOpen ? "translate-x-0" : "-translate-x-full",
        )}
      >
        <div className="flex items-center justify-between border-b border-surface-border px-5 py-5">
          <BrandMark />
          <Button
            variant="ghost"
            size="icon"
            className="lg:hidden"
            onClick={closeSidebar}
            aria-label="Close navigation"
          >
            <X className="size-4" />
          </Button>
        </div>

        <div className="flex flex-1 flex-col justify-between overflow-y-auto px-4 py-5">
          <nav className="space-y-2">
            {navigationItems.map((item) => {
              const isActive =
                pathname === item.href || pathname.startsWith(`${item.href}/`);
              const Icon = item.icon;

              return (
                <Link
                  key={item.href}
                  href={item.href}
                  onClick={closeSidebar}
                  className={cn(
                    "group flex items-center gap-3 rounded-2xl border px-3.5 py-3 transition-all",
                    isActive
                      ? "border-[rgba(104,95,255,0.35)] bg-[linear-gradient(90deg,rgba(91,124,255,0.16),rgba(124,99,255,0.18))] text-white soft-glow"
                      : "border-transparent text-slate-300 hover:border-surface-border hover:bg-white/[0.03] hover:text-white",
                  )}
                >
                  <span
                    className={cn(
                      "flex size-10 items-center justify-center rounded-2xl border transition-all",
                      isActive
                        ? "border-transparent bg-white/[0.06] text-accent-indigo"
                        : "border-surface-border bg-white/[0.02] text-slate-400 group-hover:text-slate-100",
                    )}
                  >
                    <Icon className="size-4" />
                  </span>
                  <span className="flex min-w-0 flex-1 flex-col">
                    <span className="truncate text-sm font-semibold tracking-[0.01em]">
                      {item.label}
                    </span>
                    <span className="truncate text-xs text-muted-foreground">
                      {item.caption}
                    </span>
                  </span>
                  <Badge variant={isActive ? "indigo" : "neutral"} size="sm">
                    {item.phase}
                  </Badge>
                </Link>
              );
            })}
          </nav>

          <div className="mt-8 rounded-[1.6rem] border border-surface-border bg-white/[0.03] p-4">
            <div className="flex items-center gap-3">
              <div className="flex size-11 items-center justify-center rounded-2xl bg-[linear-gradient(135deg,#7c63ff,#4f7cff)] text-sm font-bold text-white">
                {user?.avatar ?? getInitials("Guest User")}
              </div>
              <div className="min-w-0">
                <p className="truncate text-sm font-semibold text-white">
                  {user?.name ?? "Guest User"}
                </p>
                <p className="truncate text-xs text-muted-foreground">
                  {user ? formatRoleLabel(user.role) : "Unsigned"}
                </p>
              </div>
            </div>

            <div className="mt-4 rounded-2xl border border-surface-border bg-black/10 p-3">
              <p className="text-xs uppercase tracking-[0.28em] text-muted-foreground">
                System status
              </p>
              <p className="mt-2 text-sm font-medium text-slate-100">
                Frontend is connected to the live HBS backend for operational modules.
              </p>
            </div>

            <Button
              variant="secondary"
              className="mt-4 w-full justify-center"
              onClick={logout}
            >
              <LogOut className="size-4" />
              Sign out
            </Button>
          </div>
        </div>
      </aside>
    </>
  );
}

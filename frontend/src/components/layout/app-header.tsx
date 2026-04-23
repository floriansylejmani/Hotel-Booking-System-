"use client";

import { zodResolver } from "@hookform/resolvers/zod";
import { Menu, Search } from "lucide-react";
import { usePathname, useRouter, useSearchParams } from "next/navigation";
import { startTransition, useEffect } from "react";
import { useForm } from "react-hook-form";
import { NotificationPanel } from "@/components/shared/notification-panel";
import { z } from "zod";
import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
import { pageTitles } from "@/lib/navigation";
import { useAppShell } from "@/hooks/use-app-shell";
import { cn } from "@/lib/utils";

const globalSearchSchema = z.object({
  query: z.string().trim().max(80, "Search must stay under 80 characters."),
});

type GlobalSearchValues = z.infer<typeof globalSearchSchema>;

const headerDateFormatter = new Intl.DateTimeFormat("en-US", {
  weekday: "short",
  month: "short",
  day: "numeric",
  year: "numeric",
});

export function AppHeader() {
  const pathname = usePathname();
  const router = useRouter();
  const searchParams = useSearchParams();
  const { isSidebarOpen, toggleSidebar } = useAppShell();
  const pageTitle = pageTitles[pathname] ?? "Hotel Operations";

  const form = useForm<GlobalSearchValues>({
    resolver: zodResolver(globalSearchSchema),
    defaultValues: {
      query: searchParams.get("q") ?? "",
    },
  });

  useEffect(() => {
    form.reset({
      query: searchParams.get("q") ?? "",
    });
  }, [form, searchParams]);

  const onSubmit = form.handleSubmit(({ query }) => {
    const params = new URLSearchParams(searchParams.toString());

    if (query) {
      params.set("q", query);
    } else {
      params.delete("q");
    }

    startTransition(() => {
      const search = params.toString();
      router.replace(search ? `${pathname}?${search}` : pathname);
    });
  });

  return (
    <header className="sticky top-0 z-20 border-b border-surface-border bg-[rgba(6,11,22,0.78)] backdrop-blur-xl">
      <div className="mx-auto flex w-full max-w-[1400px] items-center gap-4 px-4 py-4 sm:px-6 lg:px-8">
        <Button
          variant="ghost"
          size="icon"
          className="lg:hidden"
          onClick={toggleSidebar}
          aria-label={isSidebarOpen ? "Close navigation" : "Open navigation"}
        >
          <Menu className="size-4" />
        </Button>

        <div className="min-w-0 flex-1">
          <p className="text-xs uppercase tracking-[0.28em] text-muted-foreground">
            LuxeStay Platform
          </p>
          <h1 className="truncate font-sans text-2xl font-bold tracking-tight text-white">
            {pageTitle}
          </h1>
        </div>

        <form
          onSubmit={onSubmit}
          className="hidden w-full max-w-md items-start gap-3 xl:flex"
        >
          <div className="w-full">
            <div className="relative">
              <Search className="pointer-events-none absolute left-4 top-1/2 size-4 -translate-y-1/2 text-muted-foreground" />
              <Input
                {...form.register("query")}
                placeholder="Search bookings, rooms, or booking IDs..."
                className={cn(
                  "pl-11",
                  form.formState.errors.query && "border-accent-red/50 ring-2 ring-accent-red/15",
                )}
              />
            </div>
            {form.formState.errors.query ? (
              <p className="mt-1 text-xs text-accent-red">
                {form.formState.errors.query.message}
              </p>
            ) : null}
          </div>
          <Button type="submit" variant="secondary">
            Search
          </Button>
        </form>

        <div className="hidden text-right sm:block">
          <p className="text-xs uppercase tracking-[0.28em] text-muted-foreground">
            Today
          </p>
          <p suppressHydrationWarning className="text-sm font-medium text-slate-200">
            {headerDateFormatter.format(new Date())}
          </p>
        </div>

        <NotificationPanel />
      </div>
    </header>
  );
}

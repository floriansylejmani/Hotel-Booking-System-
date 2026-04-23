import { Suspense, type ReactNode } from "react";
import { AppHeader } from "@/components/layout/app-header";
import { AppSidebar } from "@/components/layout/app-sidebar";

export function AppShell({ children }: Readonly<{ children: ReactNode }>) {
  return (
    <div className="app-shell-noise min-h-screen text-foreground">
      <AppSidebar />
      <div className="min-h-screen transition-all duration-300 lg:pl-[18.5rem]">
        <Suspense fallback={<HeaderFallback />}>
          <AppHeader />
        </Suspense>
        <main className="px-4 pb-8 pt-4 sm:px-6 lg:px-8 lg:pb-10 lg:pt-6">
          <div className="mx-auto w-full max-w-[1400px]">{children}</div>
        </main>
      </div>
    </div>
  );
}

function HeaderFallback() {
  return (
    <div className="sticky top-0 z-20 border-b border-surface-border bg-[rgba(6,11,22,0.78)] backdrop-blur-xl">
      <div className="mx-auto flex w-full max-w-[1400px] items-center justify-between gap-4 px-4 py-4 sm:px-6 lg:px-8">
        <div className="space-y-2">
          <div className="h-3 w-28 rounded-full bg-white/[0.05]" />
          <div className="h-8 w-48 rounded-full bg-white/[0.07]" />
        </div>
        <div className="hidden h-11 w-full max-w-md rounded-2xl border border-surface-border bg-white/[0.04] xl:block" />
        <div className="h-11 w-11 rounded-2xl border border-surface-border bg-white/[0.04]" />
      </div>
    </div>
  );
}

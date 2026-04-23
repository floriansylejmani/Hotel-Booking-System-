import type { ReactNode } from "react";
import { PublicOnly } from "@/components/auth/public-only";
import { BrandMark } from "@/components/shared/brand-mark";

export default function AuthLayout({
  children,
}: Readonly<{ children: ReactNode }>) {
  return (
    <PublicOnly>
      <main className="relative min-h-screen overflow-hidden bg-[linear-gradient(180deg,#07101d,#030712)] px-4 py-8 text-white">
        <div className="absolute inset-0 bg-[radial-gradient(circle_at_top_left,rgba(91,124,255,0.18),transparent_28%),radial-gradient(circle_at_bottom_right,rgba(49,208,154,0.12),transparent_26%)]" />
        <div className="relative mx-auto flex min-h-[calc(100vh-4rem)] w-full max-w-7xl flex-col justify-between gap-10 lg:flex-row lg:items-center">
          <section className="max-w-xl space-y-8">
            <BrandMark />
            <div className="space-y-5">
              <p className="text-xs uppercase tracking-[0.32em] text-accent-cyan">
                Hotel Booking Management
              </p>
              <h1 className="font-sans text-5xl font-bold tracking-tight text-white">
                Premium operations software for luxury hotel teams.
              </h1>
              <p className="max-w-lg text-base leading-7 text-slate-300">
                Manage rooms, reservations, check-ins, housekeeping, and billing
                from one dark premium workspace tailored for front-desk and
                operations staff.
              </p>
            </div>
            <div className="grid gap-4 sm:grid-cols-3">
              <div className="glass-surface rounded-[1.5rem] p-4">
                <p className="text-2xl font-bold text-white">12</p>
                <p className="mt-2 text-sm text-muted-foreground">Luxury rooms</p>
              </div>
              <div className="glass-surface rounded-[1.5rem] p-4">
                <p className="text-2xl font-bold text-white">5</p>
                <p className="mt-2 text-sm text-muted-foreground">Active stays</p>
              </div>
              <div className="glass-surface rounded-[1.5rem] p-4">
                <p className="text-2xl font-bold text-white">$18.6k</p>
                <p className="mt-2 text-sm text-muted-foreground">Monthly revenue</p>
              </div>
            </div>
          </section>

          <section className="w-full max-w-xl">{children}</section>
        </div>
      </main>
    </PublicOnly>
  );
}

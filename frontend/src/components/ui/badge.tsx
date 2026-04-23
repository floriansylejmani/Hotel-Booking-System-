import type { HTMLAttributes } from "react";
import { cn } from "@/lib/utils";

type BadgeVariant =
  | "blue"
  | "indigo"
  | "green"
  | "orange"
  | "red"
  | "neutral";

type BadgeSize = "sm" | "md";

const badgeVariants: Record<BadgeVariant, string> = {
  blue: "border border-accent-blue/24 bg-accent-blue/12 text-accent-cyan",
  indigo: "border border-accent-indigo/24 bg-accent-indigo/14 text-[#b6a8ff]",
  green: "border border-accent-green/20 bg-accent-green/10 text-accent-green",
  orange: "border border-accent-orange/22 bg-accent-orange/10 text-accent-orange",
  red: "border border-accent-red/22 bg-accent-red/10 text-accent-red",
  neutral: "border border-surface-border bg-white/[0.04] text-slate-300",
};

const badgeSizes: Record<BadgeSize, string> = {
  sm: "px-2.5 py-1 text-[0.65rem]",
  md: "px-3 py-1.5 text-[0.68rem]",
};

export type BadgeProps = HTMLAttributes<HTMLSpanElement> & {
  size?: BadgeSize;
  variant?: BadgeVariant;
};

export function Badge({
  className,
  size = "sm",
  variant = "neutral",
  ...props
}: BadgeProps) {
  return (
    <span
      className={cn(
        "inline-flex items-center justify-center rounded-full font-semibold uppercase tracking-[0.18em]",
        badgeVariants[variant],
        badgeSizes[size],
        className,
      )}
      {...props}
    />
  );
}

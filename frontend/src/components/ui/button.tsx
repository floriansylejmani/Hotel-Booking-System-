import { forwardRef } from "react";
import type { ButtonHTMLAttributes } from "react";
import { cn } from "@/lib/utils";

type ButtonVariant = "primary" | "secondary" | "ghost" | "danger";
type ButtonSize = "sm" | "md" | "lg" | "icon";

const variantClasses: Record<ButtonVariant, string> = {
  primary:
    "bg-[linear-gradient(135deg,#4f7cff,#7c63ff)] text-white shadow-[0_22px_42px_-24px_rgba(99,102,241,0.7)] hover:brightness-110",
  secondary:
    "border border-surface-border bg-white/[0.04] text-slate-100 hover:border-surface-border-strong hover:bg-white/[0.08]",
  ghost:
    "border border-transparent bg-transparent text-slate-300 hover:border-surface-border hover:bg-white/[0.04] hover:text-white",
  danger:
    "border border-accent-red/28 bg-accent-red/10 text-accent-red hover:bg-accent-red/18",
};

const sizeClasses: Record<ButtonSize, string> = {
  sm: "h-9 px-3.5 text-sm",
  md: "h-11 px-4 text-sm",
  lg: "h-12 px-5 text-sm",
  icon: "size-11 p-0",
};

export type ButtonProps = ButtonHTMLAttributes<HTMLButtonElement> & {
  variant?: ButtonVariant;
  size?: ButtonSize;
};

export const Button = forwardRef<HTMLButtonElement, ButtonProps>(
  (
    { className, size = "md", type = "button", variant = "primary", ...props },
    ref,
  ) => {
    return (
      <button
        ref={ref}
        type={type}
        className={cn(
          "inline-flex items-center justify-center gap-2 rounded-2xl font-semibold transition-all outline-none focus-visible:ring-2 focus-visible:ring-ring disabled:pointer-events-none disabled:opacity-50",
          variantClasses[variant],
          sizeClasses[size],
          className,
        )}
        {...props}
      />
    );
  },
);

Button.displayName = "Button";

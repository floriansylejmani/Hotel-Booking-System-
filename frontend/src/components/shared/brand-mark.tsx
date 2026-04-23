import { KeyRound } from "lucide-react";

export function BrandMark() {
  return (
    <div className="flex items-center gap-3">
      <div className="flex size-11 items-center justify-center rounded-2xl bg-[linear-gradient(135deg,#5b7cff,#7c63ff)] text-white shadow-[0_18px_38px_-20px_rgba(99,102,241,0.65)]">
        <KeyRound className="size-5" />
      </div>
      <div>
        <p className="font-sans text-lg font-bold tracking-tight text-white">
          LuxeStay
        </p>
        <p className="text-xs uppercase tracking-[0.22em] text-muted-foreground">
          Hotel Management
        </p>
      </div>
    </div>
  );
}

import type { LucideIcon } from "lucide-react";
import { ArrowUpRight } from "lucide-react";
import { Badge } from "@/components/ui/badge";
import { Card, CardDescription, CardHeader, CardTitle } from "@/components/ui/card";
import { cn } from "@/lib/utils";

type MetricCardProps = {
  icon: LucideIcon;
  label: string;
  value: string;
  sublabel: string;
  delta?: string;
  tone: "blue" | "green" | "indigo" | "orange";
};

const toneMap = {
  blue: "text-accent-cyan",
  green: "text-accent-green",
  indigo: "text-[#b6a8ff]",
  orange: "text-accent-orange",
} as const;

export function MetricCard({
  delta,
  icon: Icon,
  label,
  sublabel,
  tone,
  value,
}: MetricCardProps) {
  return (
    <Card className="overflow-hidden bg-[linear-gradient(180deg,rgba(14,21,37,0.94),rgba(7,11,20,0.94))]">
      <CardHeader className="space-y-5">
        <div className="flex items-start justify-between gap-4">
          <div className="flex size-13 items-center justify-center rounded-[1.3rem] border border-white/8 bg-white/[0.05] text-white">
            <Icon className="size-5" />
          </div>
          {delta ? (
            <Badge variant={tone === "orange" ? "green" : tone} size="sm" className="gap-1 tracking-[0.14em]">
              <ArrowUpRight className="size-3" />
              {delta}
            </Badge>
          ) : null}
        </div>
        <div>
          <CardTitle className="text-[2rem]">{value}</CardTitle>
          <CardDescription className="mt-2 text-base text-slate-200">
            {label}
          </CardDescription>
          <p className={cn("mt-2 text-sm font-semibold", toneMap[tone])}>{sublabel}</p>
        </div>
      </CardHeader>
    </Card>
  );
}

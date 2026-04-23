import type { LucideIcon } from "lucide-react";
import { ArrowRight } from "lucide-react";
import { Badge } from "@/components/ui/badge";
import { Button } from "@/components/ui/button";
import {
  Card,
  CardContent,
  CardDescription,
  CardHeader,
  CardTitle,
} from "@/components/ui/card";
import { SectionHeading } from "@/components/ui/section-heading";
import { cn } from "@/lib/utils";

type AccentTone = "blue" | "indigo" | "green" | "orange" | "red";

const accentStyles: Record<AccentTone, string> = {
  blue: "from-accent-blue/22 to-accent-cyan/14 border-accent-blue/25",
  indigo: "from-accent-indigo/22 to-accent-blue/16 border-accent-indigo/25",
  green: "from-accent-green/22 to-accent-cyan/14 border-accent-green/25",
  orange: "from-accent-orange/20 to-accent-red/12 border-accent-orange/20",
  red: "from-accent-red/20 to-accent-orange/12 border-accent-red/20",
};

type ModulePlaceholderProps = {
  icon: LucideIcon;
  title: string;
  phase: string;
  summary: string;
  accent: AccentTone;
  deliverables: string[];
  foundationItems: string[];
};

export function ModulePlaceholder({
  accent,
  deliverables,
  foundationItems,
  icon: Icon,
  phase,
  summary,
  title,
}: ModulePlaceholderProps) {
  return (
    <div className="space-y-6">
      <SectionHeading
        eyebrow={phase}
        title={title}
        description={summary}
        actions={
          <Button variant="secondary">
            Phase Ready
            <ArrowRight className="size-4" />
          </Button>
        }
      />

      <div className="grid gap-6 xl:grid-cols-[minmax(0,1.2fr)_minmax(0,0.8fr)]">
        <Card
          className={cn(
            "overflow-hidden bg-[linear-gradient(180deg,rgba(17,24,39,0.94),rgba(8,12,21,0.94))]",
            accentStyles[accent],
          )}
        >
          <CardHeader className="relative">
            <div className="absolute inset-x-0 top-0 h-28 bg-[radial-gradient(circle_at_top,rgba(124,99,255,0.24),transparent_65%)]" />
            <div className="relative flex items-start justify-between gap-4">
              <div className="space-y-3">
                <div className="flex size-14 items-center justify-center rounded-[1.4rem] border border-white/8 bg-white/[0.05] text-white">
                  <Icon className="size-6" />
                </div>
                <div>
                  <CardTitle>{title}</CardTitle>
                  <CardDescription>
                    This route is intentionally scaffolded in PHASE 1 and held back
                    from full implementation until its delivery phase.
                  </CardDescription>
                </div>
              </div>
              <Badge variant={accent} size="md">
                {phase}
              </Badge>
            </div>
          </CardHeader>
          <CardContent className="grid gap-4 md:grid-cols-2">
            {deliverables.map((item) => (
              <div
                key={item}
                className="rounded-[1.25rem] border border-surface-border bg-white/[0.03] px-4 py-4 text-sm text-slate-200"
              >
                {item}
              </div>
            ))}
          </CardContent>
        </Card>

        <Card>
          <CardHeader>
            <CardTitle>Foundation already in place</CardTitle>
            <CardDescription>
              These shared building blocks are available for the module work that
              follows.
            </CardDescription>
          </CardHeader>
          <CardContent className="space-y-3">
            {foundationItems.map((item) => (
              <div
                key={item}
                className="flex items-center justify-between rounded-[1.1rem] border border-surface-border bg-white/[0.02] px-4 py-3"
              >
                <span className="text-sm text-slate-200">{item}</span>
                <Badge variant="neutral" size="sm">
                  Ready
                </Badge>
              </div>
            ))}
          </CardContent>
        </Card>
      </div>
    </div>
  );
}

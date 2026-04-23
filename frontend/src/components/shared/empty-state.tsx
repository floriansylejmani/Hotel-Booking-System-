import type { LucideIcon } from "lucide-react";
import { Card, CardContent } from "@/components/ui/card";

type EmptyStateProps = {
  icon: LucideIcon;
  title: string;
  description: string;
};

export function EmptyState({ description, icon: Icon, title }: EmptyStateProps) {
  return (
    <Card className="border-dashed">
      <CardContent className="flex min-h-72 flex-col items-center justify-center text-center">
        <div className="flex size-20 items-center justify-center rounded-full border border-surface-border bg-[linear-gradient(135deg,rgba(91,124,255,0.18),rgba(124,99,255,0.12))] text-accent-cyan">
          <Icon className="size-8" />
        </div>
        <h2 className="mt-6 text-2xl font-bold text-white">{title}</h2>
        <p className="mt-3 max-w-md text-sm leading-6 text-muted-foreground">
          {description}
        </p>
      </CardContent>
    </Card>
  );
}

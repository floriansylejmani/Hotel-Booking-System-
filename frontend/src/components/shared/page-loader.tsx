export function PageLoader({ label = "Loading workspace..." }: { label?: string }) {
  return (
    <div className="flex min-h-[60vh] items-center justify-center">
      <div className="glass-surface rounded-[1.8rem] px-8 py-6 text-center">
        <div className="mx-auto h-10 w-10 animate-spin rounded-full border-2 border-accent-indigo/30 border-t-accent-indigo" />
        <p className="mt-4 text-sm text-muted-foreground">{label}</p>
      </div>
    </div>
  );
}

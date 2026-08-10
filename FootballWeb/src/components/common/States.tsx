import { ApiError, NotFoundError } from "../../api/client";

export function LoadingState({ label = "Loading..." }: { label?: string }) {
  return (
    <div className="flex items-center justify-center py-16 text-ink-muted">
      <span className="mr-2 inline-block h-4 w-4 animate-spin rounded-full border-2 border-brand-200 border-t-brand-500" />
      {label}
    </div>
  );
}

export function EmptyState({
  title,
  description,
}: {
  title: string;
  description?: string;
}) {
  return (
    <div className="rounded-lg border border-dashed border-surface-border bg-surface px-6 py-12 text-center">
      <p className="font-medium text-ink">{title}</p>
      {description && (
        <p className="mt-1 text-sm text-ink-muted">{description}</p>
      )}
    </div>
  );
}

export function ErrorState({ error }: { error: unknown }) {
  if (error instanceof NotFoundError) {
    return (
      <EmptyState
        title="Not found"
        description="This item doesn't exist or was removed."
      />
    );
  }

  const message =
    error instanceof ApiError
      ? error.message
      : error instanceof Error
        ? error.message
        : "Something went wrong.";

  return (
    <div className="rounded-lg border border-danger/30 bg-danger/5 px-6 py-8 text-center">
      <p className="font-medium text-danger">Couldn't load this data</p>
      <p className="mt-1 text-sm text-ink-muted">{message}</p>
    </div>
  );
}

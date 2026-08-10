interface PaginationProps {
  page: number;
  pageSize: number;
  totalCount: number;
  onPageChange: (page: number) => void;
}

export function Pagination({
  page,
  pageSize,
  totalCount,
  onPageChange,
}: PaginationProps) {
  const totalPages = Math.max(1, Math.ceil(totalCount / pageSize));

  if (totalPages <= 1) return null;

  return (
    <div className="flex items-center justify-between border-t border-surface-border pt-3 text-sm">
      <span className="text-ink-muted">
        Page {page} of {totalPages} &middot; {totalCount} total
      </span>
      <div className="flex gap-2">
        <button
          type="button"
          className="rounded-md border border-surface-border px-3 py-1.5 disabled:cursor-not-allowed disabled:opacity-40 hover:border-brand-500 hover:text-brand-500"
          disabled={page <= 1}
          onClick={() => onPageChange(page - 1)}
        >
          Previous
        </button>
        <button
          type="button"
          className="rounded-md border border-surface-border px-3 py-1.5 disabled:cursor-not-allowed disabled:opacity-40 hover:border-brand-500 hover:text-brand-500"
          disabled={page >= totalPages}
          onClick={() => onPageChange(page + 1)}
        >
          Next
        </button>
      </div>
    </div>
  );
}

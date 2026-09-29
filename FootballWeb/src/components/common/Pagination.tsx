import type { PaginationMetadata } from "../../types/transfer";

interface PaginationProps {
  pagination: PaginationMetadata;
  onPageChange: (page: number) => void;
}

export function Pagination({ pagination, onPageChange }: PaginationProps) {
  const { page, totalItems, totalPages } = pagination;

  if (totalPages <= 1) return null;

  return (
    <div className="flex items-center justify-between border-t border-surface-border pt-3 text-sm">
      <span className="pl-3 pb-3 text-ink-muted">
        Page {page} of {totalPages} &middot; {totalItems} total
      </span>
      <div className="flex gap-2 pb-3 pr-3">
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

import { useState } from "react";
import { useNavigate } from "react-router-dom";
import { usePlayerSearch } from "../features/players/hooks";
import {
  LoadingState,
  ErrorState,
  EmptyState,
} from "../components/common/States";
import type { PlayerSummaryDTO } from "../types/player";
import { formatMarketValue } from "../lib/format";

const PAGE_SIZE = 20;

// Matches TransferService.MaptoStatus output ("current" | "loaned" | "transferred").
// TODO(backend): PlayerSearchQuery has no transferStatus param yet, so this
// filter is applied client-side against the current page only.
type TransferStatusFilter = "all" | "current" | "loaned" | "transferred";

const TRANSFER_FILTER_OPTIONS: {
  label: string;
  value: TransferStatusFilter;
}[] = [
  { label: "All", value: "all" },
  { label: "Current", value: "current" },
  { label: "Loaned", value: "loaned" },
  { label: "Transferred", value: "transferred" },
];

function initials(name: string): string {
  const parts = name.trim().split(/\s+/);
  if (parts.length === 1) return parts[0].slice(0, 2).toUpperCase();
  return (parts[0][0] + parts[parts.length - 1][0]).toUpperCase();
}

export function PlayerListPage() {
  const navigate = useNavigate();
  const [search, setSearch] = useState("");
  // TODO(backend): mockup's Position filter is a broad category select
  // (Goalkeeper/Defender/Midfielder/Forward), but PlayerSearchQuery only
  // supports an exact positionCode (e.g. "LW", "CB"). Kept as a free-text
  // code input until the backend exposes a category-level filter.
  const [positionCode, setPositionCode] = useState("");
  const [nationality, setNationality] = useState("");
  const [transferStatus, setTransferStatus] =
    useState<TransferStatusFilter>("all");
  const [page, setPage] = useState(1);

  const { data, isLoading, error } = usePlayerSearch({
    search: search || undefined,
    positionCode: positionCode || undefined,
    nationality: nationality || undefined,
    page,
    pageSize: PAGE_SIZE,
  });

  const items =
    data && transferStatus !== "all"
      ? data.items.filter((p) => p.transferStatus === transferStatus)
      : (data?.items ?? []);

  return (
    <div className="mx-auto max-w-[1600px]">
      {/* Page header */}
      <div className="mb-gutter flex items-end justify-between">
        <div>
          <h1 className="font-display-lg text-on-surface">Players</h1>
          <p className="mt-2 font-body-md text-on-surface-variant">
            Squad overview and player information
          </p>
        </div>
      </div>

      {/* Toolbar */}
      <div className="mb-gutter flex flex-wrap items-center justify-between gap-gutter rounded-lg border border-outline-variant bg-surface-container p-unit">
        <div className="flex min-w-[300px] flex-1 items-center gap-gutter">
          <div className="relative w-full max-w-sm">
            <span className="material-symbols-outlined absolute left-3 top-1/2 -translate-y-1/2 text-outline-variant">
              search
            </span>
            <input
              value={search}
              onChange={(e) => {
                setSearch(e.target.value);
                setPage(1);
              }}
              placeholder="Search players..."
              className="w-full rounded border border-outline-variant bg-surface-container-low py-2 pl-10 pr-4 font-body-sm text-on-surface placeholder:text-outline-variant focus:border-primary focus:ring-0"
            />
          </div>
        </div>

        <div className="flex items-center gap-gutter">
          <FilterField
            label="POSITION"
            value={positionCode}
            onChange={(v) => {
              setPositionCode(v);
              setPage(1);
            }}
            placeholder="e.g. LW"
          />
          <FilterField
            label="NAT"
            value={nationality}
            onChange={(v) => {
              setNationality(v);
              setPage(1);
            }}
            placeholder="e.g. ENG"
          />
          <div className="flex items-center gap-2 rounded border border-outline-variant bg-surface-container-low p-1">
            <span className="px-2 font-label-caps uppercase text-outline-variant">
              TRANSFER
            </span>
            <div className="relative">
              <select
                value={transferStatus}
                onChange={(e) =>
                  setTransferStatus(e.target.value as TransferStatusFilter)
                }
                className="cursor-pointer appearance-none border-none bg-transparent py-1 pl-2 pr-8 font-body-sm text-on-surface focus:ring-0"
              >
                {TRANSFER_FILTER_OPTIONS.map((opt) => (
                  <option
                    key={opt.value}
                    value={opt.value}
                    className="bg-surface-container-low text-on-surface"
                  >
                    {opt.label}
                  </option>
                ))}
              </select>
              <span className="material-symbols-outlined pointer-events-none absolute right-1 top-1/2 -translate-y-1/2 text-[16px] text-outline-variant">
                expand_more
              </span>
            </div>
          </div>
        </div>
      </div>

      {isLoading && <LoadingState label="Searching players..." />}
      {error && <ErrorState error={error} />}

      {data && items.length === 0 && (
        <EmptyState
          title="No players found"
          description="Try a different search or clear the filters."
        />
      )}

      {data && items.length > 0 && (
        <div className="flex flex-col overflow-hidden rounded-lg border border-outline-variant bg-surface-container shadow-[0_0_0_1px_rgba(71,85,105,0.1)]">
          <div className="overflow-x-auto">
            <table className="w-full border-collapse whitespace-nowrap text-left">
              <thead>
                <tr className="border-b border-outline-variant bg-surface-container-high">
                  <th className="w-16 px-4 py-3 text-right font-label-caps uppercase text-on-surface-variant">
                    #
                  </th>
                  <th className="px-4 py-3 font-label-caps uppercase text-on-surface-variant">
                    Player
                  </th>
                  <th className="w-24 px-4 py-3 font-label-caps uppercase text-on-surface-variant">
                    Pos
                  </th>
                  <th className="w-32 px-4 py-3 font-label-caps uppercase text-on-surface-variant">
                    Nat
                  </th>
                  <th className="w-24 px-4 py-3 text-right font-label-caps uppercase text-on-surface-variant">
                    Age
                  </th>
                  <th className="w-32 px-4 py-3 font-label-caps uppercase text-on-surface-variant">
                    Status
                  </th>
                  <th className="w-32 px-4 py-3 text-right font-label-caps uppercase text-on-surface-variant">
                    Value
                  </th>
                  <th className="w-32 px-4 py-3 font-label-caps uppercase text-on-surface-variant">
                    Transfer
                  </th>
                  <th className="w-24 px-4 py-3 text-right font-label-caps uppercase text-on-surface-variant">
                    Contract
                  </th>
                </tr>
              </thead>
              <tbody className="font-body-sm">
                {items.map((p) => (
                  <PlayerRow
                    key={p.playerId}
                    player={p}
                    onClick={() => navigate(`/players/${p.playerId}`)}
                  />
                ))}
              </tbody>
            </table>
          </div>

          <PaginationBar
            page={data.page}
            pageSize={data.pageSize}
            totalCount={data.totalCount}
            onPageChange={setPage}
          />
        </div>
      )}
    </div>
  );
}

function FilterField({
  label,
  value,
  onChange,
  placeholder,
}: {
  label: string;
  value: string;
  onChange: (v: string) => void;
  placeholder?: string;
}) {
  return (
    <div className="flex items-center gap-2 rounded border border-outline-variant bg-surface-container-low p-1">
      <span className="px-2 font-label-caps uppercase text-outline-variant">
        {label}
      </span>
      <input
        value={value}
        onChange={(e) => onChange(e.target.value)}
        placeholder={placeholder}
        className="w-24 border-none bg-transparent py-1 pl-2 pr-2 font-body-sm text-on-surface placeholder:text-outline-variant focus:ring-0"
      />
    </div>
  );
}

function TransferBadge({
  status,
}: {
  status: PlayerSummaryDTO["transferStatus"];
}) {
  if (status === "current") {
    return (
      <span className="inline-flex items-center rounded bg-primary/10 border border-primary/30 px-2 py-0.5 text-[10px] font-bold uppercase tracking-wider text-primary">
        Current
      </span>
    );
  }
  if (status === "loaned") {
    return (
      <span className="inline-flex items-center rounded bg-secondary-container/30 border border-outline-variant px-2 py-0.5 text-[10px] font-bold uppercase tracking-wider text-on-secondary-container">
        Loaned
      </span>
    );
  }
  if (status === "transferred") {
    return (
      <span className="inline-flex items-center rounded bg-error-container/20 border border-error/30 px-2 py-0.5 text-[10px] font-bold uppercase tracking-wider text-error">
        Transferred
      </span>
    );
  }
  return <span className="text-on-surface-variant">-</span>;
}

function PlayerRow({
  player,
  onClick,
}: {
  player: PlayerSummaryDTO;
  onClick: () => void;
}) {
  return (
    <tr
      onClick={onClick}
      className="group relative h-row-height-compact cursor-pointer border-b border-outline-variant/30 transition-colors hover:bg-primary-container/10"
    >
      <td className="relative px-4 py-2 text-right font-data-mono text-on-surface-variant">
        <div className="absolute bottom-0 left-0 top-0 w-[2px] bg-primary opacity-0 transition-opacity group-hover:opacity-100" />
        {player.shirtNumber ?? "-"}
      </td>
      <td className="px-4 py-2">
        <div className="flex items-center gap-3">
          <div className="flex h-6 w-6 items-center justify-center rounded-full border border-outline-variant bg-surface-container-high font-data-mono text-[10px] text-on-surface">
            {initials(player.name)}
          </div>
          <span className="font-semibold text-on-surface">{player.name}</span>
        </div>
      </td>
      <td className="px-4 py-2 font-data-mono text-on-surface-variant">
        {player.positionCode ?? "-"}
      </td>
      <td className="px-4 py-2 text-on-surface-variant">
        {player.nationality ?? "-"}
      </td>
      <td className="px-4 py-2 text-right font-data-mono text-on-surface-variant">
        {player.age != null ? player.age : "-"}
      </td>
      <td className="px-4 py-2 text-on-surface-variant">
        {player.status ?? "-"}
      </td>
      <td className="px-4 py-2 text-right text-on-surface-variant">
        {player.marketValue != null
          ? formatMarketValue(player.marketValue)
          : "-"}
      </td>
      <td className="px-4 py-2">
        <TransferBadge status={player.transferStatus} />
      </td>
      <td className="px-4 py-2 text-right text-on-surface-variant">
        {player.contractUntil ?? "-"}
      </td>
    </tr>
  );
}

function PaginationBar({
  page,
  pageSize,
  totalCount,
  onPageChange,
}: {
  page: number;
  pageSize: number;
  totalCount: number;
  onPageChange: (page: number) => void;
}) {
  const totalPages = Math.max(1, Math.ceil(totalCount / pageSize));
  const from = totalCount === 0 ? 0 : (page - 1) * pageSize + 1;
  const to = Math.min(page * pageSize, totalCount);

  const pageNumbers = buildPageList(page, totalPages);

  return (
    <div className="flex items-center justify-between border-t border-outline-variant bg-surface-container-high p-3">
      <span className="font-body-sm text-on-surface-variant">
        Showing {from} to {to} of {totalCount} players
      </span>
      <div className="flex items-center gap-1">
        <button
          type="button"
          disabled={page <= 1}
          onClick={() => onPageChange(page - 1)}
          className="p-1 text-on-surface-variant transition-colors hover:text-on-surface disabled:opacity-50"
        >
          <span className="material-symbols-outlined">chevron_left</span>
        </button>

        {pageNumbers.map((n, i) =>
          n === "..." ? (
            <span
              key={`ellipsis-${i}`}
              className="px-2 font-data-mono text-on-surface-variant"
            >
              ...
            </span>
          ) : (
            <button
              key={n}
              type="button"
              onClick={() => onPageChange(n)}
              className={`flex h-8 w-8 items-center justify-center rounded font-data-mono transition-colors ${
                n === page
                  ? "bg-primary-container text-on-primary-container"
                  : "text-on-surface-variant hover:bg-surface-variant"
              }`}
            >
              {n}
            </button>
          ),
        )}

        <button
          type="button"
          disabled={page >= totalPages}
          onClick={() => onPageChange(page + 1)}
          className="flex h-8 items-center justify-center gap-1 rounded px-3 font-body-sm text-on-surface-variant transition-colors hover:bg-surface-variant hover:text-on-surface disabled:opacity-50"
        >
          Next
          <span className="material-symbols-outlined text-[18px]">
            chevron_right
          </span>
        </button>
      </div>
    </div>
  );
}

// Builds a compact page list like [1, "...", 4, 5, 6, "...", 10] centered
// around the current page, always keeping first/last visible.
function buildPageList(current: number, total: number): (number | "...")[] {
  if (total <= 7) {
    return Array.from({ length: total }, (_, i) => i + 1);
  }

  const pages = new Set<number>([1, total, current, current - 1, current + 1]);
  const sorted = [...pages]
    .filter((p) => p >= 1 && p <= total)
    .sort((a, b) => a - b);

  const result: (number | "...")[] = [];
  for (let i = 0; i < sorted.length; i++) {
    if (i > 0 && sorted[i] - sorted[i - 1] > 1) {
      result.push("...");
    }
    result.push(sorted[i]);
  }
  return result;
}

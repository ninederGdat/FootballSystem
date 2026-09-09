import { useMemo, useState } from "react";
import { useTransfers } from "../features/transfers/hooks";
import type { TransferApiItem, TransferDirection } from "../types/transfer";
import {
  LoadingState,
  ErrorState,
  EmptyState,
} from "../components/common/States";
import { Pagination } from "../components/common/Pagination";
import { formatMatchDate, formatMarketValue } from "../lib/format";

// TODO(backend): centralize this — currently duplicated wherever the app
// needs to know "which side is us" (e.g. match home/away logic).
const CHELSEA_TEAM_ID = 8455;

const DIRECTION_TABS: { label: string; value: TransferDirection | "all" }[] = [
  { label: "All", value: "all" },
  { label: "Incoming", value: "in" },
  { label: "Outgoing", value: "out" },
];

// Backend transferType enum is "contract" | "on_loan" — there is no "free"
// transfer type. "Free transfer" is a contract transfer with fee === null.
const TYPE_FILTER_OPTIONS: {
  label: string;
  value: "all" | "contract" | "on_loan";
}[] = [
  { label: "All Types", value: "all" },
  { label: "Permanent", value: "contract" },
  { label: "Loan", value: "on_loan" },
];

function getDirection(transfer: TransferApiItem): TransferDirection {
  return transfer.toClubId === CHELSEA_TEAM_ID ? "in" : "out";
}

function initials(name: string): string {
  return name
    .split(" ")
    .filter(Boolean)
    .map((part) => part[0])
    .join("")
    .slice(0, 2)
    .toUpperCase();
}

function TypeBadge({ transfer }: { transfer: TransferApiItem }) {
  if (transfer.onLoan) {
    return (
      <span className="px-2 py-0.5 rounded text-[11px] font-medium bg-surface-container-highest text-on-surface-variant border border-outline-variant/50">
        Loan
      </span>
    );
  }
  if (transfer.fee == null) {
    return (
      <span className="px-2 py-0.5 rounded text-[11px] font-medium bg-surface-variant text-on-surface-variant">
        Free Transfer
      </span>
    );
  }
  return (
    <span className="px-2 py-0.5 rounded text-[11px] font-medium bg-surface-variant text-on-surface-variant">
      Transfer
    </span>
  );
}

function TransferRow({ transfer }: { transfer: TransferApiItem }) {
  const direction = getDirection(transfer);
  const isIncoming = direction === "in";

  return (
    <tr className="h-row-height-compact border-b border-outline-variant/30 even:bg-surface-container-low/20 hover:bg-primary-container/10 transition-colors group relative cursor-pointer">
      <td className="px-4 relative text-center">
        <div className="absolute left-0 top-0 bottom-0 w-0.5 bg-primary opacity-0 group-hover:opacity-100 transition-opacity" />
        <span
          className={`material-symbols-outlined text-[16px] ${isIncoming ? "text-primary" : "text-tertiary"}`}
        >
          {isIncoming ? "south_east" : "north_east"}
        </span>
      </td>
      <td className="px-4">
        <div className="flex items-center gap-3">
          <div className="w-6 h-6 rounded-full bg-surface-variant flex items-center justify-center font-data-mono text-[10px] text-on-surface">
            {initials(transfer.playerName)}
          </div>
          <span className="font-medium">{transfer.playerName}</span>
        </div>
      </td>
      <td
        className={`px-4 ${isIncoming ? "text-on-surface-variant" : "font-medium"}`}
      >
        {transfer.fromClubName}
      </td>
      <td
        className={`px-4 ${isIncoming ? "font-medium" : "text-on-surface-variant"}`}
      >
        {transfer.toClubName}
      </td>
      <td className="px-4 font-data-mono text-data-mono text-on-surface-variant">
        {formatMatchDate(transfer.transferDate)}
      </td>
      <td className="px-4">
        <TypeBadge transfer={transfer} />
      </td>
      <td className="px-4 text-right font-data-mono text-data-mono">
        {transfer.fee != null ? (
          formatMarketValue(transfer.fee)
        ) : (
          <span className="text-outline">{transfer.onLoan ? "—" : "Free"}</span>
        )}
      </td>
    </tr>
  );
}

export default function TransferPage() {
  const [direction, setDirection] = useState<TransferDirection | "all">("all");
  const [transferType, setTransferType] = useState<
    "all" | "contract" | "on_loan"
  >("all");
  const [page, setPage] = useState(1);
  const pageSize = 20;

  // Direction has no backend param (derived client-side from toClubId), so it
  // isn't sent to the API. transferType maps directly to TransferQuery.TransferType.
  const { data, isLoading, isError, error, refetch } = useTransfers({
    page,
    pageSize,
    transferType: transferType === "all" ? undefined : transferType,
  });

  const filtered = useMemo(() => {
    const items = data?.data ?? [];
    if (direction === "all") return items;
    return items.filter((t) => getDirection(t) === direction);
  }, [data, direction]);

  const pagination = data?.pagination;

  return (
    <main className="mt-[5px] pd-[5px] flex-1 overflow-y-auto p-container-padding">
      {/* Page Header */}
      <div className="mb-8 flex justify-between items-end">
        <div>
          <h2 className="font-display-lg text-display-lg text-on-surface mb-1">
            Transfers
          </h2>
          <p className="font-body-md text-body-md text-on-surface-variant">
            Chelsea transfer activity and transfer history
          </p>
        </div>
        <div className="flex items-center gap-2">
          <button className="px-4 py-2 bg-surface-container-low border border-outline-variant rounded hover:bg-surface-container-high transition-colors text-body-sm font-body-sm flex items-center gap-2">
            <span className="material-symbols-outlined text-[16px]">
              download
            </span>
            Export CSV
          </button>
        </div>
      </div>

      {/* Filters Section */}
      <div className="flex items-center justify-between border-b border-outline-variant pb-4 mb-4">
        <div className="flex bg-surface-container-low p-1 rounded-lg border border-outline-variant">
          {DIRECTION_TABS.map((tab) => (
            <button
              key={tab.value}
              onClick={() => setDirection(tab.value)}
              className={
                direction === tab.value
                  ? "px-4 py-1.5 rounded bg-surface border border-outline-variant shadow-sm text-on-surface font-body-sm text-body-sm font-medium"
                  : "px-4 py-1.5 rounded text-on-surface-variant hover:text-on-surface font-body-sm text-body-sm transition-colors"
              }
            >
              {tab.label}
            </button>
          ))}
        </div>

        <div className="flex items-center gap-3">
          {/* TODO(backend): TransferQuery has no season param, only dateFrom/dateTo.
             Dropped the season select until there's a seasons lookup or we
             decide to map "season" to a dateFrom/dateTo range client-side. */}
          <div className="relative">
            <select
              value={transferType}
              onChange={(e) =>
                setTransferType(e.target.value as typeof transferType)
              }
              className="appearance-none bg-surface-container-low border border-outline-variant rounded pl-3 pr-8 py-1.5 text-body-sm font-body-sm text-on-surface focus:outline-none focus:border-primary transition-colors cursor-pointer"
            >
              {TYPE_FILTER_OPTIONS.map((opt) => (
                <option key={opt.value} value={opt.value}>
                  {opt.label}
                </option>
              ))}
            </select>
            <span className="material-symbols-outlined absolute right-2 top-1/2 -translate-y-1/2 text-outline pointer-events-none text-[16px]">
              expand_more
            </span>
          </div>
        </div>
      </div>

      {/* Data Table Container */}
      <div className="border border-outline-variant rounded-lg bg-surface-container-lowest overflow-hidden">
        {isLoading ? (
          <LoadingState />
        ) : isError ? (
          <ErrorState error={error ?? "Failed to load transfers."} />
        ) : filtered.length === 0 ? (
          <EmptyState
            title="No transfers found"
            description="No transfers found for the selected filters."
          />
        ) : (
          <>
            <table className="w-full text-left border-collapse whitespace-nowrap">
              <thead>
                <tr className="font-label-caps text-label-caps text-on-surface-variant border-b border-outline-variant bg-surface-container-low">
                  <th className="px-4 py-3 font-semibold w-12 text-center">
                    Dir
                  </th>
                  <th className="px-4 py-3 font-semibold">Player</th>
                  <th className="px-4 py-3 font-semibold">From</th>
                  <th className="px-4 py-3 font-semibold">To</th>
                  <th className="px-4 py-3 font-semibold">Date</th>
                  <th className="px-4 py-3 font-semibold">Type</th>
                  <th className="px-4 py-3 font-semibold text-right">Fee</th>
                </tr>
              </thead>
              <tbody className="font-body-sm text-body-sm text-on-surface">
                {filtered.map((transfer) => (
                  <TransferRow
                    key={`${transfer.playerId}-${transfer.transferDate}-${transfer.toClubId}`}
                    transfer={transfer}
                  />
                ))}
              </tbody>
            </table>

            {pagination && (
              <Pagination
                page={pagination.page}
                pageSize={pagination.pageSize}
                totalCount={pagination.totalItems}
                onPageChange={setPage}
              />
            )}
          </>
        )}
      </div>
    </main>
  );
}

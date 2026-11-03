import { useEffect, useMemo, useState } from "react";
import {
  useTransferStatistics,
  useTransfers,
} from "../features/transfers/hooks";
import { useSeasons } from "../features/seasons/hooks";
import type {
  TransferApiItem,
  TransferDirection,
  TransferQueryParams,
} from "../types/transfer";
import {
  LoadingState,
  ErrorState,
  EmptyState,
} from "../components/common/States";
import { Pagination } from "../components/common/Pagination";
import { formatMatchDate, formatMarketValue } from "../lib/format";
import { useContextTeam } from "../features/team-context/TeamContext";
import { getTrackedTeam, TRACKED_TEAMS } from "../lib/teams";

type Mode = "club" | "all" | "internal";
type InternalDirection = "all" | "ab" | "ba";

const PAGE_SIZE = 20;
const CLUB_A = TRACKED_TEAMS[0];
const CLUB_B = TRACKED_TEAMS[1];

const DIRECTION_TABS: { label: string; value: "all" | TransferDirection }[] = [
  { label: "All", value: "all" },
  { label: "Incoming", value: "in" },
  { label: "Outgoing", value: "out" },
];

// Backend transferType enum is "contract" | "on_loan" — "Free transfer" is a
// contract transfer with fee === null, not a filter value.
const TYPE_FILTER_OPTIONS = [
  { label: "All Types", value: "all" },
  { label: "Permanent", value: "contract" },
  { label: "Loan", value: "on_loan" },
] as const;

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
  if (transfer.contractExtension || transfer.fromClubId === transfer.toClubId) {
    return (
      <span className="px-2 py-0.5 rounded text-[11px] font-medium bg-surface-container-highest text-on-surface-variant border border-outline-variant/50">
        EXTENSION
      </span>
    );
  }
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

function InternalTag() {
  return (
    <span className="text-[10px] font-data-mono px-1.5 py-0.5 rounded bg-surface-container-high text-on-surface-variant">
      INTERNAL
    </span>
  );
}

function MovementChip({ transfer }: { transfer: TransferApiItem }) {
  const from = getTrackedTeam(transfer.fromClubId);
  const to = getTrackedTeam(transfer.toClubId);
  return (
    <div className="inline-flex items-center gap-1.5 px-2 py-0.5 rounded border border-outline-variant bg-surface">
      <span className="text-[10px] font-bold font-data-mono px-1 py-0.2 rounded bg-primary-container text-on-primary-container">
        {from?.abbr ?? "—"}
      </span>
      <span className="text-outline text-[11px]">➔</span>
      <span className="text-[10px] font-bold font-data-mono px-1 py-0.2 rounded bg-surface-bright text-on-surface border border-outline-variant/50">
        {to?.abbr ?? "—"}
      </span>
    </div>
  );
}

function TransferRow({
  transfer,
  showDir,
  showMovement,
  showInternalTag,
  contextTeamId,
}: {
  transfer: TransferApiItem;
  showDir: boolean;
  showMovement: boolean;
  showInternalTag: boolean;
  contextTeamId: number;
}) {
  const isIncoming = transfer.toClubId === contextTeamId;
  const isExtensionRow =
    transfer.contractExtension || transfer.fromClubId === transfer.toClubId;
  const isInternalMove =
    transfer.fromClubId !== transfer.toClubId &&
    getTrackedTeam(transfer.fromClubId) !== undefined &&
    getTrackedTeam(transfer.toClubId) !== undefined;
  const fromCellClass = showDir
    ? isIncoming
      ? "px-4 text-on-surface-variant"
      : "px-4 font-medium"
    : "px-4 text-on-surface";
  const toCellClass = showDir
    ? isIncoming
      ? "px-4 font-medium"
      : "px-4 text-on-surface-variant"
    : "px-4 text-on-surface";

  return (
    <tr className="h-row-height-compact border-b border-outline-variant/30 even:bg-surface-container-low/20 hover:bg-primary-container/10 transition-colors group relative cursor-pointer">
      {showDir && (
        <td className="px-4 relative text-center">
          <div className="absolute left-0 top-0 bottom-0 w-0.5 bg-primary opacity-0 group-hover:opacity-100 transition-opacity" />
          <span
            className={`material-symbols-outlined text-[16px] ${isIncoming ? "text-primary" : "text-tertiary"}`}
          >
            {isIncoming ? "south_east" : "north_east"}
          </span>
        </td>
      )}
      {showMovement && (
        <td className="px-4 text-center">
          <MovementChip transfer={transfer} />
        </td>
      )}
      <td className="px-4">
        <div className="flex items-center gap-3">
          <div className="w-6 h-6 rounded-full bg-surface-variant flex items-center justify-center font-data-mono text-[10px] text-on-surface">
            {initials(transfer.playerName)}
          </div>
          <span className="font-medium">{transfer.playerName}</span>
        </div>
      </td>
      <td className={fromCellClass}>{transfer.fromClubName}</td>
      <td className={toCellClass}>{transfer.toClubName}</td>
      <td className="px-4 font-data-mono text-data-mono text-on-surface-variant">
        {formatMatchDate(transfer.transferDate)}
      </td>
      <td className="px-4">
        <div className="flex items-center gap-1.5">
          <TypeBadge transfer={transfer} />
          {showInternalTag && !isExtensionRow && isInternalMove && (
            <InternalTag />
          )}
        </div>
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

type InternalKpiState = ReturnType<typeof useTransferStatistics>;

function InternalKpiRow({ kpi }: { kpi: InternalKpiState }) {
  const s = kpi.data;
  const seasonCaption = s ? s.season.replace("-", "/") : "";
  const cards: { label: string; value: string; detail: string }[] = [
    {
      label: "Internal movements",
      value: s
        ? String(
            s.permanentBuyCount +
              s.permanentSellCount +
              s.loanInCount +
              s.loanOutCount,
          )
        : "…",
      detail: seasonCaption || "Loading...",
    },
    {
      label: `Loans to ${CLUB_B.teamName}`,
      value: s ? String(s.loanOutCount) : "…",
      detail: seasonCaption || "Loading...",
    },
    {
      label: `Loans to ${CLUB_A.teamName}`,
      value: s ? String(s.loanInCount) : "…",
      detail: seasonCaption || "Loading...",
    },
    {
      label: "Permanent moves",
      value: s ? String(s.permanentBuyCount + s.permanentSellCount) : "…",
      detail: s
        ? `${s.permanentSellCount} to ${CLUB_B.teamName} · ${s.permanentBuyCount} to ${CLUB_A.teamName} · ${seasonCaption}`
        : "Loading...",
    },
  ];
  const cardClass =
    "flex min-h-32 flex-col justify-between border border-outline-variant/50 bg-surface-container-low p-5 shadow-sm";

  if (kpi.isError) {
    return (
      <div className="grid grid-cols-1 gap-4 sm:grid-cols-2 xl:grid-cols-4 mb-4">
        <div className={`${cardClass} sm:col-span-2 xl:col-span-4`}>
          <p className="font-body-sm text-body-sm text-error">
            Failed to load transfer statistics.
          </p>
        </div>
      </div>
    );
  }

  return (
    <div className="grid grid-cols-1 gap-4 sm:grid-cols-2 xl:grid-cols-4 mb-4">
      {cards.map((card) => (
        <section key={card.label} className={cardClass}>
          <h2 className="font-label-caps text-label-caps uppercase text-on-surface-variant">
            {card.label}
          </h2>
          <div>
            <p className="font-data-mono text-3xl font-semibold text-on-surface">
              {card.value}
            </p>
            <p className="mt-2 text-xs text-on-surface-variant">{card.detail}</p>
          </div>
        </section>
      ))}
    </div>
  );
}

export default function TransferPage() {
  const { teamId: contextTeamId, team } = useContextTeam();

  const [mode, setMode] = useState<Mode>("club");
  const [direction, setDirection] = useState<"all" | TransferDirection>("all");
  const [internalDirection, setInternalDirection] =
    useState<InternalDirection>("all");
  const [transferType, setTransferType] = useState<
    "all" | "contract" | "on_loan"
  >("all");
  const [season, setSeason] = useState("all");
  const [page, setPage] = useState(1);
  const [hideExtensions, setHideExtensions] = useState(false);

  // "Hide extensions" defaults per mode: ON in all-clubs / internal modes,
  // OFF for the single-club view (re-applied whenever the mode changes).
  useEffect(() => {
    setHideExtensions(mode !== "club");
  }, [mode]);

  const {
    data: seasonsData,
    isLoading: seasonsLoading,
    isError: seasonsError,
  } = useSeasons();
  const seasonOptions = [
    { code: "all", label: "All Seasons" },
    ...(seasonsData?.data.map((item) => ({
      code: item.code,
      label: item.name,
    })) ?? []),
  ];

  useEffect(() => {
    setPage(1);
  }, [contextTeamId, mode, direction, internalDirection, transferType, season, hideExtensions]);

  const query = useMemo((): TransferQueryParams => {
    const base: TransferQueryParams = {
      page,
      pageSize: PAGE_SIZE,
      season: season === "all" ? undefined : season,
      transferType: transferType === "all" ? undefined : transferType,
    };
    if (mode === "club") {
      return {
        ...base,
        teamId: contextTeamId,
        direction: direction === "all" ? undefined : direction,
      };
    }
    if (mode === "internal") {
      const internal: TransferQueryParams = { ...base, internalOnly: true };
      if (internalDirection === "ab") {
        return { ...internal, fromClubId: CLUB_A.teamId, toClubId: CLUB_B.teamId };
      }
      if (internalDirection === "ba") {
        return { ...internal, fromClubId: CLUB_B.teamId, toClubId: CLUB_A.teamId };
      }
      return internal;
    }
    return base;
  }, [page, season, transferType, mode, direction, internalDirection, contextTeamId]);

  const { data, isLoading, isError, error } = useTransfers(query);

  // KPI totals live in their own query so their loading/error state never
  // gates the table below.
  const kpi = useTransferStatistics({
    teamId: CLUB_A.teamId,
    season: season === "all" ? undefined : season,
    internalOnly: true,
  });

  const modeTabs: { value: Mode; label: string }[] = [
    { value: "club", label: team.teamName },
    { value: "all", label: "All clubs" },
    { value: "internal", label: `${CLUB_A.abbr} ↔ ${CLUB_B.abbr}` },
  ];
  const internalTabs: { value: InternalDirection; label: string }[] = [
    { value: "all", label: "All movements" },
    { value: "ab", label: `${CLUB_A.teamName} → ${CLUB_B.teamName}` },
    { value: "ba", label: `${CLUB_B.teamName} → ${CLUB_A.teamName}` },
  ];

  const subtitle =
    mode === "club"
      ? `Transfer activity for ${team.teamName}`
      : mode === "all"
        ? `Transfers of ${TRACKED_TEAMS.map((t) => t.teamName).join(" and ")}`
        : `Transfers between ${CLUB_A.teamName} and ${CLUB_B.teamName}`;

  function handleResetFilters() {
    setMode("club");
    setDirection("all");
    setInternalDirection("all");
    setTransferType("all");
    setSeason("all");
    setPage(1);
  }

  const items = (data?.data ?? []).filter(
    (transfer) =>
      !hideExtensions ||
      !(transfer.contractExtension || transfer.fromClubId === transfer.toClubId),
  );
  const pagination = data?.pagination;
  const showDir = mode === "club";
  const showMovement = mode === "internal";
  const showInternalTag = mode !== "internal";
  const showDirectionTabs = mode !== "all";

  return (
    <main className="mt-[44px] flex-1 overflow-y-auto p-container-padding">
      {/* Page Header */}
      <div className="mb-8 flex justify-between items-end">
        <div>
          <h2 className="font-display-lg text-display-lg text-on-surface mb-1">
            Transfers
          </h2>
          <p className="font-body-md text-body-md text-on-surface-variant">
            {subtitle}
          </p>
        </div>
      </div>

      {/* Mode control */}
      <div className="mb-6 flex items-center justify-between gap-3 pb-3 border-b border-outline-variant/40">
        <div className="flex items-center bg-surface-container-lowest p-1 rounded-lg border border-outline-variant w-fit">
          {modeTabs.map((tab) => (
            <button
              key={tab.value}
              type="button"
              onClick={() => setMode(tab.value)}
              className={
                mode === tab.value
                  ? "flex items-center gap-1.5 px-3 py-1.5 rounded-lg bg-primary-container text-on-primary-container font-body-sm text-body-sm font-semibold transition-all"
                  : "flex items-center gap-1.5 px-3 py-1.5 rounded-lg text-on-surface-variant hover:text-on-surface font-body-sm text-body-sm transition-all"
              }
            >
              {mode === tab.value && (
                <span className="w-2 h-2 rounded-full bg-primary" />
              )}
              {tab.label}
            </button>
          ))}
        </div>
      </div>

      {mode === "internal" && <InternalKpiRow kpi={kpi} />}

      {/* Filters Section */}
      <div
        className={`flex flex-wrap items-center ${showDirectionTabs ? "justify-between" : "justify-end"} border-b border-outline-variant pb-4 mb-4 gap-3`}
      >
        {/* Direction-tabs pill renders only when it has tabs (club/internal);
            in "all" mode the row stays justify-end with no empty bordered box. */}
        {(mode === "club" || mode === "internal") && (
          <div className="flex bg-surface-container-low p-1 rounded-lg border border-outline-variant">
          {mode === "club" &&
            DIRECTION_TABS.map((tab) => (
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
          {mode === "internal" &&
            internalTabs.map((tab) => (
              <button
                key={tab.value}
                onClick={() => setInternalDirection(tab.value)}
                className={
                  internalDirection === tab.value
                    ? "px-4 py-1.5 rounded bg-surface border border-outline-variant shadow-sm text-on-surface font-body-sm text-body-sm font-medium"
                    : "px-4 py-1.5 rounded text-on-surface-variant hover:text-on-surface font-body-sm text-body-sm transition-colors"
                }
              >
                {tab.label}
              </button>
            ))}
          </div>
        )}

        <div className="flex items-center gap-3">
          <button
            type="button"
            onClick={() => setHideExtensions((value) => !value)}
            aria-pressed={hideExtensions}
            className={`flex items-center gap-1.5 appearance-none rounded px-3 py-1.5 text-body-sm font-body-sm focus:outline-none focus:border-primary transition-colors cursor-pointer ${
              hideExtensions
                ? "bg-primary-container text-on-primary-container border border-outline-variant"
                : "bg-surface-container-low text-on-surface-variant border border-outline-variant"
            }`}
          >
            <span className="material-symbols-outlined text-[16px]">
              {hideExtensions ? "visibility_off" : "visibility"}
            </span>
            Hide extensions
          </button>
          <div className="relative">
            <select
              value={season}
              disabled={seasonsLoading || seasonsError}
              onChange={(e) => setSeason(e.target.value)}
              className="appearance-none bg-surface-container-low border border-outline-variant rounded pl-3 pr-8 py-1.5 text-body-sm font-body-sm text-on-surface focus:outline-none focus:border-primary transition-colors cursor-pointer"
            >
              {seasonOptions.map((opt) => (
                <option
                  key={opt.code}
                  value={opt.code}
                  className="bg-surface-container-low text-on-surface"
                >
                  {opt.label}
                </option>
              ))}
            </select>
            <span className="material-symbols-outlined absolute right-2 top-1/2 -translate-y-1/2 text-outline pointer-events-none text-[16px]">
              expand_more
            </span>
          </div>
          <div className="relative">
            <select
              value={transferType}
              onChange={(e) =>
                setTransferType(e.target.value as typeof transferType)
              }
              className="appearance-none bg-surface-container-low border border-outline-variant rounded pl-3 pr-8 py-1.5 text-body-sm font-body-sm text-on-surface focus:outline-none focus:border-primary transition-colors cursor-pointer"
            >
              {TYPE_FILTER_OPTIONS.map((opt) => (
                <option
                  key={opt.value}
                  value={opt.value}
                  className="bg-surface-container-low text-on-surface"
                >
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
          <ErrorState error={error} />
        ) : items.length === 0 ? (
          <div>
            <EmptyState
              title="No transfers found"
              description="No transfers found for the selected filters."
            />
            <div className="flex justify-center pb-4">
              <button
                type="button"
                onClick={handleResetFilters}
                className="px-4 py-2 rounded bg-primary-container text-on-primary-container font-body-sm text-body-sm font-semibold hover:bg-primary-container/90 transition-colors"
              >
                Reset filters
              </button>
            </div>
          </div>
        ) : (
          <>
            <table className="w-full text-left border-collapse whitespace-nowrap">
              <thead>
                <tr className="font-label-caps text-label-caps text-on-surface-variant border-b border-outline-variant bg-surface-container-low">
                  {showDir && (
                    <th className="px-4 py-3 font-semibold w-12 text-center">
                      Dir
                    </th>
                  )}
                  {showMovement && (
                    <th className="px-4 py-3 font-semibold text-center">
                      Movement
                    </th>
                  )}
                  <th className="px-4 py-3 font-semibold">Player</th>
                  <th className="px-4 py-3 font-semibold">From</th>
                  <th className="px-4 py-3 font-semibold">To</th>
                  <th className="px-4 py-3 font-semibold">Date</th>
                  <th className="px-4 py-3 font-semibold">Type</th>
                  <th className="px-4 py-3 font-semibold text-right">Fee</th>
                </tr>
              </thead>
              <tbody className="font-body-sm text-body-sm text-on-surface">
                {items.map((transfer, index) => (
                  <TransferRow
                    key={`${transfer.playerId}-${transfer.transferDate}-${transfer.fromClubId}-${transfer.toClubId}-${transfer.transferType}-${index}`}
                    transfer={transfer}
                    showDir={showDir}
                    showMovement={showMovement}
                    showInternalTag={showInternalTag}
                    contextTeamId={contextTeamId}
                  />
                ))}
              </tbody>
            </table>

            {pagination && (
              <Pagination pagination={pagination} onPageChange={setPage} />
            )}
          </>
        )}
      </div>
    </main>
  );
}

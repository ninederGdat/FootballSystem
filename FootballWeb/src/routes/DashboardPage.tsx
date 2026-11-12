import { useState, type ReactNode } from "react";
import type { UseQueryResult } from "@tanstack/react-query";
import { Link, useNavigate } from "react-router-dom";
import {
  EmptyState,
  ErrorState,
  LoadingState,
} from "../components/common/States";
import { SectionCard } from "../components/common/SectionCard";
import { formatLoanDate, matchOutcome } from "../lib/format";
import { useSeasons } from "../features/seasons/hooks";
import { useTransferStatistics } from "../features/transfers/hooks";
import { useMatches } from "../features/matches/hooks";
import { usePlayerSearch } from "../features/players/hooks";
import { useContextTeam } from "../features/team-context/TeamContext";
import type { MatchSummaryDTO } from "../types/match";
import type { TransferStatistics } from "../types/transfer";

function formatEuroAmount(amount: number): string {
  const absolute = Math.abs(amount);
  if (absolute >= 1_000_000) {
    return `€${(amount / 1_000_000).toLocaleString(undefined, { maximumFractionDigits: 1 })}M`;
  }
  if (absolute >= 1_000) {
    return `€${(amount / 1_000).toLocaleString(undefined, { maximumFractionDigits: 0 })}K`;
  }
  return `€${amount.toLocaleString()}`;
}

// Result tones per the approved pill colors: W green, D amber, L rose.
const RESULT_TONES: Record<"W" | "D" | "L", string> = {
  W: "border-emerald-500/20 bg-emerald-500/10 text-emerald-400",
  D: "border-amber-500/20 bg-amber-500/10 text-amber-400",
  L: "border-rose-500/20 bg-rose-500/10 text-rose-400",
};

// W / D / L of the viewed club via lib matchOutcome (a shootout decides a
// level match); no result when a score is null.
function resultFor(match: MatchSummaryDTO): "W" | "D" | "L" | null {
  if (match.scoreHome === null || match.scoreAway === null) return null;
  switch (
    matchOutcome(
      match.scoreHome,
      match.scoreAway,
      match.homeOrAway,
      match.penaltyHome,
      match.penaltyAway,
    )
  ) {
    case "WIN":
      return "W";
    case "LOSS":
      return "L";
    case "DRAW":
      return "D";
    default:
      return null;
  }
}

function formatKickoff(iso: string): string {
  return new Date(iso).toLocaleTimeString(undefined, {
    hour: "2-digit",
    minute: "2-digit",
    hour12: false,
  });
}

function MetricCard({
  icon,
  label,
  value,
  detail,
  tone,
}: {
  icon: string;
  label: string;
  value: ReactNode;
  detail: string;
  tone: "blue" | "green" | "slate";
}) {
  const tones = {
    blue: "bg-primary/10 text-primary",
    green: "bg-emerald-400/10 text-emerald-300",
    slate: "bg-surface-container-high text-on-surface-variant",
  };

  return (
    <section className="flex min-h-40 flex-col justify-between border border-outline-variant/50 bg-surface-container-low p-5 shadow-sm transition-colors hover:border-primary/40">
      <div className="mb-4 flex items-center gap-2.5">
        <span
          className={`flex h-8 w-8 items-center justify-center rounded ${tones[tone]}`}
        >
          <span className="material-symbols-outlined text-[18px]">{icon}</span>
        </span>
        <h2 className="font-label-caps text-label-caps uppercase text-on-surface-variant">
          {label}
        </h2>
      </div>
      <div>
        <p className="font-data-mono text-3xl font-semibold text-on-surface">
          {value}
        </p>
        <p className="mt-2 text-xs text-on-surface-variant">{detail}</p>
      </div>
    </section>
  );
}

function TransferComparison({
  statistics,
}: {
  statistics: TransferStatistics;
}) {
  const highestValue = Math.max(
    statistics.totalFeeToBuy,
    statistics.totalFeeToSell,
    1,
  );
  const scale = Math.ceil(highestValue / 50_000_000) * 50_000_000 || 50_000_000;
  const rows = [
    {
      label: "Spending",
      amount: statistics.totalFeeToBuy,
      color: "bg-primary-container",
    },
    {
      label: "Revenue",
      amount: statistics.totalFeeToSell,
      color: "bg-tertiary-fixed-dim",
    },
  ];
  const tiles = [
    {
      key: "permanent-buy",
      label: "Permanent signings (season)",
      count: statistics.permanentBuyCount,
    },
    {
      key: "permanent-sell",
      label: "Permanent sales (season)",
      count: statistics.permanentSellCount,
    },
    {
      key: "loan-in",
      label: "Loans in (season)",
      count: statistics.loanInCount,
    },
    {
      key: "loan-out",
      label: "Loans out (season)",
      count: statistics.loanOutCount,
    },
    {
      key: "contract-extensions",
      label: "Contract extensions (season)",
      count: statistics.contractExtensionCount,
    },
  ];

  return (
    <section className="border border-outline-variant/50 bg-surface-container-low p-5 shadow-sm md:p-6">
      <div className="mb-6 flex flex-col justify-between gap-3 border-b border-outline-variant/30 pb-5 sm:flex-row sm:items-start">
        <div>
          <div className="flex flex-wrap items-center gap-2">
            <h2 className="font-headline-sm text-lg font-semibold text-on-surface">
              Spending vs revenue
            </h2>
            <span className="border border-primary/30 bg-primary-container/20 px-2 py-0.5 font-data-mono text-[11px] text-primary">
              Net: {statistics.netSpend < 0 ? "−" : ""}
              {formatEuroAmount(Math.abs(statistics.netSpend))}
            </span>
          </div>
          <p className="mt-1 text-xs text-on-surface-variant">
            {statistics.teamName} transfer fees for{" "}
            {statistics.season.replace("-", "/")}
          </p>
        </div>
        <Link
          to="/transfers"
          className="inline-flex items-center gap-1 self-start text-xs font-medium text-primary hover:text-on-surface"
        >
          View transfers
          <span className="material-symbols-outlined text-[16px]">
            arrow_forward
          </span>
        </Link>
      </div>

      <div
        className="p-4 rounded bg-surface-container-low flex flex-col gap-4"
        aria-label="Spending compared with revenue"
      >
        {rows.map((row) => (
          <div key={row.label}>
            <div className="flex items-baseline justify-between gap-3">
              <span className="text-sm font-semibold text-on-surface">
                {row.label}
              </span>
              <span className="font-data-mono font-bold text-sm text-on-surface">
                {formatEuroAmount(row.amount)}
              </span>
            </div>
            <div className="mt-1.5 h-3 overflow-hidden rounded-full bg-surface-container-highest">
              <div
                className={`h-full rounded-full ${row.color} transition-[width] duration-500`}
                style={{
                  width: `${Math.max((row.amount / scale) * 100, row.amount ? 1 : 0)}%`,
                }}
              />
            </div>
          </div>
        ))}
      </div>

      <div className="mt-5 flex justify-between border-t border-outline-variant/30 pt-3 font-data-mono text-[11px] text-on-surface-variant">
        <span>€0</span>
        <span>{formatEuroAmount(scale)}</span>
      </div>

      <div className="mt-5 grid grid-cols-2 sm:grid-cols-5 gap-3">
        {tiles.map((tile) => (
          <div
            key={tile.key}
            className="p-2.5 rounded bg-surface-container-low flex flex-col"
          >
            <span className="font-label-caps text-[10px] uppercase text-outline truncate">
              {tile.label}
            </span>
            <span className="font-data-mono text-headline-sm font-bold text-on-surface mt-0.5">
              {tile.count}
            </span>
          </div>
        ))}
      </div>
    </section>
  );
}

// Recent form: last 5 finished results as W/D/L chips. Matches arrive
// newest-first (sort desc), so filtering then reversing puts the oldest
// chip on the left and the newest on the right; no result yet shows "—".
function RecentFormCard({
  matches,
  isLoading,
  isError,
  error,
}: {
  matches: MatchSummaryDTO[];
  isLoading: boolean;
  isError: boolean;
  error: unknown;
}) {
  const chips = matches
    .map(resultFor)
    .filter((chip): chip is "W" | "D" | "L" => chip !== null)
    .reverse();
  return (
    <MetricCard
      icon="history"
      label="Recent form"
      tone="slate"
      detail={
        isError
          ? `Results unavailable${error instanceof Error ? ` — ${error.message}` : ""}`
          : "Last 5 finished matches"
      }
      value={
        isLoading ? (
          "…"
        ) : isError ? (
          "—"
        ) : chips.length > 0 ? (
          <span className="flex items-center gap-2">
            {chips.map((chip, index) => (
              <span
                key={`${chip}-${index}`}
                className={`flex h-8 w-8 items-center justify-center rounded border font-data-mono text-sm font-bold ${RESULT_TONES[chip]}`}
              >
                {chip}
              </span>
            ))}
          </span>
        ) : (
          "—"
        )
      }
    />
  );
}

// Shared match row: short date, "vs" (home) / "at" (away) + opponent,
// competition, then the kick-off time (upcoming) or a "W 3–0" score pill
// (results, our score first). Rows navigate to the match detail page.
function MatchRow({
  match,
  showResult,
}: {
  match: MatchSummaryDTO;
  showResult: boolean;
}) {
  const navigate = useNavigate();
  const home = match.homeOrAway === "home";
  const result = resultFor(match);
  const ours = home ? match.scoreHome : match.scoreAway;
  const theirs = home ? match.scoreAway : match.scoreHome;
  const ourPens = home ? match.penaltyHome : match.penaltyAway;
  const theirPens = home ? match.penaltyAway : match.penaltyHome;
  const pens =
    ourPens != null && theirPens != null ? ` (${ourPens}–${theirPens} p)` : "";

  const right =
    showResult && result !== null ? (
      <span
        className={`inline-flex h-6 shrink-0 items-center rounded border px-2 font-data-mono text-xs font-bold ${RESULT_TONES[result]}`}
      >
        {result} {ours}–{theirs}
        {pens}
      </span>
    ) : (
      <span className="shrink-0 font-data-mono text-xs text-on-surface-variant">
        {formatKickoff(match.matchDate)}
      </span>
    );

  return (
    <button
      type="button"
      onClick={() => navigate(`/matches/${match.matchId}`)}
      className="flex w-full items-center gap-3 py-2 text-left transition-colors hover:bg-surface-container-high/40"
    >
      <span className="w-24 shrink-0 font-data-mono text-xs text-on-surface-variant">
        {formatLoanDate(match.matchDate)}
      </span>
      <span className="min-w-0 flex-1 truncate">
        <span className="font-data-mono text-xs text-primary">
          {home ? "vs" : "at"}
        </span>{" "}
        <span className="text-sm text-on-surface">{match.opponentName}</span>
        <span className="ml-1.5 hidden text-xs text-on-surface-variant sm:inline">
          {match.competitionName}
        </span>
      </span>
      {right}
    </button>
  );
}

// Shared card for Upcoming fixtures / Recent results.
function MatchListCard({
  title,
  icon,
  matches,
  emptyTitle,
  showResult,
  isLoading,
  isError,
  error,
}: {
  title: string;
  icon: string;
  matches: MatchSummaryDTO[];
  emptyTitle: string;
  showResult: boolean;
  isLoading: boolean;
  isError: boolean;
  error: unknown;
}) {
  return (
    <SectionCard title={title} icon={icon} className="h-full">
      {isLoading ? (
        <LoadingState label={`Loading ${title.toLowerCase()}...`} />
      ) : isError ? (
        <ErrorState error={error} />
      ) : matches.length === 0 ? (
        <EmptyState title={emptyTitle} />
      ) : (
        <div className="divide-y divide-outline-variant/30">
          {matches.map((match) => (
            <MatchRow
              key={match.matchId}
              match={match}
              showResult={showResult}
            />
          ))}
        </div>
      )}
    </SectionCard>
  );
}

// TODO(backend): only the squad total remains a separate count request.
type PlayerQuery = ReturnType<typeof usePlayerSearch>;

// Squad snapshot: the squad total comes from a player search (pageSize 1; the
// count is read from pagination.totalItems so no player rows are fetched).
// The two loan rows read the season-scoped transfer statistics instead of
// current player status, so they count loan-ins from untracked clubs and
// stub players the player queries miss.
type SquadRow =
  | { key: string; label: string; hint: string; source: "squadAll" }
  | {
      key: string;
      label: string;
      hint: string;
      source: "statistics";
      stat: "loanOut" | "loanIn";
    };

function SquadSnapshotCard({
  squadAll,
  statistics,
}: {
  squadAll: PlayerQuery;
  statistics: UseQueryResult<TransferStatistics>;
}) {
  const rows: SquadRow[] = [
    {
      key: "squad",
      label: "Squad",
      hint: "All club players",
      source: "squadAll",
    },
    {
      key: "loan-out",
      label: "Loaned out (season)",
      hint: "Loaned out during the selected season",
      source: "statistics",
      stat: "loanOut",
    },
    {
      key: "loan-in",
      label: "Loaned in (season)",
      hint: "Loaned in during the selected season",
      source: "statistics",
      stat: "loanIn",
    },
  ];

  return (
    <SectionCard title="Squad snapshot" icon="groups" className="h-full">
      <div className="divide-y divide-outline-variant/30">
        {rows.map((row) => (
          <div
            key={row.key}
            className="flex items-center justify-between py-2.5"
          >
            <div>
              <p className="text-sm text-on-surface">{row.label}</p>
              <p className="text-xs text-on-surface-variant">{row.hint}</p>
            </div>
            {row.source === "squadAll" ? (
              squadAll.isError ? (
                <span className="text-xs text-error">Failed</span>
              ) : !squadAll.data ? (
                <span className="font-data-mono text-sm text-on-surface-variant">
                  …
                </span>
              ) : (
                <span className="font-data-mono text-2xl font-semibold text-on-surface">
                  {squadAll.data.pagination.totalItems}
                </span>
              )
            ) : statistics.isError ? (
              <span className="text-xs text-error">Failed</span>
            ) : !statistics.data ? (
              <span className="font-data-mono text-sm text-on-surface-variant">
                …
              </span>
            ) : (
              <span className="font-data-mono text-2xl font-semibold text-on-surface">
                {row.stat === "loanOut"
                  ? statistics.data.loanOutCount
                  : statistics.data.loanInCount}
              </span>
            )}
          </div>
        ))}
      </div>
    </SectionCard>
  );
}

export function DashboardPage() {
  const { teamId: contextTeamId, team } = useContextTeam();
  const [chosenSeason, setChosenSeason] = useState("");
  const seasons = useSeasons();
  const seasonOptions = seasons.data?.data ?? [];
  const activeSeason =
    seasonOptions.find((season) => season.code === chosenSeason)?.code ??
    seasonOptions[0]?.code;
  const statistics = useTransferStatistics({
    teamId: contextTeamId,
    season: activeSeason ?? "",
  });
  const current = statistics.data;

  // One request for finished matches feeds both Recent results and the
  // Recent form chips (season selector never affects match data).
  const recentFinished = useMatches({
    teamId: contextTeamId,
    status: "FINISHED",
    sort: "desc",
    pageSize: 5,
  });
  const upcoming = useMatches({
    teamId: contextTeamId,
    status: "UPCOMING",
    pageSize: 5,
  });
  const finishedItems = recentFinished.data?.data ?? [];
  const upcomingItems = upcoming.data?.data ?? [];
  const transfersEmpty =
    !statistics.isError && !statistics.isLoading && !current;

  // TODO(backend): only the squad total remains a separate count request.
  const squadAll = usePlayerSearch({ teamId: contextTeamId, pageSize: 1 });

  return (
    <div className="space-y-7">
      <div className="flex flex-col justify-between gap-4 sm:flex-row sm:items-end">
        <div>
          <div className="mb-1 flex flex-wrap items-center gap-2">
            <span className="font-label-caps text-label-caps uppercase text-primary">
              {team.teamName} overview
            </span>
            <span className="h-1 w-1 rounded-full bg-outline-variant" />
            <span className="font-data-mono text-xs text-on-surface-variant">
              {team.leagueName}
            </span>
          </div>
          <h1 className="font-display-lg text-3xl font-bold text-on-surface">
            Dashboard
          </h1>
          <p className="mt-1 text-sm text-on-surface-variant">
            Transfers, squad snapshot and fixtures for {team.teamName}
          </p>
        </div>

        {seasonOptions.length > 0 && (
          <div
            className="flex max-w-full items-center gap-1 overflow-x-auto border border-outline-variant/60 bg-surface-container-low p-1"
            role="group"
            aria-label="Select season"
          >
            {seasonOptions.slice(0, 3).map((season) => {
              const isActive = season.code === activeSeason;
              return (
                <button
                  key={season.code}
                  type="button"
                  aria-pressed={isActive}
                  onClick={() => setChosenSeason(season.code)}
                  className={`whitespace-nowrap px-3 py-1.5 text-xs font-medium transition-colors ${
                    isActive
                      ? "bg-primary-container text-on-primary"
                      : "text-on-surface-variant hover:bg-surface-container-high hover:text-on-surface"
                  }`}
                >
                  {season.name}
                </button>
              );
            })}
          </div>
        )}
      </div>

      {/* Row 1 — KPI cards: Spending, Revenue, Net spend, Recent form */}
      <div className="grid grid-cols-1 gap-4 sm:grid-cols-2 xl:grid-cols-4">
        {statistics.isError ? (
          <div className="sm:col-span-2 xl:col-span-3">
            <ErrorState error={statistics.error} />
          </div>
        ) : (
          <>
            <MetricCard
              icon="south_east"
              label="Spending"
              value={current ? formatEuroAmount(current.totalFeeToBuy) : "…"}
              detail={
                current
                  ? `${current.permanentBuyCount} buys · ${current.loanInCount} loans in`
                  : transfersEmpty
                    ? "No configured seasons"
                    : "Loading..."
              }
              tone="blue"
            />
            <MetricCard
              icon="north_east"
              label="Revenue"
              value={current ? formatEuroAmount(current.totalFeeToSell) : "…"}
              detail={
                current
                  ? `${current.permanentSellCount} sales · ${current.loanOutCount} loans out`
                  : transfersEmpty
                    ? "No configured seasons"
                    : "Loading..."
              }
              tone="green"
            />
            <MetricCard
              icon="price_change"
              label="Net spend"
              value={
                current
                  ? `${current.netSpend < 0 ? "−" : ""}${formatEuroAmount(Math.abs(current.netSpend))}`
                  : "…"
              }
              detail={
                current
                  ? `Spending minus revenue · ${current.season.replace("-", "/")}`
                  : transfersEmpty
                    ? "No configured seasons"
                    : "Loading..."
              }
              tone="slate"
            />
          </>
        )}
        <RecentFormCard
          matches={finishedItems}
          isLoading={recentFinished.isLoading}
          isError={recentFinished.isError}
          error={recentFinished.error}
        />
      </div>
      {/* Row 2 — Spending vs revenue + squad snapshot */}
      <div className="grid grid-cols-1 gap-4 lg:grid-cols-3">
        {statistics.isError ? (
          <div className="lg:col-span-3">
            <ErrorState error={statistics.error} />
          </div>
        ) : statistics.isLoading || !current ? (
          <div className="lg:col-span-3">
            {transfersEmpty ? (
              <EmptyState title="No configured seasons are available." />
            ) : (
              <LoadingState label="Loading transfer overview..." />
            )}
          </div>
        ) : (
          <div className="lg:col-span-2">
            <TransferComparison statistics={current} />
          </div>
        )}
        <SquadSnapshotCard squadAll={squadAll} statistics={statistics} />
      </div>

      {/* Row 3 — upcoming fixtures + recent results */}
      <div className="grid grid-cols-1 gap-4 lg:grid-cols-2">
        <MatchListCard
          title="Upcoming fixtures"
          icon="event"
          matches={upcomingItems}
          emptyTitle="No upcoming fixtures"
          showResult={false}
          isLoading={upcoming.isLoading}
          isError={upcoming.isError}
          error={upcoming.error}
        />
        <MatchListCard
          title="Recent results"
          icon="sports_score"
          matches={finishedItems}
          emptyTitle="No finished matches"
          showResult={true}
          isLoading={recentFinished.isLoading}
          isError={recentFinished.isError}
          error={recentFinished.error}
        />
      </div>
    </div>
  );
}

import { Fragment, useMemo, useState } from "react";
import { useNavigate } from "react-router-dom";
import { useMatches } from "../features/matches/hooks";
import { useCompetitions } from "../features/competitions/hooks";
import { ApiError } from "../api/client";
import type {
  MatchSearchQuery,
  MatchStatus,
  MatchSummaryDTO,
} from "../types/match";

const PAGE_SIZE = 20;
const ALL_COMPETITIONS = "All Competitions";

const OUR_TEAM_NAME = "Chelsea";
const OUR_TEAM_CODE = "CHE";

function fallbackCode(name: string): string {
  return name.slice(0, 3).toUpperCase();
}

// Unlike competition, MatchSearchQuery already has a `Season` field —
// MatchService.SearchMatchesAsync resolves it server-side via
// ISeasonService.Resolve into a fromDate/toDate range. So this filter is
// sent straight through to the API as the season Code — no client-side
// date derivation needed.

// appsettings.json first.
interface SeasonOption {
  code: string;
  label: string;
}

const SEASON_OPTIONS: SeasonOption[] = [
  { code: "all", label: "All Seasons" },
  { code: "2025-26", label: "2025/26" },
  { code: "2026-27", label: "2026/27" },
];

const STATUS_TABS: { label: string; value: MatchStatus | "all" }[] = [
  { label: "All", value: "all" },
  { label: "Upcoming", value: "UPCOMING" },
  { label: "Finished", value: "FINISHED" },
];

function monthGroupKey(iso: string): string {
  const d = new Date(iso);
  return d
    .toLocaleDateString(undefined, { month: "long", year: "numeric" })
    .toUpperCase();
}

function formatShortDate(iso: string): string {
  return new Date(iso).toLocaleDateString(undefined, {
    month: "short",
    day: "numeric",
  });
}

function formatKickoffTime(iso: string): string {
  return new Date(iso).toLocaleTimeString(undefined, {
    hour: "2-digit",
    minute: "2-digit",
    hour12: false,
  });
}

function statusBadge(status: MatchStatus): { label: string; dotClass: string } {
  switch (status) {
    case "live":
      return { label: "LIVE", dotClass: "bg-error animate-pulse" };
    case "FINISHED":
      return { label: "FT", dotClass: "bg-secondary" };
    case "cancelled":
      return { label: "CANC.", dotClass: "bg-error" };
    case "UPCOMING":
    default:
      return { label: "UPCOMING", dotClass: "bg-primary-container" };
  }
}

interface Row {
  homeName: string;
  homeCode: string;
  awayName: string;
  awayCode: string;
}

function toRow(match: MatchSummaryDTO): Row {
  if (match.homeOrAway === "home") {
    return {
      homeName: OUR_TEAM_NAME,
      homeCode: OUR_TEAM_CODE,
      awayName: match.opponentName,
      awayCode: fallbackCode(match.opponentName),
    };
  }
  return {
    homeName: match.opponentName,
    homeCode: fallbackCode(match.opponentName),
    awayName: OUR_TEAM_NAME,
    awayCode: OUR_TEAM_CODE,
  };
}

function groupByMonth(items: MatchSummaryDTO[]): [string, MatchSummaryDTO[]][] {
  const groups = new Map<string, MatchSummaryDTO[]>();
  for (const item of items) {
    const key = monthGroupKey(item.matchDate);
    const bucket = groups.get(key);
    if (bucket) bucket.push(item);
    else groups.set(key, [item]);
  }
  return Array.from(groups.entries());
}

export function MatchListPage() {
  const navigate = useNavigate();

  const [page, setPage] = useState(1);
  const [status, setStatus] = useState<MatchStatus | "all">("all");
  const [competition, setCompetition] = useState(ALL_COMPETITIONS);
  const [seasonCode, setSeasonCode] = useState("all");

  const query: MatchSearchQuery = useMemo(
    () => ({
      page,
      pageSize: PAGE_SIZE,
      status: status === "all" ? undefined : status,
      season: seasonCode === "all" ? undefined : seasonCode,
    }),
    [page, status, seasonCode],
  );

  const { data, isLoading, isError, error, isFetching } = useMatches(query);

  // GET /api/competitions - CompetitionsController. Used to drive the
  // dropdown for real instead of the old hand-copied static list.
  const { data: competitionsData, isLoading: competitionsLoading } =
    useCompetitions();

  const competitionOptions = useMemo(
    () => [
      ALL_COMPETITIONS,
      ...(competitionsData?.data.map((c) => c.name) ?? []),
    ],
    [competitionsData],
  );

  const filteredItems = useMemo(() => {
    const items = data?.data ?? [];
    if (competition === ALL_COMPETITIONS) return items;
    return items.filter((m) => m.competitionName === competition);
  }, [data, competition]);

  const monthGroups = useMemo(
    () => groupByMonth(filteredItems),
    [filteredItems],
  );

  const totalCount = data?.pagination.totalItems ?? 0;
  const totalPages = Math.max(1, data?.pagination.totalPages ?? 1);

  function handleStatusChange(value: MatchStatus | "all") {
    setStatus(value);
    setPage(1);
  }

  function handleSeasonChange(code: string) {
    setSeasonCode(code);
    setPage(1);
  }

  function handleCompetitionChange(name: string) {
    setCompetition(name);
    setPage(1);
  }

  const errorMessage =
    error instanceof ApiError
      ? error.message
      : error instanceof Error
        ? error.message
        : null;

  return (
    <div className="p-container-padding max-w-[1600px] mx-auto">
      {/* Page Header */}
      <div className="mb-gutter">
        <h2 className="font-display-lg text-display-lg text-on-surface mb-2">
          Matches
        </h2>
        <p className="font-body-md text-body-md text-on-surface-variant">
          Results and upcoming fixtures
        </p>
      </div>

      {/* Toolbar: status tabs + competition filter */}
      <div className="bg-surface-container border border-outline-variant rounded-lg p-unit mb-gutter flex flex-wrap gap-gutter items-center justify-between">
        <div className="flex items-center gap-1 bg-surface-container-low border border-outline-variant rounded-DEFAULT p-1">
          {STATUS_TABS.map((tab) => (
            <button
              key={tab.value}
              onClick={() => handleStatusChange(tab.value)}
              className={
                tab.value === status
                  ? "px-3 py-1.5 rounded-DEFAULT bg-primary-container text-on-primary-container font-label-caps text-label-caps uppercase transition-colors"
                  : "px-3 py-1.5 rounded-DEFAULT text-on-surface-variant hover:text-on-surface font-label-caps text-label-caps uppercase transition-colors"
              }
            >
              {tab.label}
            </button>
          ))}
        </div>

        <div className="flex items-center gap-gutter">
          <div className="flex items-center gap-2 border border-outline-variant rounded-DEFAULT bg-surface-container-low p-1">
            <span className="text-outline-variant font-label-caps text-label-caps px-2">
              SEASON
            </span>
            <select
              className="bg-transparent border-none text-on-surface focus:ring-0 font-body-sm py-1 pl-2 pr-8 cursor-pointer appearance-none"
              value={seasonCode}
              onChange={(e) => handleSeasonChange(e.target.value)}
            >
              {SEASON_OPTIONS.map((opt) => (
                <option
                  key={opt.code}
                  value={opt.code}
                  className="bg-surface-container-low text-on-surface"
                >
                  {opt.label}
                </option>
              ))}
            </select>
          </div>

          <div className="flex items-center gap-2 border border-outline-variant rounded-DEFAULT bg-surface-container-low p-1">
            <span className="text-outline-variant font-label-caps text-label-caps px-2">
              COMPETITION
            </span>
            <select
              className="bg-transparent border-none text-on-surface focus:ring-0 font-body-sm py-1 pl-2 pr-8 cursor-pointer appearance-none disabled:opacity-50"
              value={competition}
              disabled={competitionsLoading}
              onChange={(e) => handleCompetitionChange(e.target.value)}
            >
              {competitionOptions.map((opt) => (
                <option
                  key={opt}
                  value={opt}
                  className="bg-surface-container-low text-on-surface"
                >
                  {opt}
                </option>
              ))}
            </select>
          </div>
        </div>
      </div>

      {/* Data Table */}
      <div className="bg-surface-container border border-outline-variant rounded-lg overflow-hidden flex flex-col shadow-[0_0_0_1px_rgba(71,85,105,0.1)]">
        <div className="overflow-x-auto">
          <table className="w-full text-left border-collapse whitespace-nowrap">
            <thead>
              <tr className="bg-surface-container-high border-b border-outline-variant">
                <th className="font-label-caps text-label-caps text-on-surface-variant py-3 px-4 w-24">
                  DATE
                </th>
                <th className="font-label-caps text-label-caps text-on-surface-variant py-3 px-4 w-40">
                  COMPETITION
                </th>
                <th className="font-label-caps text-label-caps text-on-surface-variant py-3 px-4 text-right">
                  HOME
                </th>
                <th className="font-label-caps text-label-caps text-on-surface-variant py-3 px-4 w-28 text-center">
                  SCORE
                </th>
                <th className="font-label-caps text-label-caps text-on-surface-variant py-3 px-4">
                  AWAY
                </th>
                <th className="font-label-caps text-label-caps text-on-surface-variant py-3 px-4 w-28">
                  STATUS
                </th>
              </tr>
            </thead>
            <tbody className="font-body-sm">
              {isLoading && (
                <tr>
                  <td
                    colSpan={6}
                    className="py-8 px-4 text-center text-on-surface-variant"
                  >
                    Loading matches...
                  </td>
                </tr>
              )}

              {isError && (
                <tr>
                  <td colSpan={6} className="py-8 px-4 text-center text-error">
                    Failed to load matches
                    {errorMessage ? `: ${errorMessage}` : ""}
                  </td>
                </tr>
              )}

              {!isLoading && !isError && filteredItems.length === 0 && (
                <tr>
                  <td
                    colSpan={6}
                    className="py-8 px-4 text-center text-on-surface-variant"
                  >
                    No matches found for the selected filters.
                  </td>
                </tr>
              )}

              {!isLoading &&
                !isError &&
                monthGroups.map(([monthLabel, matches]) => (
                  <Fragment key={`group-${monthLabel}`}>
                    <tr className="bg-surface-container-low">
                      <td
                        colSpan={6}
                        className="py-1.5 px-4 font-label-caps text-label-caps text-on-surface-variant tracking-wider"
                      >
                        {monthLabel}
                      </td>
                    </tr>
                    {matches.map((match) => {
                      const row = toRow(match);
                      const badge = statusBadge(match.status);
                      const scoreText =
                        match.status === "UPCOMING" ||
                        match.scoreHome === null ||
                        match.scoreAway === null
                          ? formatKickoffTime(match.matchDate)
                          : `${match.scoreHome} — ${match.scoreAway}`;

                      return (
                        <tr
                          key={match.matchId}
                          onClick={() => navigate(`/matches/${match.matchId}`)}
                          className="match-row h-row-height-compact border-b border-outline-variant/30 hover:bg-primary-container/10 group cursor-pointer transition-colors relative"
                        >
                          <td className="py-2 px-4 font-data-mono text-data-mono text-on-surface-variant relative">
                            <div className="absolute left-0 top-0 bottom-0 w-[2px] bg-primary opacity-0 group-hover:opacity-100 transition-opacity" />
                            {formatShortDate(match.matchDate)}
                          </td>
                          <td className="py-2 px-4 text-on-surface-variant">
                            {match.competitionName}
                          </td>
                          <td className="py-2 px-4 text-right">
                            <div className="flex items-center justify-end gap-2">
                              <span className="text-on-surface font-semibold">
                                {row.homeName}
                              </span>
                              <span className="w-6 h-6 rounded-full bg-surface-container-high border border-outline-variant flex items-center justify-center font-data-mono text-[10px] text-on-surface-variant">
                                {row.homeCode}
                              </span>
                            </div>
                          </td>
                          <td className="py-2 px-4 text-center font-data-mono text-data-mono text-on-surface">
                            {scoreText}
                          </td>
                          <td className="py-2 px-4">
                            <div className="flex items-center gap-2">
                              <span className="w-6 h-6 rounded-full bg-surface-container-high border border-outline-variant flex items-center justify-center font-data-mono text-[10px] text-on-surface-variant">
                                {row.awayCode}
                              </span>
                              <span className="text-on-surface font-semibold">
                                {row.awayName}
                              </span>
                            </div>
                          </td>
                          <td className="py-2 px-4">
                            <span className="inline-flex items-center gap-1.5 px-2 py-0.5 rounded-full bg-surface-container-high border border-outline-variant text-[10px] font-bold text-on-surface uppercase tracking-wider">
                              <span
                                className={`w-1.5 h-1.5 rounded-full ${badge.dotClass}`}
                              />
                              {badge.label}
                            </span>
                          </td>
                        </tr>
                      );
                    })}
                  </Fragment>
                ))}
            </tbody>
          </table>
        </div>

        {/* Pagination */}
        <div className="bg-surface-container-high border-t border-outline-variant p-3 flex items-center justify-between">
          <span className="font-body-sm text-body-sm text-on-surface-variant">
            {totalCount === 0
              ? "No matches"
              : `Showing ${filteredItems.length} of ${totalCount} matches`}
            {isFetching && !isLoading ? " · refreshing..." : ""}
          </span>
          <div className="flex items-center gap-1">
            <button
              className="p-1 text-on-surface-variant hover:text-on-surface disabled:opacity-50 transition-colors"
              disabled={page <= 1}
              onClick={() => setPage((p) => Math.max(1, p - 1))}
            >
              <span className="material-symbols-outlined">chevron_left</span>
            </button>
            <span className="font-data-mono text-data-mono text-on-surface px-2">
              {page} / {totalPages}
            </span>
            <button
              className="p-1 text-on-surface-variant hover:text-on-surface disabled:opacity-50 transition-colors"
              disabled={page >= totalPages}
              onClick={() => setPage((p) => Math.min(totalPages, p + 1))}
            >
              <span className="material-symbols-outlined">chevron_right</span>
            </button>
          </div>
        </div>
      </div>
    </div>
  );
}

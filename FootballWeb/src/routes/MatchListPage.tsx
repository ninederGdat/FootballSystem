import { useEffect, useMemo, useState } from "react";
import { useNavigate } from "react-router-dom";
import { useMatches } from "../features/matches/hooks";
import { useCompetitions } from "../features/competitions/hooks";
import { ApiError } from "../api/client";
import { useContextTeam } from "../features/team-context/TeamContext";
import { getTrackedTeam, TRACKED_TEAMS } from "../lib/teams";
import { formatLoanDate } from "../lib/format";
import type {
  MatchSearchQuery,
  MatchStatus,
  MatchSummaryDTO,
} from "../types/match";

const PAGE_SIZE = 20;

type SortDir = "asc" | "desc";

// Finished tab reads newest first; All/Upcoming read oldest first.
function defaultSortFor(status: MatchStatus | "all"): SortDir {
  return status === "FINISHED" ? "desc" : "asc";
}

function fallbackCode(name: string): string {
  return name.slice(0, 3).toUpperCase();
}

// Tracked clubs own their official abbreviation (CFC/RCSA). Anything else
// falls back to the first 3 letters of its display name.
function teamAbbr(teamId: number | null, name: string): string {
  const tracked = teamId != null ? getTrackedTeam(teamId) : undefined;
  return tracked?.abbr ?? fallbackCode(name);
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

// "Aug 16, 2026" per the prototype date column.
function formatListDate(iso: string): string {
  return formatLoanDate(iso);
}

function formatKickoffTime(iso: string): string {
  return new Date(iso).toLocaleTimeString(undefined, {
    hour: "2-digit",
    minute: "2-digit",
    hour12: false,
  });
}

function isTracked(id: number | null): boolean {
  return id != null && getTrackedTeam(id) !== undefined;
}

// Approved status pill style: no dot; FT neutral, UPCOMING
// primary-container. live/cancel keep the pre-existing dot treatments
// (no prototype reference exists for them).
function statusBadge(status: MatchStatus): {
  label: string;
  pillClass: string;
  dotClass?: string;
} {
  switch (status) {
    case "live":
      return {
        label: "LIVE",
        pillClass: "bg-surface-container-high text-on-surface",
        dotClass: "bg-error animate-pulse",
      };
    case "FINISHED":
      return {
        label: "FT",
        pillClass: "bg-surface-container-highest text-on-surface",
      };
    case "cancelled":
      return {
        label: "CANC.",
        pillClass: "bg-surface-container-high text-on-surface",
        dotClass: "bg-error",
      };
    case "UPCOMING":
    default:
      return {
        label: "UPCOMING",
        pillClass: "bg-primary-container text-on-primary-container",
      };
  }
}

// Score pill colors derive only from actual data: the row club's result
// (homeOrAway + scores). Internal Chelsea-Strasbourg fixtures and rows
// without scores render neutral (constraint 3). Columns are always stored
// as "home - away" regardless of which club the row describes.
function scorePill(
  match: MatchSummaryDTO,
): { text: string; pillClass: string } {
  const { scoreHome, scoreAway } = match;
  const internal =
    isTracked(match.teamId) && isTracked(match.opponentTeamId);

  if (scoreHome === null || scoreAway === null) {
    return {
      text: formatKickoffTime(match.matchDate),
      pillClass:
        "bg-surface-container-high text-on-surface-variant font-medium",
    };
  }

  const scoreText = `${scoreHome} - ${scoreAway}`;

  if (internal) {
    return {
      text: scoreText,
      pillClass:
        "bg-surface-container-high text-on-surface font-bold border border-outline-variant/40",
    };
  }

  const ours = match.homeOrAway === "home" ? scoreHome : scoreAway;
  const theirs = match.homeOrAway === "home" ? scoreAway : scoreHome;
  const tone =
    ours > theirs
      ? "border bg-emerald-500/10 text-emerald-400 border-emerald-500/20"
      : ours < theirs
        ? "border bg-rose-500/10 text-rose-400 border-rose-500/20"
        : "border bg-amber-500/10 text-amber-400 border-amber-500/20";
  return { text: scoreText, pillClass: tone };
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

// Badge/name styling per club: viewed clubs get the primary treatment,
// tracked-but-not-viewed opponents get the internal style, everyone else
// is neutral. Viewed set = context club (scope) or all TRACKED_TEAMS (all).
function clubBadgeClass(
  clubId: number | null,
  viewedIds: number[],
): string | null {
  if (clubId == null) return null;
  if (viewedIds.includes(clubId)) {
    return "w-7 h-7 rounded-full bg-primary-container text-on-primary-container font-data-mono text-data-mono flex items-center justify-center font-bold ring-1 ring-primary/40";
  }
  if (isTracked(clubId)) {
    return "w-7 h-7 rounded-full bg-surface-bright text-on-surface font-data-mono text-data-mono flex items-center justify-center font-bold ring-1 ring-secondary/50";
  }
  return "w-7 h-7 rounded-full bg-surface-container-highest text-on-surface-variant font-data-mono text-data-mono flex items-center justify-center font-semibold";
}

function clubNameClass(
  clubId: number | null,
  viewedIds: number[],
): string {
  if (clubId != null && viewedIds.includes(clubId)) {
    return "font-semibold text-primary";
  }
  return "text-on-surface";
}

export function MatchListPage() {
  const navigate = useNavigate();

  const { teamId: contextTeamId, team } = useContextTeam();
  const [page, setPage] = useState(1);
  const [status, setStatus] = useState<MatchStatus | "all">("all");
  const [sort, setSort] = useState<SortDir>(defaultSortFor(status));
  const [scope, setScope] = useState<"context" | "all">("context");
  const [competitionId, setCompetitionId] = useState<number | "all">("all");
  const [seasonCode, setSeasonCode] = useState("all");
  const [opponentInput, setOpponentInput] = useState("");
  const [debouncedOpponent, setDebouncedOpponent] = useState("");

  // Server-side filter (MatchRepository opponent_name LIKE). Debounced so
  // typing does not fire a request per keystroke; the query resets page.
  useEffect(() => {
    const t = setTimeout(
      () => setDebouncedOpponent(opponentInput.trim()),
      400,
    );
    return () => clearTimeout(t);
  }, [opponentInput]);

  useEffect(() => {
    setPage(1);
  }, [contextTeamId, scope, debouncedOpponent]);

  const query: MatchSearchQuery = useMemo(
    () => ({
      page,
      pageSize: PAGE_SIZE,
      status: status === "all" ? undefined : status,
      sort,
      season: seasonCode === "all" ? undefined : seasonCode,
      teamId: scope === "context" ? contextTeamId : undefined,
      competitionId: competitionId === "all" ? undefined : competitionId,
      opponent: debouncedOpponent === "" ? undefined : debouncedOpponent,
    }),
    [page, status, sort, seasonCode, contextTeamId, scope, competitionId, debouncedOpponent],
  );

  const {
    data,
    isLoading,
    isError,
    error,
    isFetching,
    refetch,
  } = useMatches(query);

  // GET /api/competitions - CompetitionsController. Used to drive the
  // dropdown for real instead of the old hand-copied static list.
  const { data: competitionsData, isLoading: competitionsLoading } =
    useCompetitions();

  const competitionOptions = useMemo(
    () =>
      competitionsData?.data.map((c) => ({ id: c.id, name: c.name })) ?? [],
    [competitionsData],
  );

  const items = data?.data ?? [];

  const monthGroups = useMemo(
    () => groupByMonth(items),
    [items],
  );

  const totalCount = data?.pagination.totalItems ?? 0;
  const totalPages = Math.max(1, data?.pagination.totalPages ?? 1);

  // Viewed-club set drives the badge highlight (constraint: prototype's
  // scope all shows both tracked clubs primary, context shows only one).
  const viewedIds = useMemo(
    () =>
      scope === "context" ? [contextTeamId] : TRACKED_TEAMS.map((t) => t.teamId),
    [scope, contextTeamId],
  );

  function handleStatusChange(value: MatchStatus | "all") {
    setStatus(value);
    setSort(defaultSortFor(value));
    setPage(1);
  }

  function handleSortChange(value: string) {
    setSort(value === "desc" ? "desc" : "asc");
    setPage(1);
  }

  function handleSeasonChange(code: string) {
    setSeasonCode(code);
    setPage(1);
  }

  function handleCompetitionChange(value: string) {
    setCompetitionId(value === "all" ? "all" : Number(value));
    setPage(1);
  }

  function handleResetFilters() {
    setStatus("all");
    setSort(defaultSortFor("all"));
    setSeasonCode("all");
    setCompetitionId("all");
    setOpponentInput("");
    setDebouncedOpponent("");
    setScope("context");
    setPage(1);
  }

  const errorMessage =
    error instanceof ApiError
      ? error.message
      : error instanceof Error
        ? error.message
        : null;

  // TRACKED_TEAMS join for the scope=all subtitle (approved decision 6).
  const scopeLabel =
    scope === "context"
      ? team.teamName
      : TRACKED_TEAMS.map((t) => t.teamName).join(" and ");

  const statusPillBase =
    "inline-flex items-center px-2 py-0.5 rounded-full font-label-caps text-label-caps font-bold uppercase tracking-wider";

  const clubCell = (
    clubId: number | null,
    name: string,
    code: string,
    isOpponent: boolean,
  ) => {
    const badge = clubBadgeClass(clubId, viewedIds);
    // INTERNAL tag lives on the opponent cell only, and only when the
    // opponent is tracked but not the viewed club (prototype rule).
    const internalTracked =
      isOpponent &&
      isTracked(clubId) &&
      !(clubId != null && viewedIds.includes(clubId));
    return (
      <div className="flex items-center gap-2.5">
        {badge !== null && <span className={badge}>{code}</span>}
        <span className={clubNameClass(clubId, viewedIds)}>{name}</span>
        {badge === null && (
          <span className="font-data-mono text-data-mono text-on-surface">
            {code}
          </span>
        )}
        {internalTracked && (
          <span className="text-[10px] font-data-mono px-1.5 py-0.5 rounded bg-surface-container-high text-on-surface-variant">
            INTERNAL
          </span>
        )}
      </div>
    );
  };

  return (
    <div className="p-container-padding max-w-[1600px] mx-auto flex flex-col gap-5 pt-1">
      {/* Page Header */}
      <div className="flex flex-col gap-1">
        <h1 className="font-headline-md text-headline-md text-on-surface tracking-tight font-bold">
          Matches
        </h1>
        <p className="font-body-sm text-body-sm text-on-surface-variant">
          Results and upcoming fixtures for {scopeLabel}
        </p>
      </div>

      {/* Controls Toolbar */}
      <div className="flex flex-wrap items-center justify-between gap-3 p-3 rounded-xl bg-surface-container-low shadow-sm">
        <div className="flex flex-wrap items-center gap-3">
          <div className="flex items-center bg-surface-container-lowest p-1 rounded-lg">
            {STATUS_TABS.map((tab) => (
              <button
                key={tab.value}
                type="button"
                onClick={() => handleStatusChange(tab.value)}
                className={
                  tab.value === status
                    ? "px-3 py-1.5 rounded-lg bg-surface-container-highest text-on-surface font-body-sm text-body-sm font-semibold shadow-inner transition-colors"
                    : "px-3 py-1.5 rounded-lg text-on-surface-variant hover:text-on-surface font-body-sm text-body-sm transition-colors"
                }
              >
                {tab.label}
              </button>
            ))}
          </div>
          <div className="flex items-center bg-surface-container-lowest p-1 rounded-lg">
            <button
              type="button"
              onClick={() => setScope("context")}
              className={
                scope === "context"
                  ? "flex items-center gap-1.5 px-3 py-1.5 rounded-lg bg-primary-container text-on-primary-container font-body-sm text-body-sm font-semibold transition-all"
                  : "flex items-center gap-1.5 px-3 py-1.5 rounded-lg text-on-surface-variant hover:text-on-surface font-body-sm text-body-sm font-normal transition-all"
              }
            >
              {scope === "context" && (
                <span className="w-2 h-2 rounded-full bg-primary" />
              )}
              {team.teamName}
            </button>
            <button
              type="button"
              onClick={() => setScope("all")}
              className={
                scope === "all"
                  ? "flex items-center gap-1.5 px-3 py-1.5 rounded-lg bg-primary-container text-on-primary-container font-body-sm text-body-sm font-semibold transition-all"
                  : "flex items-center gap-1.5 px-3 py-1.5 rounded-lg text-on-surface-variant hover:text-on-surface font-body-sm text-body-sm font-normal transition-all"
              }
            >
              {scope === "all" && (
                <span className="w-2 h-2 rounded-full bg-primary" />
              )}
              All clubs
            </button>
          </div>
        </div>

        <div className="flex flex-wrap items-center gap-2">
          <div className="relative">
            <select
              className="appearance-none bg-surface-container-lowest text-on-surface font-body-sm text-body-sm rounded-lg pl-3 pr-8 py-1.5 focus:outline-none focus:ring-1 focus:ring-primary cursor-pointer"
              value={sort}
              aria-label="Sort matches"
              onChange={(e) => handleSortChange(e.target.value)}
            >
              <option value="desc">Newest first</option>
              <option value="asc">Oldest first</option>
            </select>
            <span className="material-symbols-outlined pointer-events-none absolute right-2 top-1/2 -translate-y-1/2 text-on-surface-variant text-[16px]">
              expand_more
            </span>
          </div>
          <div className="relative w-44 md:w-56">
            <span className="material-symbols-outlined absolute left-2.5 top-1/2 -translate-y-1/2 text-on-surface-variant text-[16px]">
              search
            </span>
            <input
              type="text"
              value={opponentInput}
              onChange={(e) => setOpponentInput(e.target.value)}
              placeholder="Filter opponent..."
              className="w-full bg-surface-container-lowest text-on-surface placeholder:text-on-surface-variant font-body-sm text-body-sm rounded-lg pl-8 pr-3 py-1.5 focus:outline-none focus:ring-1 focus:ring-primary"
            />
          </div>
          <div className="relative">
            <select
              className="appearance-none bg-surface-container-lowest text-on-surface font-body-sm text-body-sm rounded-lg pl-3 pr-8 py-1.5 focus:outline-none focus:ring-1 focus:ring-primary cursor-pointer"
              value={seasonCode}
              onChange={(e) => handleSeasonChange(e.target.value)}
            >
              {SEASON_OPTIONS.map((opt) => (
                <option key={opt.code} value={opt.code}>
                  {opt.label}
                </option>
              ))}
            </select>
            <span className="material-symbols-outlined pointer-events-none absolute right-2 top-1/2 -translate-y-1/2 text-on-surface-variant text-[16px]">
              expand_more
            </span>
          </div>
          <div className="relative">
            <select
              className="appearance-none bg-surface-container-lowest text-on-surface font-body-sm text-body-sm rounded-lg pl-3 pr-8 py-1.5 focus:outline-none focus:ring-1 focus:ring-primary cursor-pointer disabled:opacity-50"
              value={String(competitionId)}
              disabled={competitionsLoading}
              onChange={(e) => handleCompetitionChange(e.target.value)}
            >
              <option value="all">All Competitions</option>
              {competitionOptions.map((c) => (
                <option key={c.id} value={String(c.id)}>
                  {c.name}
                </option>
              ))}
            </select>
            <span className="material-symbols-outlined pointer-events-none absolute right-2 top-1/2 -translate-y-1/2 text-on-surface-variant text-[16px]">
              expand_more
            </span>
          </div>
        </div>
      </div>

      {/* Loading skeleton card */}
      {isLoading && (
        <div className="flex flex-col gap-4 p-8 rounded-xl bg-surface-container-low shadow-sm">
          <div className="flex items-center justify-between pb-3">
            <div className="flex items-center gap-3">
              <div className="w-5 h-5 rounded-full bg-primary-container animate-pulse" />
              <div className="h-4 w-40 bg-surface-container-high rounded animate-pulse" />
            </div>
            <div className="h-4 w-24 bg-surface-container-high rounded animate-pulse" />
          </div>
          <div className="space-y-3">
            {[0, 1, 2, 3, 4].map((i) => (
              <div
                key={i}
                className={
                  i === 0
                    ? "h-10 w-full bg-surface-container-high/60 rounded animate-pulse"
                    : "h-12 w-full bg-surface-container-high/40 rounded animate-pulse"
                }
              />
            ))}
          </div>
          <div className="flex items-center justify-center py-6 gap-2 text-on-surface-variant font-data-mono text-body-sm">
            <span className="material-symbols-outlined animate-spin text-primary text-[20px]">
              progress_activity
            </span>
            <span>Loading matches for {scopeLabel}...</span>
          </div>
        </div>
      )}

      {/* Error card; Retry refetches the same query */}
      {isError && (
        <div className="flex flex-col items-center justify-center p-14 rounded-xl bg-surface-container-low text-center shadow-sm">
          <div className="w-14 h-14 rounded-2xl bg-error-container/30 text-error flex items-center justify-center mb-4 ring-1 ring-error/20">
            <span className="material-symbols-outlined text-[32px]">
              error_outline
            </span>
          </div>
          <h3 className="font-headline-sm text-headline-sm text-on-surface font-semibold mb-1">
            Failed to load matches
          </h3>
          <p className="font-body-sm text-body-sm text-on-surface-variant max-w-md mb-5">
            Unable to retrieve match fixtures{errorMessage ? `: ${errorMessage}` : ""}
          </p>
          <button
            type="button"
            onClick={() => refetch()}
            className="px-4 py-2 rounded-lg bg-primary text-on-primary font-body-sm text-body-sm font-semibold hover:bg-primary/90 transition-colors"
          >
            Retry
          </button>
        </div>
      )}

      {/* Empty card with working reset */}
      {!isLoading && !isError && items.length === 0 && (
        <div className="flex flex-col items-center justify-center p-14 rounded-xl bg-surface-container-low text-center shadow-sm">
          <div className="w-14 h-14 rounded-2xl bg-surface-container-high flex items-center justify-center text-primary mb-4 shadow-inner">
            <span className="material-symbols-outlined text-[32px]">
              event_busy
            </span>
          </div>
          <h3 className="font-headline-sm text-headline-sm text-on-surface font-semibold mb-1">
            No matches found
          </h3>
          <p className="font-body-sm text-body-sm text-on-surface-variant max-w-md mb-5">
            No matches found for the selected filters.
          </p>
          <button
            type="button"
            onClick={handleResetFilters}
            className="px-4 py-2 rounded-lg bg-primary-container text-on-primary-container font-body-sm text-body-sm font-semibold hover:bg-primary-container/90 transition-colors shadow-sm"
          >
            Reset filters
          </button>
        </div>
      )}

      {/* Month cards. Server orders match_date asc, match_id asc; the
          grouping below only re-chunks the current page rows. */}
      {!isLoading && !isError &&
        monthGroups.map(([monthLabel, matches]) => (
          <div
            key={`group-${monthLabel}`}
            className="flex flex-col rounded-xl bg-surface-container-low shadow-md overflow-hidden"
          >
            <div className="flex items-center justify-between px-5 py-3 bg-surface-container">
              <div className="flex items-center gap-2.5">
                <span className="w-1.5 h-4 bg-primary rounded-full" />
                <span className="font-headline-sm text-headline-sm tracking-tight text-on-surface font-bold uppercase">
                  {monthLabel}
                </span>
                <span className="px-2 py-0.5 rounded-full bg-surface-container-high text-on-surface-variant font-data-mono text-label-caps">
                  {matches.length} matches
                </span>
              </div>
            </div>
            <div className="w-full overflow-x-auto">
              <table className="w-full text-left border-collapse whitespace-nowrap">
                <thead>
                  <tr className="bg-surface-container-high/60 text-on-surface-variant font-label-caps text-label-caps uppercase tracking-wider">
                    <th className="py-2.5 px-4 w-36">DATE</th>
                    <th className="py-2.5 px-4 w-48">COMPETITION</th>
                    <th className="py-2.5 px-4">HOME</th>
                    <th className="py-2.5 px-4 text-center w-28">SCORE</th>
                    <th className="py-2.5 px-4">AWAY</th>
                    <th className="py-2.5 px-4 text-right w-28">STATUS</th>
                    <th className="py-2.5 px-3 text-center w-10" />
                  </tr>
                </thead>
                <tbody className="divide-y divide-surface-container font-body-sm text-body-sm text-on-surface">
                  {matches.map((match) => {
                    const home = match.homeOrAway === "home";
                    const rowClubName =
                      match.teamId != null
                        ? (getTrackedTeam(match.teamId)?.teamName ?? "Unknown")
                        : "Unknown";
                    const homeName = home ? rowClubName : match.opponentName;
                    const awayName = home ? match.opponentName : rowClubName;
                    const homeId = home ? match.teamId : match.opponentTeamId;
                    const awayId = home ? match.opponentTeamId : match.teamId;
                    const homeCode = teamAbbr(homeId, homeName);
                    const awayCode = teamAbbr(awayId, awayName);
                    const badge = statusBadge(match.status);
                    const pill = scorePill(match);

                    return (
                      <tr
                        key={match.matchId}
                        onClick={() => navigate(`/matches/${match.matchId}`)}
                        className="group hover:bg-surface-container-high/40 transition-colors cursor-pointer"
                      >
                        <td className="py-3 px-4 text-on-surface font-medium">
                          {formatListDate(match.matchDate)}
                        </td>
                        <td className="py-3 px-4 text-on-surface-variant">
                          {match.competitionName}
                        </td>
                        <td className="py-3 px-4">
                          {clubCell(
                            homeId,
                            homeName,
                            homeCode,
                            homeId === match.opponentTeamId,
                          )}
                        </td>
                        <td className="py-3 px-4 text-center">
                          <span
                            className={`inline-block font-data-mono text-body-md px-2.5 py-0.5 rounded ${pill.pillClass}`}
                          >
                            {pill.text}
                          </span>
                        </td>
                        <td className="py-3 px-4">
                          {clubCell(
                            awayId,
                            awayName,
                            awayCode,
                            awayId === match.opponentTeamId,
                          )}
                        </td>
                        <td className="py-3 px-4 text-right">
                          <span className={`${statusPillBase} ${badge.pillClass}`}>
                            {badge.dotClass && (
                              <span
                                className={`w-1.5 h-1.5 rounded-full mr-1.5 ${badge.dotClass}`}
                              />
                            )}
                            {badge.label}
                          </span>
                        </td>
                        <td className="py-3 px-3 text-center">
                          <span className="material-symbols-outlined text-on-surface-variant text-[18px]">
                            chevron_right
                          </span>
                        </td>
                      </tr>
                    );
                  })}
                </tbody>
              </table>
            </div>
          </div>
        ))}

      {/* Pagination footer card (existing pagination behavior preserved) */}
      {!isLoading && !isError && (
        <div className="flex flex-col sm:flex-row sm:items-center justify-between gap-3 px-4 py-3 rounded-xl bg-surface-container-low text-on-surface-variant shadow-sm">
          <div className="font-data-mono text-body-sm">
            Showing{" "}
            <strong className="text-on-surface">{items.length}</strong> of{" "}
            <strong className="text-on-surface">{totalCount}</strong> matches
            {isFetching && !isLoading ? " · refreshing..." : ""}
          </div>
          <div className="flex items-center gap-1.5">
            <button
              type="button"
              className="px-2.5 py-1 rounded-lg bg-surface-container text-on-surface-variant hover:text-on-surface hover:bg-surface-container-high transition-colors font-body-sm flex items-center gap-1 disabled:opacity-50 disabled:cursor-not-allowed"
              disabled={page <= 1}
              onClick={() => setPage((p) => Math.max(1, p - 1))}
            >
              <span className="material-symbols-outlined text-[16px]">
                chevron_left
              </span>
              <span>Prev</span>
            </button>
            <span className="px-3 py-1 font-body-sm text-body-sm text-on-surface font-medium">
              Page {page} of {totalPages}
            </span>
            <button
              type="button"
              className="px-2.5 py-1 rounded-lg bg-surface-container text-on-surface hover:bg-surface-container-high transition-colors font-body-sm flex items-center gap-1 disabled:opacity-50 disabled:cursor-not-allowed"
              disabled={page >= totalPages}
              onClick={() => setPage((p) => Math.min(totalPages, p + 1))}
            >
              <span>Next</span>
              <span className="material-symbols-outlined text-[16px]">
                chevron_right
              </span>
            </button>
          </div>
        </div>
      )}
    </div>
  );
}

import { useState } from "react";
import { Link, useParams } from "react-router-dom";
import { useContextTeam } from "../features/team-context/TeamContext";
import { usePlayer, usePlayerAppearances } from "../features/players/hooks";
import { AppearanceList } from "../components/player/AppearanceList";
import { ProfileLoanLine } from "../features/players/ProfileLoanLine";
import { LoadingState, ErrorState } from "../components/common/States";
import {
  formatAge,
  formatLoanDate,
  formatMarketValue,
} from "../lib/format";
import type { PlayerTeamDTO } from "../types/player";

const badgeClass =
  "inline-flex items-center px-2 py-0.5 rounded text-label-caps font-label-caps uppercase tracking-wider";

// Raw DB status values are "Healthy" | "Injured" (PlayerClean default,
// PlayerMapper). Existing UI convention maps Healthy -> "Active"
// (PlayerListPage filter, StatusPill). Unknown values keep their raw label.
function statusLabel(status: string): string {
  const s = status.toLowerCase();
  if (s === "healthy") return "Active";
  if (s === "injured") return "Injured";
  return status;
}

// Mirror of PlayerRelationResolver.Resolve for the profile payload.
// The profile has no relation field; parentTeam is populated only when the
// player is on loan somewhere (PlayerService.IsOnLoan). Which loaned flavor
// applies is relative to the context club (same teamId the Players list
// queries with). Transferred cannot be detected from this payload.
function relationBadge(
  currentTeam: PlayerTeamDTO | null,
  parentTeam: PlayerTeamDTO | null,
  contextTeamId: number,
): string {
  if (!parentTeam) return "Current";
  if (!currentTeam) return "Loaned";
  if (
    currentTeam.teamId === contextTeamId &&
    parentTeam.teamId !== contextTeamId
  ) {
    return "Loaned In";
  }
  if (
    parentTeam.teamId === contextTeamId &&
    currentTeam.teamId !== contextTeamId
  ) {
    return "Loaned Out";
  }
  return "Loaned";
}

export function PlayerDetailPage() {
  const { playerId } = useParams<{ playerId: string }>();
  const id = Number(playerId);
  const [page, setPage] = useState(1);

  const profile = usePlayer(id);
  const appearances = usePlayerAppearances(id, page, 20);

  if (!Number.isFinite(id)) {
    return <ErrorState error={new Error("Invalid player ID in URL.")} />;
  }

  if (profile.isLoading) return <LoadingState label="Loading player..." />;
  if (profile.error) return <ErrorState error={profile.error} />;
  if (!profile.data) return null;

  const player = profile.data;
  const teamContext = useContextTeam();
  const isInjured = player.status.status.toLowerCase() === "injured";
  const statusDotClass = isInjured
    ? "bg-error"
    : player.status.status.toLowerCase() === "healthy"
      ? "bg-emerald-400"
      : "bg-outline";
  const relation = relationBadge(
    player.currentTeam,
    player.parentTeam,
    teamContext.teamId,
  );

  return (
    <div className="flex flex-col gap-6">
      {/* Back bar */}
      <div className="flex items-center justify-between pt-2">
        <Link
          to="/players"
          className="inline-flex items-center gap-1.5 px-2.5 py-1.5 rounded-lg bg-surface-container-low text-on-surface-variant hover:text-on-surface hover:bg-surface-container transition-colors group"
        >
          <span className="material-symbols-outlined text-[16px] text-primary transition-transform group-hover:-translate-x-0.5">
            arrow_back
          </span>
          <span className="font-label-caps text-label-caps tracking-wider uppercase">
            Back to Players
          </span>
        </Link>
      </div>

      {/* Identity Block */}
      <div className="bg-surface-container-low rounded-xl p-6 shadow-md relative overflow-hidden">
        <div className="absolute -right-16 -top-16 w-64 h-64 bg-primary/5 rounded-full blur-3xl pointer-events-none" />
        <div className="flex flex-col lg:flex-row lg:items-center justify-between gap-6 relative z-10">
          <div className="flex items-start sm:items-center gap-5">
            <div className="w-16 h-16 sm:w-20 sm:h-20 rounded-xl bg-surface-container-highest flex flex-col items-center justify-center shrink-0 shadow-inner">
              <span className="font-label-caps text-label-caps text-outline uppercase tracking-wider -mb-1">
                Shirt
              </span>
              <span className="font-data-mono text-display-lg text-primary font-bold">
                {player.shirtNumber !== null ? `#${player.shirtNumber}` : "—"}
              </span>
            </div>
            <div className="flex flex-col gap-2">
              <div className="flex flex-wrap items-center gap-2.5">
                <h1 className="font-display-lg text-display-lg text-on-surface font-bold tracking-tight">
                  {player.name}
                </h1>
                <span
                  className={`${badgeClass} bg-surface-container-highest text-primary-fixed-dim`}
                >
                  {relation}
                </span>
                <span
                  className={`${badgeClass} bg-surface-container text-on-surface gap-1.5`}
                >
                  <span
                    className={`w-1.5 h-1.5 rounded-full ${statusDotClass}`}
                  />
                  {statusLabel(player.status.status)}
                </span>
              </div>
              <div className="flex flex-wrap items-center gap-y-1 gap-x-3 text-on-surface-variant font-body-sm text-body-sm">
                {player.preferredPosition && (
                  <span className="font-medium text-on-surface">
                    {player.preferredPosition.positionName}
                  </span>
                )}
                {player.preferredPosition && player.nationality && (
                  <span className="text-outline">•</span>
                )}
                {player.nationality && <span>{player.nationality}</span>}
                {player.currentTeam && (
                  <>
                    {(player.preferredPosition || player.nationality) && (
                      <span className="text-outline">•</span>
                    )}
                    <span className="inline-flex items-center gap-1.5">
                      <span className="w-5 h-5 rounded-full bg-surface-container-high flex items-center justify-center font-data-mono text-[10px] text-primary font-bold">
                        {player.currentTeam.teamName.slice(0, 3).toUpperCase()}
                      </span>
                      <span className="text-on-surface">
                        {player.currentTeam.teamName}
                      </span>
                    </span>
                  </>
                )}
              </div>
              <ProfileLoanLine
                currentTeam={player.currentTeam}
                parentTeam={player.parentTeam}
                loanEndDate={player.loanEndDate}
              />
            </div>
          </div>
          <div className="flex items-center gap-4 sm:gap-6 self-start lg:self-auto bg-surface-container/60 p-4 rounded-lg">
            <div className="flex flex-col">
              <span className="font-label-caps text-label-caps text-outline uppercase tracking-wider">
                Market Value
              </span>
              <span className="font-data-mono text-headline-md text-on-surface font-semibold mt-0.5">
                {formatMarketValue(player.contract.marketValue)}
              </span>
            </div>
            <div className="w-px h-10 bg-surface-container-highest" />
            <div className="flex flex-col min-w-[100px]">
              <span className="font-label-caps text-label-caps text-outline uppercase tracking-wider">
                Contract Until
              </span>
              <span className="font-data-mono text-headline-md text-on-surface-variant font-semibold mt-0.5">
                {player.contract.contractUntil
                  ? formatLoanDate(player.contract.contractUntil)
                  : "—"}
              </span>
            </div>
            <div className="w-px h-10 bg-surface-container-highest hidden sm:block" />
            <div className="flex-col hidden sm:flex">
              <span className="font-label-caps text-label-caps text-outline uppercase tracking-wider">
                Age
              </span>
              <span className="font-data-mono text-headline-md text-on-surface font-semibold mt-0.5">
                {formatAge(player.dateOfBirth)}
              </span>
              <span className="font-label-caps text-[10px] text-secondary mt-0.5">
                {player.dateOfBirth
                  ? formatLoanDate(player.dateOfBirth)
                  : "—"}
              </span>
            </div>
          </div>
        </div>
        {player.status.injuryDescription && (
          <p className="relative z-10 mt-4 rounded bg-error-container/20 border border-error-container px-3 py-2 font-body-sm text-body-sm text-error">
            {player.status.injuryDescription}
          </p>
        )}
      </div>

      {/* Match Appearances */}
      <div className="flex flex-col gap-4">
        <div className="bg-surface-container-low rounded-xl p-5 shadow-sm flex flex-col gap-4">
          <div className="flex flex-col sm:flex-row sm:items-center justify-between gap-2 pb-2">
            <div>
              <h2 className="font-headline-sm text-headline-sm text-on-surface">
                Match Appearances
              </h2>
              <p className="font-body-sm text-body-sm text-on-surface-variant mt-0.5">
                Match participation history
              </p>
            </div>
          </div>

          {appearances.isLoading && (
            <LoadingState label="Loading appearances..." />
          )}
          {appearances.error && <ErrorState error={appearances.error} />}

          {appearances.data && (
            <AppearanceList appearances={appearances.data.data} />
          )}

          {appearances.data && appearances.data.data.length > 0 && (
            <AppearancePagination
              page={page}
              pagination={appearances.data.pagination}
              shownCount={appearances.data.data.length}
              onPageChange={setPage}
            />
          )}
        </div>
      </div>
    </div>
  );
}

// Page-local footer per the approved prototype (the shared Pagination
// component stays untouched for other pages).
function AppearancePagination({
  page,
  pagination,
  shownCount,
  onPageChange,
}: {
  page: number;
  pagination: { page: number; totalItems: number; totalPages: number };
  shownCount: number;
  onPageChange: (page: number) => void;
}) {
  const { totalItems, totalPages } = pagination;
  const pillClass =
    "px-2 py-1 rounded bg-surface-container text-on-surface-variant hover:text-on-surface hover:bg-surface-container-high transition-colors font-body-sm text-[12px] disabled:opacity-40 disabled:cursor-not-allowed";

  return (
    <div className="flex flex-col sm:flex-row items-center justify-between gap-3 pt-3 text-secondary font-label-caps text-label-caps">
      <span>
        Showing {shownCount} of {totalItems} appearances
      </span>
      <div className="flex items-center gap-2">
        <button
          type="button"
          className={pillClass}
          disabled={page <= 1}
          onClick={() => onPageChange(page - 1)}
        >
          &lt; Previous
        </button>
        <span className="font-data-mono text-[12px] text-on-surface px-1">
          Page {page} of {totalPages}
        </span>
        <button
          type="button"
          className={pillClass}
          disabled={page >= totalPages}
          onClick={() => onPageChange(page + 1)}
        >
          Next &gt;
        </button>
      </div>
    </div>
  );
}

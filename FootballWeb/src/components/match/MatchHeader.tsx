import type { MatchResponse } from "../../types/match";
import { formatMatchDate, formatStatus } from "../../lib/format";

const TEAM_NAME = "Chelsea";

export function MatchHeader({ match }: { match: MatchResponse }) {
  const isHome = match.homeOrAway === "home";
  const homeName = isHome ? TEAM_NAME : match.opponentName;
  const awayName = isHome ? match.opponentName : TEAM_NAME;
  const homeScore = isHome ? match.scoreHome : match.scoreAway;
  const awayScore = isHome ? match.scoreAway : match.scoreHome;

  return (
    <div className="rounded-lg border border-surface-border bg-surface p-6">
      <div className="mb-4 flex flex-wrap items-center justify-between gap-2 text-sm text-ink-muted">
        <span>{match.competitionName ?? "Unknown competition"}</span>
        <span>{formatMatchDate(match.matchDate)}</span>
      </div>

      <div className="flex items-center justify-center gap-6 sm:gap-10">
        <TeamBlock name={homeName} />
        <div className="flex items-center gap-3 text-3xl font-bold tabular-nums text-ink">
          <span>{homeScore ?? "-"}</span>
          <span className="text-ink-faint">:</span>
          <span>{awayScore ?? "-"}</span>
        </div>
        <TeamBlock name={awayName} />
      </div>

      <div className="mt-4 flex justify-center">
        <span className="inline-block rounded bg-brand-50 px-3 py-1 text-xs font-medium text-brand-600">
          {formatStatus(match.status)}
        </span>
      </div>
    </div>
  );
}

function TeamBlock({ name }: { name: string }) {
  return (
    <div className="flex w-24 flex-col items-center text-center sm:w-32">
      <div className="mb-1 flex h-10 w-10 items-center justify-center rounded-full bg-brand-100 text-sm font-semibold text-brand-600">
        {name.slice(0, 2).toUpperCase()}
      </div>
      <span className="text-sm font-medium text-ink">{name}</span>
    </div>
  );
}

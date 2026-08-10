import { Link } from "react-router-dom";
import type { MatchResponse } from "../../types/match";
import { formatMatchDate, formatStatus } from "../../lib/format";

const TEAM_NAME = "Chelsea"; // single-team scope; teamId is the tracked team

export function MatchCard({ match }: { match: MatchResponse }) {
  const isHome = match.homeOrAway === "home";
  const homeName = isHome ? TEAM_NAME : match.opponentName;
  const awayName = isHome ? match.opponentName : TEAM_NAME;
  const homeScore = isHome ? match.scoreHome : match.scoreAway;
  const awayScore = isHome ? match.scoreAway : match.scoreHome;

  return (
    <Link
      to={`/matches/${match.matchId}`}
      className="block rounded-lg border border-surface-border bg-surface p-4 transition-shadow hover:shadow-sm"
    >
      <div className="mb-2 flex items-center justify-between text-xs text-ink-muted">
        <span>{match.competitionName ?? "Unknown competition"}</span>
        <span>{formatMatchDate(match.matchDate)}</span>
      </div>

      <div className="space-y-1">
        <Row name={homeName} score={homeScore} emphasize={isHome} />
        <Row name={awayName} score={awayScore} emphasize={!isHome} />
      </div>

      <div className="mt-2">
        <span className="inline-block rounded bg-brand-50 px-2 py-0.5 text-xs font-medium text-brand-600">
          {formatStatus(match.status)}
        </span>
      </div>
    </Link>
  );
}

function Row({
  name,
  score,
  emphasize,
}: {
  name: string;
  score: number | null;
  emphasize: boolean;
}) {
  return (
    <div className="flex items-center justify-between">
      <span className={emphasize ? "font-semibold text-ink" : "text-ink"}>
        {name}
      </span>
      <span className="tabular-nums text-ink">{score ?? "-"}</span>
    </div>
  );
}

import type { MatchResponse } from "../../types/match";
import { formatMatchDate, formatStatus } from "../../lib/format";

const OUR_TEAM_NAME = "Chelsea";
const OUR_TEAM_CODE = "CHE";

function fallbackCode(name: string): string {
  return name.slice(0, 3).toUpperCase();
}

export function MatchHeader({ match }: { match: MatchResponse }) {
  const isHome = match.homeOrAway === "home";
  const homeName = isHome ? OUR_TEAM_NAME : match.opponentName;
  const awayName = isHome ? match.opponentName : OUR_TEAM_NAME;
  const homeCode = isHome ? OUR_TEAM_CODE : fallbackCode(match.opponentName);
  const awayCode = isHome ? fallbackCode(match.opponentName) : OUR_TEAM_CODE;
  const homeScore = isHome ? match.scoreHome : match.scoreAway;
  const awayScore = isHome ? match.scoreAway : match.scoreHome;

  return (
    <div className="relative rounded bg-surface-container p-6">
      <div className="absolute right-6 top-6">
        <span className="flex items-center gap-1.5 rounded-full bg-surface-variant px-3 py-1 font-label-caps uppercase text-on-surface-variant">
          <span className="h-1.5 w-1.5 rounded-full bg-on-surface-variant" />
          {formatStatus(match.status)}
        </span>
      </div>

      <div className="flex flex-wrap items-center gap-2 font-label-caps uppercase text-on-surface-variant">
        <span className="rounded bg-surface-variant px-2 py-1">
          {match.competitionName ?? "Unknown competition"}
        </span>
        <span className="normal-case tracking-normal">
          {formatMatchDate(match.matchDate)}
        </span>
      </div>

      <div className="mt-6 flex items-center justify-center gap-8 sm:gap-12">
        <TeamBlock name={homeName} code={homeCode} variant="home" />
        <div className="flex items-center gap-3 font-display-lg tabular-nums text-on-surface">
          <span>{homeScore ?? "-"}</span>
          <span className="text-outline">-</span>
          <span>{awayScore ?? "-"}</span>
        </div>
        <TeamBlock name={awayName} code={awayCode} variant="away" />
      </div>
    </div>
  );
}

function TeamBlock({
  name,
  code,
  variant,
}: {
  name: string;
  code: string;
  variant: "home" | "away";
}) {
  const badgeClasses =
    variant === "home"
      ? "bg-primary text-on-primary"
      : "bg-secondary-container text-on-secondary-container";

  return (
    <div className="flex w-24 flex-col items-center gap-2 text-center sm:w-32">
      <div
        className={`flex h-14 w-14 items-center justify-center rounded-lg font-data-mono text-[15px] font-bold ${badgeClasses}`}
      >
        {code}
      </div>
      <span className="font-body-md font-medium text-on-surface">{name}</span>
    </div>
  );
}

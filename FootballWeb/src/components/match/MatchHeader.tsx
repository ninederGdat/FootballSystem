import type { MatchResponse } from "../../types/match";
import {
  formatMatchDate,
  formatShootout,
  formatStatus,
  matchOutcome,
} from "../../lib/format";
import { getTrackedTeam } from "../../lib/teams";

function fallbackCode(name: string): string {
  return name.slice(0, 3).toUpperCase();
}

const OUTCOME_STYLES: Record<string, string> = {
  WIN: "bg-green-500/10 text-green-400",
  LOSS: "bg-red-500/10 text-red-400",
  DRAW: "bg-surface-variant text-on-surface-variant",
};

const OUTCOME_DOT: Record<string, string> = {
  WIN: "bg-green-400",
  LOSS: "bg-red-400",
  DRAW: "bg-on-surface-variant",
};

export function MatchHeader({ match }: { match: MatchResponse }) {
  const ours = match.teamId != null ? getTrackedTeam(match.teamId) : undefined;
  const ourName = ours?.teamName ?? "Unknown";
  const ourCode = ours?.abbr ?? "UNK";
  const isHome = match.homeOrAway === "home";
  const homeName = isHome ? ourName : match.opponentName;
  const awayName = isHome ? match.opponentName : ourName;
  const homeCode = isHome ? ourCode : fallbackCode(match.opponentName);
  const awayCode = isHome ? fallbackCode(match.opponentName) : ourCode;
  // score_home / score_away đã theo thứ tự chủ nhà – khách, cùng thứ tự với hai khối tên đội.
  const homeScore = match.scoreHome;
  const awayScore = match.scoreAway;
  const shootout = formatShootout(match.penaltyHome, match.penaltyAway);

  const outcome =
    match.scoreHome !== null &&
    match.scoreAway !== null &&
    match.status === "FINISHED"
      ? matchOutcome(
          match.scoreHome,
          match.scoreAway,
          match.homeOrAway,
          match.penaltyHome,
          match.penaltyAway,
        )
      : null;

  return (
    <div className="relative overflow-hidden rounded bg-surface-container p-6">
      <div className="pointer-events-none absolute inset-0 bg-gradient-to-r from-primary-container/10 via-transparent to-transparent" />

      <div className="relative flex flex-col gap-5">
        <div className="flex items-center justify-between gap-3">
          <div className="flex min-w-0 items-center gap-2 overflow-hidden">
            <span className="font-label-caps text-label-caps font-bold uppercase tracking-wider text-on-surface-variant">
              {match.competitionName ?? "Unknown competition"}
            </span>
            <span className="text-outline">•</span>
            <span className="font-body-sm text-body-sm text-outline">
              {formatMatchDate(match.matchDate)}
            </span>
          </div>
          <span className="shrink-0 rounded-full bg-surface-container-highest px-2.5 py-0.5 font-label-caps text-label-caps font-bold uppercase tracking-wider text-on-surface">
            {formatStatus(match.status)}
          </span>
        </div>

        <div className="grid grid-cols-12 items-center gap-4 py-1">
          <div className="col-span-12 flex justify-center md:col-span-5 md:justify-end">
            <TeamBlock
              name={homeName}
              code={homeCode}
              tag="HOME"
              focused={!!ours && isHome}
              nameFirst
            />
          </div>

          <div className="col-span-12 flex flex-col items-center justify-center md:col-span-2">
            <div className="flex items-center gap-3 rounded-lg bg-surface-container-lowest px-6 py-2">
              <span className="font-display-lg text-display-lg font-bold text-on-surface">
                {homeScore ?? "-"}
              </span>
              <span className="font-data-mono text-headline-sm text-outline">
                –
              </span>
              <span className="font-display-lg text-display-lg font-bold text-on-surface">
                {awayScore ?? "-"}
              </span>
            </div>
            {shootout && (
              <div className="mt-1.5 font-label-caps text-label-caps font-bold uppercase tracking-wider text-on-surface-variant">
                Pens {shootout}
              </div>
            )}
            {outcome && (
              <div
                className={`mt-2 flex items-center gap-1.5 rounded-full px-2.5 py-0.5 font-label-caps text-label-caps font-bold ${
                  OUTCOME_STYLES[outcome] ?? OUTCOME_STYLES.DRAW
                }`}
              >
                <span
                  className={`h-1.5 w-1.5 rounded-full ${
                    OUTCOME_DOT[outcome] ?? OUTCOME_DOT.DRAW
                  }`}
                />
                {outcome}
                {shootout && outcome !== "DRAW" ? " (PEN)" : ""}
              </div>
            )}
          </div>

          <div className="col-span-12 flex justify-center md:col-span-5 md:justify-start">
            <TeamBlock
              name={awayName}
              code={awayCode}
              tag="AWAY"
              focused={!!ours && !isHome}
              nameFirst={false}
            />
          </div>
        </div>
      </div>
    </div>
  );
}

function TeamBlock({
  name,
  code,
  tag,
  focused,
  nameFirst,
}: {
  name: string;
  code: string;
  tag: "HOME" | "AWAY";
  focused: boolean;
  nameFirst: boolean;
}) {
  const badge = (
    <div
      className={`flex h-14 w-14 shrink-0 items-center justify-center rounded-full font-data-mono text-headline-sm font-bold shadow-md ring-1 ring-outline-variant ${
        focused
          ? "bg-primary-container text-on-primary"
          : "bg-secondary-container text-on-secondary-container"
      }`}
    >
      {code}
    </div>
  );

  const label = (
    <div className={nameFirst ? "text-left md:text-right" : "text-left"}>
      <p className="font-body-sm text-body-sm font-semibold text-on-surface">
        {name}
      </p>
      <p className="font-label-caps text-label-caps text-outline">{tag}</p>
    </div>
  );

  return (
    <div className="flex items-center gap-4">
      {nameFirst ? (
        <>
          {label}
          {badge}
        </>
      ) : (
        <>
          {badge}
          {label}
        </>
      )}
    </div>
  );
}

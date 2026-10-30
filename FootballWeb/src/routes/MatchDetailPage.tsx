import type { ReactNode } from "react";
import { Link, useParams } from "react-router-dom";
import { useMatch } from "../features/matches/hooks";
import { MatchTimeline } from "../components/match/MatchTimeline";
import { LoadingState, ErrorState } from "../components/common/States";
import { MatchHeader } from "../components/match/MatchHeader";
import { Formation, SubstitutesList } from "../components/match/Lineup";
import { getTrackedTeam } from "../lib/teams";

function fallbackCode(name: string): string {
  return name.slice(0, 3).toUpperCase();
}

export function MatchDetailPage() {
  const { matchId } = useParams<{ matchId: string }>();
  const id = Number(matchId);

  const { data, isLoading, error } = useMatch(id);

  if (!Number.isFinite(id)) {
    return <ErrorState error={new Error("Invalid match ID in URL.")} />;
  }

  if (isLoading) return <LoadingState label="Loading match..." />;
  if (error) return <ErrorState error={error} />;
  if (!data) return null;

  const ours = data.teamId != null ? getTrackedTeam(data.teamId) : undefined;
  const ourCode = ours?.abbr ?? "UNK";
  const isHome = data.homeOrAway === "home";
  const homeCode = isHome ? ourCode : fallbackCode(data.opponentName);
  const awayCode = isHome ? fallbackCode(data.opponentName) : ourCode;

  const substitutes = data.lineup?.substitutes ?? [];
  const subsUsed = substitutes.filter((s) => s.minuteIn !== null).length;

  const tacticalTitle = ours
    ? `${ours.teamName.toUpperCase()} · TACTICAL SETUP`
    : "TACTICAL SETUP";

  return (
    <div className="space-y-6">
      <nav className="flex items-center gap-1.5 font-label-caps uppercase text-on-surface-variant">
        <Link to="/matches" className="hover:text-on-surface">
          Matches
        </Link>
        <span>/</span>
        <span>{data.competitionName ?? "Unknown competition"}</span>
        <span>/</span>
        <span className="text-on-surface">
          {homeCode} vs {awayCode}
        </span>
      </nav>

      <MatchHeader match={data} />

      <div className="grid grid-cols-1 gap-6 xl:grid-cols-12">
        <div className="flex flex-col gap-6 xl:col-span-7">
          <MatchPanel
            title={tacticalTitle}
            icon="strategy"
            right={
              data.lineup?.formation && (
                <span className="shrink-0 rounded bg-surface-container-lowest px-2.5 py-1 font-data-mono text-data-mono text-primary">
                  Formation: {data.lineup.formation.name}
                </span>
              )
            }
          >
            <Formation lineup={data.lineup} />
          </MatchPanel>

          <MatchPanel
            title="Substitutes"
            icon="group"
            right={
              <span className="font-label-caps text-label-caps uppercase text-outline">
                {substitutes.length} nominated / {subsUsed} used
              </span>
            }
          >
            <SubstitutesList substitutes={substitutes} />
          </MatchPanel>
        </div>

        <div className="xl:col-span-5">
          <MatchPanel title="Match Events" icon="schedule">
            <MatchTimeline events={data.events} match={data} />
          </MatchPanel>
        </div>
      </div>
    </div>
  );
}

function MatchPanel({
  title,
  icon,
  right,
  children,
}: {
  title: string;
  icon: string;
  right?: ReactNode;
  children: ReactNode;
}) {
  return (
    <section className="overflow-hidden rounded border border-outline-variant bg-surface-container">
      <header className="flex items-center justify-between gap-3 border-b border-outline-variant bg-surface-container-high px-4 py-3">
        <div className="flex items-center gap-2">
          <span className="material-symbols-outlined text-[18px] text-primary">
            {icon}
          </span>
          <h3 className="font-headline-sm text-headline-sm font-bold tracking-tight text-on-surface">
            {title}
          </h3>
        </div>
        {right}
      </header>
      <div className="p-4">{children}</div>
    </section>
  );
}

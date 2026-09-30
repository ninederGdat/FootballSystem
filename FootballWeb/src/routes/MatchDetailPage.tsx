import { Link, useParams } from "react-router-dom";
import { useMatch } from "../features/matches/hooks";
import { MatchTimeline } from "../components/match/MatchTimeline";
import { SectionCard } from "../components/common/SectionCard";
import { LoadingState, ErrorState } from "../components/common/States";
import { MatchHeader } from "../components/match/MatchHeader";
import { Formation, SubstitutesList } from "../components/match/Lineup";

const OUR_TEAM_CODE = "CHE";

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

  const isHome = data.homeOrAway === "home";
  const homeCode = isHome ? OUR_TEAM_CODE : fallbackCode(data.opponentName);
  const awayCode = isHome ? fallbackCode(data.opponentName) : OUR_TEAM_CODE;

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

      <div className="grid grid-cols-1 gap-6 lg:grid-cols-3">
        <div className="lg:col-span-2">
          <SectionCard
            title="Tactical Setup"
            icon="strategy"
            right={
              data.lineup?.formation && (
                <span className="font-body-sm text-on-surface-variant">
                  Formation: {data.lineup.formation.name}
                </span>
              )
            }
          >
            <Formation lineup={data.lineup} />
          </SectionCard>
        </div>

        <div className="flex flex-col gap-6">
          <SectionCard title="Substitutes">
            <SubstitutesList substitutes={data.lineup?.substitutes ?? []} />
          </SectionCard>

          <SectionCard title="Match Events">
            <MatchTimeline events={data.events} />
          </SectionCard>
        </div>
      </div>
    </div>
  );
}

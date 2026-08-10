import { useParams } from "react-router-dom";
import { useMatch } from "../features/matches/hooks"; 
import { MatchTimeline } from "../components/match/MatchTimeline"; 
import { Lineup } from "../components/match/Lineup"; 
import { LoadingState, ErrorState } from "../components/common/States";
import { MatchHeader } from "../components/match/MatchHeader";

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

  return (
    <div className="space-y-6">
      <MatchHeader match={data} />

      <section>
        <h2 className="mb-3 text-sm font-semibold uppercase tracking-wide text-ink-muted">
          Lineup
        </h2>
        <Lineup lineup={data.lineup} />
      </section>

      <section>
        <h2 className="mb-3 text-sm font-semibold uppercase tracking-wide text-ink-muted">
          Timeline
        </h2>
        <MatchTimeline events={data.events} />
      </section>
    </div>
  );
}

import { useState } from "react";
import { useMatch } from "../features/matches/hooks"; 
import { MatchCard } from "../components/match/MatchCard";
import { LoadingState, ErrorState } from "../components/common/States";

/**
 * IMPLEMENTATION NOTE (backend gap, see Step 18 of the brief):
 *
 * FootballApi's MatchesController only exposes:
 *   GET /api/matches/{matchId}
 *
 * There is no GET /api/matches (list) endpoint, so a real Match List page
 * can't be built against actual data yet. Rather than invent a fake list
 * endpoint or hardcode match IDs as if they were real data, this page:
 *
 *   1. Explains the gap.
 *   2. Lets you open a match by ID (using the endpoint that DOES exist),
 *      so Match Detail / Lineup / Timeline can be built and tested now.
 *
 * Suggested smallest backend addition (backwards compatible - purely
 * additive, doesn't touch GetMatch):
 *
 *   GET /api/matches?teamId=8455&page=1&pageSize=20
 *     -> paginated list of MatchClean projected to a light MatchSummaryResponse
 *        (matchId, opponentName, competitionName, matchDate, homeOrAway,
 *        scoreHome, scoreAway, status) - i.e. MatchResponse without
 *        events/lineup, since the list view doesn't need those.
 *
 * Once that endpoint exists, replace the body of this component with a
 * paginated fetch + `<MatchCard>` grid, same pattern as PlayerListPage.
 */
export function MatchListPage() {
  const [matchIdInput, setMatchIdInput] = useState("");
  const [lookupId, setLookupId] = useState<number | null>(null);

  return (
    <div className="space-y-6">
      <div>
        <h1 className="text-xl font-semibold text-ink">Matches</h1>
        <p className="mt-1 text-sm text-ink-muted">
          There's no match list endpoint on the backend yet (
          <code className="rounded bg-surface-subtle px-1 py-0.5">
            GET /api/matches
          </code>{" "}
          is missing) — only lookup by ID. Enter a known match ID to preview
          the Match Detail page below.
        </p>
      </div>

      <form
        className="flex gap-2"
        onSubmit={(e) => {
          e.preventDefault();
          const id = Number(matchIdInput);
          if (Number.isFinite(id) && id > 0) setLookupId(id);
        }}
      >
        <input
          value={matchIdInput}
          onChange={(e) => setMatchIdInput(e.target.value)}
          placeholder="e.g. 4685783"
          className="w-56 rounded-md border border-surface-border bg-surface px-3 py-2 text-sm outline-none focus:border-brand-500"
        />
        <button
          type="submit"
          className="rounded-md bg-brand-500 px-4 py-2 text-sm font-medium text-white hover:bg-brand-600"
        >
          Open match
        </button>
      </form>

      {lookupId !== null && <MatchPreview matchId={lookupId} />}
    </div>
  );
}

function MatchPreview({ matchId }: { matchId: number }) {
  const { data, isLoading, error } = useMatch(matchId);

  if (isLoading) return <LoadingState label="Loading match..." />;
  if (error) return <ErrorState error={error} />;
  if (!data) return null;

  return (
    <div className="max-w-sm">
      <MatchCard match={data} />
    </div>
  );
}

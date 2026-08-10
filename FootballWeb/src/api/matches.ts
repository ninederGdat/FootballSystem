import { apiGet } from "./client";
import type { MatchResponse } from "../types/match";

// NOTE: MatchesController only exposes GET /api/matches/{matchId:long}.
// There is no list endpoint (no GET /api/matches). The Match List page
// cannot be built against real data until one exists - see the
// implementation note surfaced in MatchListPage.tsx and the chat response
// for the suggested backend addition.

export function getMatch(matchId: number, signal?: AbortSignal) {
  return apiGet<MatchResponse>(`/api/matches/${matchId}`, undefined, {
    signal,
  });
}

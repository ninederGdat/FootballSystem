import { apiGet } from "./client";
import type {
  MatchResponse,
  MatchSearchQuery,
  MatchSearchResult,
} from "../types/match";

export function getMatch(matchId: number, signal?: AbortSignal) {
  return apiGet<MatchResponse>(`/api/matches/${matchId}`, undefined, {
    signal,
  });
}

export function searchMatches(query: MatchSearchQuery, signal?: AbortSignal) {
  return apiGet<MatchSearchResult>(
    "/api/matches",
    {
      fromDate: query.fromDate,
      toDate: query.toDate,
      // BUGFIX: `season` was defined on MatchSearchQuery and consumed by
      // MatchesController/MatchService.SearchMatchesAsync via
      // ISeasonService.Resolve, but was never actually forwarded here - the
      // season dropdown on MatchListPage was silently a no-op against the API.
      season: query.season,
      opponent: query.opponent,
      status: query.status,
      page: query.page ?? 1,
      pageSize: query.pageSize ?? 20,
    },
    { signal },
  );
}

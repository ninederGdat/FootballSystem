import { keepPreviousData, useQuery } from "@tanstack/react-query";
import { getMatch, searchMatches } from "../../api/matches";
import { MatchSearchQuery } from "../../types/match";

export function useMatch(matchId: number) {
  return useQuery({
    queryKey: ["match", matchId],
    queryFn: ({ signal }) => getMatch(matchId, signal),
    enabled: Number.isFinite(matchId),
  });
}

export function matchesQueryKey(query: MatchSearchQuery) {
  return ["matches", query] as const;
}
 
export function useMatches(query: MatchSearchQuery) {
  return useQuery({
    queryKey: matchesQueryKey(query),
    queryFn: ({ signal }) => searchMatches(query, signal),
    placeholderData: keepPreviousData,
    staleTime: 30_000,
  });
}

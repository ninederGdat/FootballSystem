import { useQuery } from "@tanstack/react-query";
import { getMatch } from "../../api/matches";

export function useMatch(matchId: number) {
  return useQuery({
    queryKey: ["match", matchId],
    queryFn: ({ signal }) => getMatch(matchId, signal),
    enabled: Number.isFinite(matchId),
  });
}

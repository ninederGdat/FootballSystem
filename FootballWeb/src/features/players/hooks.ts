import { useQuery } from "@tanstack/react-query";
import { getPlayer, getPlayerAppearances, searchPlayers } from "../../api/players";
import type { PlayerSearchQuery } from "../../types/player";

export function usePlayer(playerId: number) {
  return useQuery({
    queryKey: ["player", playerId],
    queryFn: ({ signal }) => getPlayer(playerId, signal),
    enabled: Number.isFinite(playerId),
  });
}

export function usePlayerAppearances(
  playerId: number,
  page: number,
  pageSize = 20,
) {
  return useQuery({
    queryKey: ["player", playerId, "appearances", page, pageSize],
    queryFn: ({ signal }) =>
      getPlayerAppearances(playerId, page, pageSize, signal),
    enabled: Number.isFinite(playerId),
    placeholderData: (previous) => previous, // keep old page while fetching next
  });
}

export function usePlayerSearch(query: PlayerSearchQuery) {
  return useQuery({
    queryKey: ["players", "search", query],
    queryFn: ({ signal }) => searchPlayers(query, signal),
    placeholderData: (previous) => previous,
  });
}

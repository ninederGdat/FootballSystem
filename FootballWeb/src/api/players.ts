import { apiGet } from "./client";
import type {
  PlayerAppearancesResult,
  PlayerProfileDTO,
  PlayerSearchQuery,
  PlayerSearchResult,
} from "../types/player";

export function getPlayer(playerId: number, signal?: AbortSignal) {
  return apiGet<PlayerProfileDTO>(`/api/players/${playerId}`, undefined, {
    signal,
  });
}

export function getPlayerAppearances(
  playerId: number,
  page: number,
  pageSize: number,
  signal?: AbortSignal,
) {
  return apiGet<PlayerAppearancesResult>(
    `/api/players/${playerId}/appearances`,
    { page, pageSize },
    { signal },
  );
}

export function searchPlayers(query: PlayerSearchQuery, signal?: AbortSignal) {
  return apiGet<PlayerSearchResult>(
    "/api/players",
    {
      search: query.search,
      teamId: query.teamId,
      positionCode: query.positionCode,
      nationality: query.nationality,
      page: query.page ?? 1,
      pageSize: query.pageSize ?? 20,
    },
    { signal },
  );
}

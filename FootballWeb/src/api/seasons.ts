import { apiGet } from "./client";
import type { SeasonListResponse } from "../types/season";

// GET /api/seasons - SeasonsController.GetSeasons.
export function getSeasons(signal?: AbortSignal) {
  return apiGet<SeasonListResponse>("/api/seasons", undefined, { signal });
}
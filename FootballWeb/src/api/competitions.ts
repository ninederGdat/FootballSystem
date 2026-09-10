import { apiGet } from "./client";
import type { CompetitionListResponse } from "../types/competition";

// GET /api/competitions - CompetitionsController.GetCompetitions.
// No query params on the backend yet: returns every competition the system
// knows about (not scoped to matches Chelsea has actually played).
export function getCompetitions(signal?: AbortSignal) {
  return apiGet<CompetitionListResponse>("/api/competitions", undefined, {
    signal,
  });
}

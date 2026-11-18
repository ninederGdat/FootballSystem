import { apiGet } from "./client";
import type { SyncRunListResponse } from "../types/sync";

// GET /api/sync/runs - GitHub Actions sync history, newest first.
// Backend returns 400 when limit is outside 1..20, so clamp here.
export function getSyncRuns(limit = 5, signal?: AbortSignal) {
  return apiGet<SyncRunListResponse>(
    "/api/sync/runs",
    { limit: Math.min(Math.max(limit, 1), 20) },
    { signal },
  );
}

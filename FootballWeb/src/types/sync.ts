// Mirrors the SyncRunDTO returned by GET /api/sync/runs (camelCase on the wire).

export type SyncRunStatus =
  | "running"
  | "success"
  | "partial"
  | "failed"
  | "interrupted";

export interface SyncRun {
  id: number;
  status: SyncRunStatus;
  startedAt: string;
  finishedAt: string | null;
  durationSeconds: number | null;
  trigger: string | null;
  runUrl: string | null;
  pendingStubs: number | null;
  failedPhases: string[];
}

export interface SyncRunListResponse {
  data: SyncRun[];
}

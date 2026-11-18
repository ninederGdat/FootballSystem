import { useCallback, useState } from "react";
import type { SyncRun } from "../../types/sync";
import { useSyncRuns } from "./hooks";

const LAST_SEEN_KEY = "footballsystem:sync:lastSeenRunId";

// Module-level fallback so useSyncRuns().data ?? EMPTY_RUNS is referentially
// stable - a fresh [] each render would re-run consumers' effects forever.
const EMPTY_RUNS: SyncRun[] = [];

function readLastSeenRunId(): number {
  try {
    const raw = window.localStorage.getItem(LAST_SEEN_KEY);
    const value = raw === null ? 0 : Number(raw);
    return Number.isFinite(value) ? value : 0;
  } catch {
    return 0;
  }
}

export function useSyncNotifications() {
  const query = useSyncRuns();
  const [lastSeenRunId, setLastSeenRunId] = useState(readLastSeenRunId);

  const runs = query.data?.data ?? EMPTY_RUNS;

  const isUnread = useCallback(
    (run: SyncRun) =>
      run.id > lastSeenRunId && run.status !== "running",
    [lastSeenRunId],
  );

  const unreadCount = runs.filter((run) => isUnread(run)).length;

  const markAllRead = useCallback(() => {
    const highest = runs
      .filter((run) => run.status !== "running")
      .reduce((max, run) => Math.max(max, run.id), lastSeenRunId);
    if (highest <= lastSeenRunId) return;
    setLastSeenRunId(highest);
    try {
      window.localStorage.setItem(LAST_SEEN_KEY, String(highest));
    } catch {
      // Storage unavailable (private mode, disabled, quota): the in-memory
      // state still marks everything read for this session.
    }
  }, [runs, lastSeenRunId]);

  return {
    runs,
    isLoading: query.isLoading,
    // A failed background refetch keeps the last good data; only a complete
    // failure (no data at all) is an error the bell should show.
    isError: query.isError && runs.length === 0,
    unreadCount,
    isUnread,
    markAllRead,
  };
}

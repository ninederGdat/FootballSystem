import { useQuery } from "@tanstack/react-query";
import { getSyncRuns } from "../../api/sync";

// Sync runs change slowly (GitHub Actions cadence) but finishing runs should
// surface reasonably soon: poll every 5 min, 1 min staleness, no background
// polling while the tab is hidden.
export function useSyncRuns() {
  return useQuery({
    queryKey: ["sync-runs"],
    queryFn: ({ signal }) => getSyncRuns(5, signal),
    staleTime: 60 * 1000,
    refetchInterval: 5 * 60 * 1000,
    refetchIntervalInBackground: false,
  });
}

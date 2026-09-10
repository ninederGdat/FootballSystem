import { useQuery } from "@tanstack/react-query";
import { getCompetitions } from "../../api/competitions";

// Competitions are effectively static reference data (Master Data group in
// business_logic.md) - long staleTime avoids refetching on every mount.
export function useCompetitions() {
  return useQuery({
    queryKey: ["competitions"],
    queryFn: ({ signal }) => getCompetitions(signal),
    staleTime: 60 * 60 * 1000,
  });
}

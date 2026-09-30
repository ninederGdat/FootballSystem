import { useQuery } from "@tanstack/react-query";
import { getSeasons } from "../../api/seasons";

export function useSeasons() {
  return useQuery({
    queryKey: ["seasons"],
    queryFn: ({ signal }) => getSeasons(signal),
    staleTime: 60 * 60 * 1000,
  });
}
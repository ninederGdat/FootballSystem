import { useQuery } from "@tanstack/react-query";
import { getTransferStatistics, getTransfers } from "../../api/transfers";
import type { TransferQueryParams } from "../../types/transfer";

export function useTransfers(params: TransferQueryParams) {
  return useQuery({
    queryKey: ["transfers", params],
    queryFn: () => getTransfers(params),
  });
}

export function useTransferStatistics({
  teamId,
  season,
  internalOnly,
}: {
  teamId: number;
  season?: string;
  internalOnly?: boolean;
}) {
  return useQuery({
    queryKey: ["transfer-statistics", teamId, season, internalOnly ?? false],
    queryFn: ({ signal }) =>
      getTransferStatistics({ teamId, season, internalOnly }, signal),
    enabled: Boolean(teamId && (season || internalOnly)),
  });
}

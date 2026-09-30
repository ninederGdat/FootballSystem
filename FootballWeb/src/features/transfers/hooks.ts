import { useQuery } from "@tanstack/react-query";
import { getTransferStatistics, getTransfers } from "../../api/transfers";
import type { TransferQueryParams } from "../../types/transfer";

export function useTransfers(params: TransferQueryParams) {
  return useQuery({
    queryKey: ["transfers", params],
    queryFn: () => getTransfers(params),
  });
}

export function useTransferStatistics(season?: string) {
  return useQuery({
    queryKey: ["transfer-statistics", season],
    queryFn: ({ signal }) => getTransferStatistics(season!, signal),
    enabled: Boolean(season),
  });
}

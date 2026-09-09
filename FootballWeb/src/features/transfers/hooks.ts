import { useQuery } from "@tanstack/react-query";
import { getTransfers } from "../../api/transfers";
import type { TransferQueryParams } from "../../types/transfer";

export function useTransfers(params: TransferQueryParams) {
  return useQuery({
    queryKey: ["transfers", params],
    queryFn: () => getTransfers(params),
  });
}

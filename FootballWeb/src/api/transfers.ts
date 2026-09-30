import { apiGet } from "./client";
import type {
  PagedResponse,
  TransferApiItem,
  TransferQueryParams,
  TransferStatistics,
} from "../types/transfer";

function buildQueryString(params: TransferQueryParams): string {
  const searchParams = new URLSearchParams();

  if (params.page !== undefined) searchParams.set("page", String(params.page));
  if (params.pageSize !== undefined)
    searchParams.set("pageSize", String(params.pageSize));
  if (params.season) searchParams.set("season", params.season);
  if (params.playerName) searchParams.set("playerName", params.playerName);
  if (params.playerId !== undefined)
    searchParams.set("playerId", String(params.playerId));
  if (params.fromClubId !== undefined)
    searchParams.set("fromClubId", String(params.fromClubId));
  if (params.toClubId !== undefined)
    searchParams.set("toClubId", String(params.toClubId));
  if (params.onLoan !== undefined)
    searchParams.set("onLoan", String(params.onLoan));
  if (params.contractExtension !== undefined)
    searchParams.set("contractExtension", String(params.contractExtension));
  if (params.transferType)
    searchParams.set("transferType", params.transferType);
  if (params.dateFrom) searchParams.set("dateFrom", params.dateFrom);
  if (params.dateTo) searchParams.set("dateTo", params.dateTo);

  return searchParams.toString();
}

// GET /api/transfers — see TransfersController.SearchHistoriesTransfers
export async function getTransfers(
  params: TransferQueryParams,
): Promise<PagedResponse<TransferApiItem>> {
  const qs = buildQueryString(params);
  return apiGet<PagedResponse<TransferApiItem>>(
    `/api/transfers${qs ? `?${qs}` : ""}`,
  );
}

// GET /api/transfers/statistics — seasonal totals for a team.
export function getTransferStatistics(
  season: string,
  signal?: AbortSignal,
): Promise<TransferStatistics> {
  return apiGet<TransferStatistics>(
    "/api/transfers/statistics",
    { season },
    { signal },
  );
}

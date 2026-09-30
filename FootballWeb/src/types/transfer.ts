// Mirrors FootballApi.DTOs.Responses.TransferResponse (camelCase via System.Text.Json defaults)
export interface TransferApiItem {
  playerId: number;
  playerName: string;

  fromClubId: number;
  fromClubName: string;

  toClubId: number;
  toClubName: string;

  transferDate: string; // ISO date string

  fromDate: string | null;
  toDate: string | null;

  // Backend enum values are "contract" | "on_loan" — NOT "permanent" | "loan" | "free".
  // "Free transfer" is not a distinct type; it's a "contract" transfer with fee === null.
  transferType: "contract" | "on_loan" | string;

  onLoan: boolean;
  contractExtension: boolean;

  fee: number | null;
}

export interface TransferStatistics {
  teamId: number;
  teamName: string;
  season: string;
  totalFeeToBuy: number;
  totalFeeToSell: number;
  netSpend: number;
  permanentBuyCount: number;
  permanentSellCount: number;
  loanInCount: number;
  loanOutCount: number;
}

// Mirrors PagedResponse<T> / PaginationMetadata from PagedResponse.cs
export interface PaginationMetadata {
  page: number;
  pageSize: number;
  totalItems: number;
  totalPages: number;
}

export interface PagedResponse<T> {
  data: T[];
  pagination: PaginationMetadata;
}

// Mirrors FootballApi.DTOs.Transfers.TransferQuery (query string params)
export interface TransferQueryParams {
  page?: number;
  pageSize?: number;
  season?: string;
  playerName?: string;
  playerId?: number;
  fromClubId?: number;
  toClubId?: number;
  onLoan?: boolean;
  contractExtension?: boolean;
  transferType?: string;
  dateFrom?: string; // ISO date
  dateTo?: string; // ISO date
}

// UI-only derived direction — NOT part of the API response.
// Derive with: toClubId === CHELSEA_TEAM_ID ? 'in' : 'out'
export type TransferDirection = "in" | "out";

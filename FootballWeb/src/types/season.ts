export interface SeasonSummaryResponse {
  code: string;
  name: string;
}

export interface SeasonListResponse {
  data: SeasonSummaryResponse[];
}
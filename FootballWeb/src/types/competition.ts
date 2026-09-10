// Mirrors FootballApi/DTOs/Competitions/CompetitionSummaryDto.cs
// System.Text.Json default naming policy -> camelCase on the wire.

export interface CompetitionSummaryDTO {
  id: number;
  name: string;
}

export interface CompetitionListResponse {
  data: CompetitionSummaryDTO[];
}

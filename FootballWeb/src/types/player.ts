// Mirrors FootballApi/DTOs/Players/*.cs exactly.

export interface PlayerPositionDTO {
  positionCode: string;
  positionName: string;
}

export interface PlayerStatusDTO {
  status: string; // "Active" | "Injured" | ...
  injuryDescription: string | null;
}

// Backend PlayerRelation enum -> camelCase JSON values.
export type PlayerRelation =
  | "current"
  | "loaned_in"
  | "loaned_out"
  | "transferred";

export interface PlayerTeamDTO {
  teamId: number;
  teamName: string;
  logoUrl: string | null;
}

export interface PlayerContractDTO {
  contractUntil: string | null; // DateOnly -> "YYYY-MM-DD"
  marketValue: number | null;
}

export interface PlayerProfileDTO {
  playerId: number;
  name: string;
  shirtNumber: number | null;
  dateOfBirth: string | null; // DateOnly -> "YYYY-MM-DD"
  nationality: string | null;
  status: PlayerStatusDTO;
  currentTeam: PlayerTeamDTO | null;
  preferredPosition: PlayerPositionDTO | null;
  parentTeam: PlayerTeamDTO | null; // only when on loan; the owning club
  loanEndDate: string | null; // "YYYY-MM-DD", only when on loan
  transferStatus?: PlayerTransferStatusDTO | null; // informational only, never rendered
  contract: PlayerContractDTO;
}

export interface PlayerTransferStatusDTO {
  currentTeam: string;
  status: string;
  periodEnd: string | null;
}

export interface PlayerAppearanceDTO {
  matchId: number;
  matchDate: string;
  playedForTeam: PlayerTeamDTO | null;
  opponentName: string | null;
  competitionName: string | null;
  isStarter: boolean;
  positionPlayed: PlayerPositionDTO;
  roleId: number | null;
  roleName: string | null;
  roleShort: string | null;
  minuteIn: number | null;
  minuteOut: number | null;
  goals: number;
  assists: number;
}

export interface PlayerSummaryDTO {
  playerId: number;
  name: string;
  currentTeam: PlayerTeamDTO | null;
  parentTeam: PlayerTeamDTO | null; // only when on loan
  relation: PlayerRelation;
  loanEndDate: string | null; // "YYYY-MM-DD", only when on loan
  positionCode: string | null;
  positionName: string | null;
  nationality: string | null;
  shirtNumber: number | null;
  age: number | null;
  status: string | null;
  injuryDescription: string | null;
  marketValue: number | null;
  contractUntil: string | null; // DateOnly -> "YYYY-MM-DD"
  transferStatus: string | null; // raw DB value, informational only
}

export interface PlayerSearchQuery {
  search?: string;
  teamId: number; // required by GET /api/players (400 when missing)
  positionCode?: string;
  nationality?: string;
  status?: "Healthy" | "Injured"; // DB values are capitalized; omitted = no status filter
  transferStatus?: PlayerRelation; // current | loaned_in | loaned_out | transferred
  page?: number;
  pageSize?: number;
}

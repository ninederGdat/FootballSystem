// Mirrors FootballApi/DTOs/Players/*.cs exactly.

export interface PlayerPositionDTO {
  positionCode: string;
  positionName: string;
}

export interface PlayerStatusDTO {
  status: string; // "Active" | "Injured" | ...
  injuryDescription: string | null;
}

export interface PlayerTeamDTO {
  teamId: number;
  teamName: string;
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
  contract: PlayerContractDTO;
}

export interface PlayerAppearanceDTO {
  matchId: number;
  matchDate: string;
  opponentName: string | null;
  competitionName: string | null;
  isStarter: boolean;
  positionPlayed: PlayerPositionDTO;
  roleId: number | null;
  roleName: string | null;
  roleShort: string | null;
  minuteIn: number | null;
  minuteOut: number | null;
}

export interface PlayerAppearancesResult {
  playerId: number;
  page: number;
  pageSize: number;
  totalCount: number;
  appearances: PlayerAppearanceDTO[];
}

export interface PlayerSummaryDTO {
  playerId: number;
  name: string;
  teamName: string | null;
  positionCode: string | null;
  positionName: string | null;
  nationality: string | null;
  shirtNumber: number | null;
}

export interface PlayerSearchResult {
  page: number;
  pageSize: number;
  totalCount: number;
  items: PlayerSummaryDTO[];
}

export interface PlayerSearchQuery {
  search?: string;
  teamId?: number;
  positionCode?: string;
  nationality?: string;
  page?: number;
  pageSize?: number;
}

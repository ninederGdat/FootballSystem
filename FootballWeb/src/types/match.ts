// Mirrors FootballApi/DTOs/Responses/MatchResponse.cs, LineupResponse.cs
// Field nullability follows the C# record/class exactly - do not widen or narrow.

export interface MatchEventPlayerRef {
  id: number | null;
  name: string;
}

// NOTE: MatchEventItemResponse.cs was not provided. This shape is inferred
// from the brief's example payload and business_logic.md (match_events
// table: team_id nullable SET NULL, player_id nullable SET NULL, raw_payload
// fallback for names). Verify against the real file when available.
export interface MatchEventItemResponse {
  id: number;
  eventId: number;
  type: string; // "goal" | "yellowCard" | "redCard" | ... - treat as open string, not a closed union
  minute: number;
  stoppageTime: number | null;
  teamId: number | null;
  player: MatchEventPlayerRef | null;
  assist: MatchEventPlayerRef | null;
  description: string | null;
}

export interface FormationDto {
  id: number;
  name: string;
  description: string;
}

export interface LineupPlayerResponse {
  playerId: number;
  playerName: string;
  positionCode: string | null;
  positionName: string | null;
  roleId: number | null;
  roleName: string | null;
  roleShort: string | null;
  shirtNumber: number | null;
  minuteIn: number | null;
  minuteOut: number | null;
  customX: number | null;
  customY: number | null;
}

export interface LineupResponse {
  lineupId: number;
  type: string; // "Predicted" | "Official"
  formation: FormationDto | null;
  starters: LineupPlayerResponse[];
  substitutes: LineupPlayerResponse[];
}

export interface MatchResponse {
  matchId: number;
  teamId: number | null;
  opponentTeamId: number | null;
  opponentName: string;
  competitionId: number | null;
  competitionName: string | null;
  matchDate: string; // ISO datetime string from the wire
  homeOrAway: string | null;
  scoreHome: number | null;
  scoreAway: number | null;
  status: string;
  events: MatchEventItemResponse[];
  lineup: LineupResponse | null;
}

export type MatchStatus = "UPCOMING" | "live" | "FINISHED" | "cancelled";

export interface MatchSummaryDTO {
  matchId: number;
  opponentName: string;
  opponentTeamId: number | null;
  competitionName: string;
  matchDate: string; // ISO 8601
  homeOrAway: "home" | "away";
  scoreHome: number | null;
  scoreAway: number | null;
  status: MatchStatus;
}

export interface MatchSearchQuery {
  fromDate?: string; // yyyy-MM-dd
  toDate?: string; // yyyy-MM-dd
  season?: string;
  opponent?: string;
  status?: MatchStatus;
  page?: number;
  pageSize?: number;
}

export interface MatchSearchResult {
  items: MatchSummaryDTO[];
  totalCount: number;
  season: string;
  page: number;
  pageSize: number;
}

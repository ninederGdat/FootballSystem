// TODO(backend) GET /api/teams is not implemented yet; the tracked-club list is
// hardcoded here until the backend exposes it.

export interface TrackedTeam {
  teamId: number;
  teamName: string;
  logoUrl: string | null;
}

export const TRACKED_TEAMS: TrackedTeam[] = [
  { teamId: 8455, teamName: "Chelsea", logoUrl: null },
  { teamId: 9848, teamName: "Strasbourg", logoUrl: null },
];

export const DEFAULT_TEAM_ID = 8455;

export function getTrackedTeam(teamId: number): TrackedTeam | undefined {
  return TRACKED_TEAMS.find((team) => team.teamId === teamId);
}
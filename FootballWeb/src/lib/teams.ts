// The tracked-club list is hardcoded. GET /api/teams exists but does not expose
// abbr or leagueName yet; switch to the API once those fields are available.

export interface TrackedTeam {
  teamId: number;
  teamName: string;
  abbr: string; // shown in the ClubSwitcher avatar
  leagueName: string;
}

export const TRACKED_TEAMS: TrackedTeam[] = [
  {
    teamId: 8455,
    teamName: "Chelsea",
    abbr: "CFC",
    leagueName: "Premier League",
  },
  { teamId: 9848, teamName: "Strasbourg", abbr: "RCSA", leagueName: "Ligue 1" },
];
export const DEFAULT_TEAM_ID = 8455;

export function getTrackedTeam(teamId: number): TrackedTeam | undefined {
  return TRACKED_TEAMS.find((team) => team.teamId === teamId);
}

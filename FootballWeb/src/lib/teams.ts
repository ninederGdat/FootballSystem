// TODO(backend) GET /api/teams is not implemented yet; the tracked-club list is
// hardcoded here until the backend exposes it.

export interface TrackedTeam {
  teamId: number;
  teamName: string;
  abbr: string; // hiện trong avatar của ClubSwitcher
  leagueName: string; // TODO(backend): lấy từ API khi có endpoint teams/competitions
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

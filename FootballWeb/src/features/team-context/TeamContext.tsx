import {
  createContext,
  useCallback,
  useContext,
  useMemo,
  useState,
  type ReactNode,
} from "react";
import {
  DEFAULT_TEAM_ID,
  TRACKED_TEAMS,
  type TrackedTeam,
} from "../../lib/teams";

const STORAGE_KEY = "footballsystem.contextTeamId";

function isTracked(id: number): boolean {
  return TRACKED_TEAMS.some((t) => t.teamId === id);
}

function readInitialTeamId(): number {
  try {
    const stored = Number(window.localStorage.getItem(STORAGE_KEY));
    if (isTracked(stored)) return stored;
  } catch {
    // storage unavailable (private mode, blocked): fall through to default
  }
  return DEFAULT_TEAM_ID;
}

interface TeamContextValue {
  teamId: number;
  team: TrackedTeam;
  setTeamId: (id: number) => void;
}

const TeamContext = createContext<TeamContextValue | null>(null);

export function TeamContextProvider({ children }: { children: ReactNode }) {
  const [teamId, setId] = useState(readInitialTeamId);

  const setTeamId = useCallback((id: number) => {
    if (!isTracked(id)) return;
    setId(id);
    try {
      window.localStorage.setItem(STORAGE_KEY, String(id));
    } catch {
      // ignore: the choice still applies for this session
    }
  }, []);

  const value = useMemo<TeamContextValue>(
    () => ({
      teamId,
      team: TRACKED_TEAMS.find((t) => t.teamId === teamId) ?? TRACKED_TEAMS[0],
      setTeamId,
    }),
    [teamId, setTeamId],
  );

  return <TeamContext.Provider value={value}>{children}</TeamContext.Provider>;
}

export function useContextTeam(): TeamContextValue {
  const value = useContext(TeamContext);
  if (!value) {
    throw new Error("useContextTeam must be used inside TeamContextProvider");
  }
  return value;
}

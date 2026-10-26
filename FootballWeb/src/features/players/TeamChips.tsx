import { TRACKED_TEAMS } from "../../lib/teams";

// Context-club switcher; the active teamId lives in the URL (?teamId=).
export function TeamChips({
  teamId,
  onChange,
}: {
  teamId: number;
  onChange: (teamId: number) => void;
}) {
  return (
    <div className="flex items-center gap-2">
      <span className="font-label-caps uppercase text-outline-variant">Team</span>
      {TRACKED_TEAMS.map((team) => (
        <button
          key={team.teamId}
          type="button"
          onClick={() => onChange(team.teamId)}
          className={
            team.teamId === teamId
              ? "px-3 py-1.5 rounded-DEFAULT bg-primary-container text-on-primary-container font-label-caps uppercase transition-colors"
              : "px-3 py-1.5 rounded-DEFAULT text-on-surface-variant hover:text-on-surface font-label-caps uppercase transition-colors"
          }
        >
          {team.teamName}
        </button>
      ))}
    </div>
  );
}

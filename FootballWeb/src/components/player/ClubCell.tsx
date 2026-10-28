import type { PlayerTeamDTO } from "../../types/player";

export function ClubCell({ team }: { team: PlayerTeamDTO | null }) {
  if (!team) return <span>—</span>;
  return (
    <span className="inline-flex items-center gap-2">
      {team.logoUrl && (
        <img
          src={team.logoUrl}
          alt={team.teamName}
          loading="lazy"
          className="w-4 h-4 rounded"
        />
      )}
      <span className="font-body-sm text-body-sm text-on-surface">
        {team.teamName}
      </span>
    </span>
  );
}

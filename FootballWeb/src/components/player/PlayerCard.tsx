import { Link } from "react-router-dom";
import type { PlayerSummaryDTO } from "../../types/player";

export function PlayerCard({ player }: { player: PlayerSummaryDTO }) {
  return (
    <Link
      to={`/players/${player.playerId}`}
      className="flex items-center gap-3 rounded-lg border border-surface-border bg-surface p-3 transition-shadow hover:shadow-sm"
    >
      <div className="flex h-10 w-10 shrink-0 items-center justify-center rounded-full bg-brand-100 text-sm font-semibold text-brand-600">
        {player.shirtNumber ?? "?"}
      </div>
      <div className="min-w-0">
        <p className="truncate font-medium text-ink">{player.name}</p>
        <p className="truncate text-xs text-ink-muted">
          {[player.positionName, player.teamName, player.nationality]
            .filter(Boolean)
            .join(" · ") || "—"}
        </p>
      </div>
    </Link>
  );
}

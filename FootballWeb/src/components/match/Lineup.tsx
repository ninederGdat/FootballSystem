import type { LineupPlayerResponse, LineupResponse } from "../../types/match";
import { EmptyState } from "../../components/common/States";

export function Lineup({ lineup }: { lineup: LineupResponse | null }) {
  if (!lineup) {
    return (
      <EmptyState
        title="No lineup available"
        description="Lineup data hasn't been published for this match yet."
      />
    );
  }

  return (
    <div className="space-y-6">
      <Formation lineup={lineup} />

      <div>
        <h3 className="mb-2 text-sm font-semibold text-ink">Substitutes</h3>
        {lineup.substitutes.length === 0 ? (
          <p className="text-sm text-ink-muted">No substitute data.</p>
        ) : (
          <ul className="divide-y divide-surface-border rounded-md border border-surface-border bg-surface">
            {lineup.substitutes.map((p) => (
              <PlayerRow key={p.playerId} player={p} />
            ))}
          </ul>
        )}
      </div>
    </div>
  );
}

function Formation({ lineup }: { lineup: LineupResponse }) {
  return (
    <div>
      <div className="mb-2 flex items-center justify-between">
        <h3 className="text-sm font-semibold text-ink">Starting XI</h3>
        {lineup.formation && (
          <span className="rounded bg-brand-50 px-2 py-0.5 text-xs font-medium text-brand-600">
            {lineup.formation.name}
          </span>
        )}
      </div>

      {lineup.starters.length === 0 ? (
        <p className="text-sm text-ink-muted">No starter data.</p>
      ) : (
        <div className="relative overflow-hidden rounded-lg bg-brand-500 p-4" style={{ minHeight: 360 }}>
          {lineup.starters.map((p) => (
            <FormationDot key={p.playerId} player={p} />
          ))}
        </div>
      )}
    </div>
  );
}

function FormationDot({ player }: { player: LineupPlayerResponse }) {
  // customX/Y are 0..1 pitch-relative coordinates when present. Fall back to
  // a simple centered stack so the UI degrades gracefully instead of piling
  // every player at the same spot.
  const hasCoords = player.customX !== null && player.customY !== null;
  const left = hasCoords ? `${(player.customX as number) * 100}%` : "50%";
  const top = hasCoords ? `${(player.customY as number) * 100}%` : "50%";

  return (
    <div
      className="absolute flex -translate-x-1/2 -translate-y-1/2 flex-col items-center"
      style={{ left, top }}
      title={player.roleName ?? player.positionName ?? undefined}
    >
      <div className="flex h-8 w-8 items-center justify-center rounded-full bg-white text-xs font-semibold text-brand-600 shadow">
        {player.shirtNumber ?? "?"}
      </div>
      <span className="mt-1 max-w-[72px] truncate text-center text-[11px] font-medium text-white">
        {player.playerName}
      </span>
    </div>
  );
}

function PlayerRow({ player }: { player: LineupPlayerResponse }) {
  return (
    <li className="flex items-center justify-between px-3 py-2 text-sm">
      <div className="flex items-center gap-2">
        <span className="w-6 shrink-0 text-center text-xs font-semibold text-ink-muted">
          {player.shirtNumber ?? "-"}
        </span>
        <span className="font-medium text-ink">{player.playerName}</span>
      </div>
      <div className="flex items-center gap-2 text-xs text-ink-muted">
        {player.positionCode && <span>{player.positionCode}</span>}
        {player.minuteIn !== null && (
          <span className="rounded bg-surface-subtle px-1.5 py-0.5">
            On {player.minuteIn}'
          </span>
        )}
      </div>
    </li>
  );
}

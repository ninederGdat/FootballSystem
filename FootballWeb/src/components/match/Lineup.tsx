import type { LineupPlayerResponse, LineupResponse } from "../../types/match";
import { EmptyState } from "../../components/common/States";

export function Formation({ lineup }: { lineup: LineupResponse | null }) {
  if (!lineup) {
    return (
      <EmptyState
        title="No lineup available"
        description="Lineup data hasn't been published for this match yet."
      />
    );
  }

  if (lineup.starters.length === 0) {
    return (
      <p className="font-body-sm text-on-surface-variant">No starter data.</p>
    );
  }

  const line = "rgba(255,255,255,0.3)";

  return (
    <div
      className="relative mx-auto w-full overflow-hidden rounded"
      style={{
        aspectRatio: "105 / 68", // sân ngang
        backgroundColor: "#1a3c25",
        backgroundImage:
          "linear-gradient(rgba(255,255,255,0.05) 1px, transparent 1px), linear-gradient(90deg, rgba(255,255,255,0.05) 1px, transparent 1px)",
        backgroundSize: "40px 40px",
      }}
    >
      {/* Đường biên */}
      <div
        className="pointer-events-none absolute inset-2"
        style={{ border: `2px solid ${line}` }}
      />
      {/* Đường giữa sân (dọc) */}
      <div
        className="pointer-events-none absolute bottom-2 left-1/2 top-2"
        style={{ borderLeft: `2px solid ${line}` }}
      />
      {/* Vòng tròn giữa sân */}
      <div
        className="pointer-events-none absolute left-1/2 top-1/2 aspect-square h-[27%] -translate-x-1/2 -translate-y-1/2 rounded-full"
        style={{ border: `2px solid ${line}` }}
      />
      {/* Chấm giữa sân */}
      <div
        className="pointer-events-none absolute left-1/2 top-1/2 h-1.5 w-1.5 -translate-x-1/2 -translate-y-1/2 rounded-full"
        style={{ backgroundColor: line }}
      />
      {/* Vòng cấm trái */}
      <div
        className="pointer-events-none absolute left-2 top-1/2 h-[59%] w-[16%] -translate-y-1/2"
        style={{ border: `2px solid ${line}`, borderLeft: "none" }}
      />
      {/* Vòng cấm phải */}
      <div
        className="pointer-events-none absolute right-2 top-1/2 h-[59%] w-[16%] -translate-y-1/2"
        style={{ border: `2px solid ${line}`, borderRight: "none" }}
      />
      {/* Chấm phạt đền */}
      <div
        className="pointer-events-none absolute top-1/2 h-1.5 w-1.5 -translate-y-1/2 rounded-full"
        style={{ left: "11%", backgroundColor: line }}
      />
      <div
        className="pointer-events-none absolute top-1/2 h-1.5 w-1.5 -translate-y-1/2 rounded-full"
        style={{ right: "11%", backgroundColor: line }}
      />

      {lineup.starters.map((p) => (
        <FormationDot key={p.playerId} player={p} />
      ))}
    </div>
  );
}

function FormationDot({ player }: { player: LineupPlayerResponse }) {
  const hasCoords = player.customX !== null && player.customY !== null;

  // x = chiều dài sân (GK ~0.1) -> left
  // y = chiều ngang sân -> top
  const left = hasCoords ? `${(player.customX as number) * 100}%` : "50%";
  const top = hasCoords ? `${(player.customY as number) * 100}%` : "50%";

  return (
    <div
      className="absolute flex -translate-x-1/2 -translate-y-1/2 flex-col items-center"
      style={{ left, top }}
      title={player.roleName ?? player.positionName ?? undefined}
    >
      <div className="flex h-8 w-8 items-center justify-center rounded-full bg-surface-container-highest font-data-mono text-on-surface shadow">
        {player.shirtNumber ?? "?"}
      </div>
      <span className="mt-1 max-w-[80px] truncate text-center font-label-caps text-on-surface">
        {player.playerName}
      </span>
    </div>
  );
}

export function SubstitutesList({
  substitutes,
}: {
  substitutes: LineupPlayerResponse[];
}) {
  if (substitutes.length === 0) {
    return (
      <p className="font-body-sm text-on-surface-variant">
        No substitute data.
      </p>
    );
  }

  return (
    <ul className="divide-y divide-outline-variant">
      {substitutes.map((p) => (
        <PlayerRow key={p.playerId} player={p} />
      ))}
    </ul>
  );
}

function PlayerRow({ player }: { player: LineupPlayerResponse }) {
  return (
    <li className="flex items-center justify-between py-2.5 font-body-sm">
      <div className="flex items-center gap-2.5">
        <span className="w-5 shrink-0 text-center font-data-mono text-on-surface-variant">
          {player.shirtNumber ?? "-"}
        </span>
        <span className="font-medium text-primary">{player.playerName}</span>
      </div>
      <div className="flex items-center gap-2">
        {player.positionCode && (
          <span className="rounded border border-outline-variant px-1.5 py-0.5 font-label-caps text-on-surface-variant">
            {player.positionCode}
          </span>
        )}
        {player.minuteIn !== null && (
          <span className="flex items-center gap-1 rounded bg-surface-variant px-1.5 py-0.5 font-label-caps uppercase text-on-surface-variant">
            <span className="material-symbols-outlined text-[14px]">
              sync_alt
            </span>
            {player.minuteIn}' IN
          </span>
        )}
      </div>
    </li>
  );
}

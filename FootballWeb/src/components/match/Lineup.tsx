import type { LineupPlayerResponse, LineupResponse } from "../../types/match";
import { EmptyState } from "../../components/common/States";

type PositionCategory = "GK" | "DEF" | "MID" | "FWD";

const DEF_CODES = ["CB", "LCB", "RCB", "LB", "RB", "LWB", "RWB"];
const FWD_CODES = ["CF", "ST", "LF", "RF"];

function codeOf(player: LineupPlayerResponse): string | null {
  return player.positionCode ?? player.roleShort ?? null;
}

function categoryOf(code: string | null): PositionCategory {
  if (!code) return "MID";
  const upper = code.toUpperCase();
  if (upper === "GK") return "GK";
  if (DEF_CODES.includes(upper)) return "DEF";
  if (FWD_CODES.includes(upper)) return "FWD";
  return "MID";
}

function isGoalkeeper(player: LineupPlayerResponse): boolean {
  return categoryOf(codeOf(player)) === "GK";
}

function hasCoords(player: LineupPlayerResponse): boolean {
  return player.customX !== null && player.customY !== null;
}

// "Nicolas Jackson" -> "N. Jackson"; single-part names render as-is.
function shortName(name: string): string {
  const parts = name.trim().split(/\s+/);
  if (parts.length < 2) return name;
  return `${parts[0][0]}. ${parts[parts.length - 1]}`;
}

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

  const coordsMode = lineup.starters.every(hasCoords);

  // Auto-orient: GKs normally sit at a low pitch-length fraction, attacking
  // upward means top = (1 - x). If the wire encodes the opposite, flip it
  // instead of drawing the team upside down.
  const gkPlayers = lineup.starters.filter(isGoalkeeper);
  const gkCoords = gkPlayers.filter(hasCoords);
  const flipX =
    coordsMode &&
    gkCoords.length > 0 &&
    gkCoords.reduce((sum, p) => sum + (p.customX as number), 0) /
      gkCoords.length >
      0.5;

  return (
    <div
      className="relative mx-auto w-full max-w-sm overflow-hidden rounded"
      style={{
        aspectRatio: "68 / 105",
        backgroundColor: "#1a3c25",
        backgroundImage:
          "linear-gradient(rgba(255,255,255,0.05) 1px, transparent 1px), linear-gradient(90deg, rgba(255,255,255,0.05) 1px, transparent 1px)",
        backgroundSize: "40px 40px",
      }}
    >
      <div
        className="pointer-events-none absolute inset-2"
        style={{ border: "2px solid rgba(255,255,255,0.3)" }}
      />
      <div className="pointer-events-none absolute left-2 right-2 top-1/2 h-[2px] -translate-y-1/2 bg-white/30" />
      <div
        className="pointer-events-none absolute left-1/2 top-1/2 aspect-square w-[27%] -translate-x-1/2 -translate-y-1/2 rounded-full"
        style={{ border: "2px solid rgba(255,255,255,0.3)" }}
      />
      <div className="pointer-events-none absolute left-1/2 top-1/2 h-1.5 w-1.5 -translate-x-1/2 -translate-y-1/2 rounded-full bg-white/60" />
      <div
        className="pointer-events-none absolute left-1/2 top-2 h-[16%] w-[59%] -translate-x-1/2"
        style={{ border: "2px solid rgba(255,255,255,0.3)", borderBottom: "none" }}
      />
      <div
        className="pointer-events-none absolute left-1/2 top-2 h-[7%] w-[28%] -translate-x-1/2"
        style={{ border: "2px solid rgba(255,255,255,0.3)", borderBottom: "none" }}
      />
      <div
        className="pointer-events-none absolute bottom-2 left-1/2 h-[16%] w-[59%] -translate-x-1/2"
        style={{ border: "2px solid rgba(255,255,255,0.3)", borderTop: "none" }}
      />
      <div
        className="pointer-events-none absolute bottom-2 left-1/2 h-[7%] w-[28%] -translate-x-1/2"
        style={{ border: "2px solid rgba(255,255,255,0.3)", borderTop: "none" }}
      />
      <div className="pointer-events-none absolute left-1/2 top-[13%] h-1.5 w-1.5 -translate-x-1/2 rounded-full bg-white/60" />
      <div className="pointer-events-none absolute bottom-[13%] left-1/2 h-1.5 w-1.5 -translate-x-1/2 rounded-full bg-white/60" />

      {coordsMode ? (
        lineup.starters.map((p) => {
          const x = p.customX as number;
          const topPercent = flipX ? x * 100 : (1 - x) * 100;
          return (
            <FormationDot
              key={p.playerId}
              player={p}
              top={`${topPercent}%`}
              left={`${(p.customY as number) * 100}%`}
            />
          );
        })
      ) : (
        <PositionRows players={lineup.starters} />
      )}
    </div>
  );
}

function PositionRows({ players }: { players: LineupPlayerResponse[] }) {
  const rows: Record<PositionCategory, LineupPlayerResponse[]> = {
    GK: [],
    DEF: [],
    MID: [],
    FWD: [],
  };
  for (const p of players) {
    rows[categoryOf(codeOf(p))].push(p);
  }

  const ordered: Array<{
    key: PositionCategory;
    row: LineupPlayerResponse[];
  }> = [
    { key: "FWD", row: rows.FWD },
    { key: "MID", row: rows.MID },
    { key: "DEF", row: rows.DEF },
    { key: "GK", row: rows.GK },
  ];

  return (
    <div className="relative z-10 flex h-full w-full flex-col justify-between py-2">
      {ordered.map(({ key, row }) =>
        row.length === 0 ? null : (
          <div
            key={key}
            className={`flex items-start ${
              row.length === 1
                ? "justify-center"
                : "justify-around px-4 md:px-6"
            }`}
          >
            {row.map((p) => (
              <FormationDot key={p.playerId} player={p} />
            ))}
          </div>
        ),
      )}
    </div>
  );
}

function FormationDot({
  player,
  top,
  left,
}: {
  player: LineupPlayerResponse;
  top?: string;
  left?: string;
}) {
  const gk = isGoalkeeper(player);
  const code = codeOf(player);

  return (
    <div
      className="absolute flex -translate-x-1/2 -translate-y-1/2 select-none flex-col items-center"
      style={{ top: top ?? "50%", left: left ?? "50%" }}
      title={player.roleName ?? player.positionName ?? undefined}
    >
      <div
        className={`flex h-8 w-8 items-center justify-center rounded-full font-data-mono text-data-mono font-bold shadow-lg ${
          gk
            ? "bg-secondary text-inverse-surface ring-2 ring-outline"
            : "bg-primary-container text-on-primary ring-2 ring-primary"
        }`}
      >
        {player.shirtNumber ?? "?"}
      </div>
      <span className="mt-1 rounded bg-surface-container-lowest/90 px-1.5 py-0.5 text-center text-[11px] font-semibold tracking-tight text-on-surface shadow">
        {shortName(player.playerName)}
        {code ? ` (${code})` : ""}
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
    <ul className="grid grid-cols-1 gap-2 md:grid-cols-3">
      {substitutes.map((p) => (
        <SubChip key={p.playerId} player={p} />
      ))}
    </ul>
  );
}

function SubChip({ player }: { player: LineupPlayerResponse }) {
  const used = player.minuteIn !== null;

  return (
    <li
      className={`flex items-center justify-between rounded p-2 ${
        used
          ? "bg-surface-container-high ring-1 ring-primary/30"
          : "bg-surface-container-low"
      }`}
    >
      <div className="flex min-w-0 items-center gap-2">
        <span
          className={`flex h-6 w-6 shrink-0 items-center justify-center rounded font-data-mono text-data-mono ${
            used
              ? "bg-primary-container text-on-primary"
              : "bg-surface-container-highest text-on-surface"
          }`}
        >
          {player.shirtNumber ?? "-"}
        </span>
        <span className="truncate font-body-sm text-body-sm text-on-surface">
          {player.playerName}
        </span>
      </div>
      <div className="flex shrink-0 items-center gap-1.5">
        {player.positionCode && (
          <span className="font-label-caps text-label-caps text-outline">
            {player.positionCode}
          </span>
        )}
        {used && (
          <span className="flex items-center gap-0.5">
            <span className="rounded bg-primary/20 px-1 font-data-mono text-data-mono text-primary">
              {player.minuteIn}'
            </span>
            <span className="material-symbols-outlined text-xs text-primary">
              arrow_upward
            </span>
          </span>
        )}
      </div>
    </li>
  );
}

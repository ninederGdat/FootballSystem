import type { PlayerAppearanceDTO } from "../../types/player";
import { formatMatchDate, formatMinuteRange } from "../../lib/format";
import { EmptyState } from "../../components/common/States";

export function AppearanceList({
  appearances,
}: {
  appearances: PlayerAppearanceDTO[];
}) {
  if (appearances.length === 0) {
    return (
      <EmptyState
        title="No appearances"
        description="No match appearances recorded for this player yet."
      />
    );
  }

  return (
    <div className="w-full border border-outline-variant rounded bg-surface-container overflow-hidden">
      <table className="w-full text-left border-collapse">
        <thead className="bg-surface-container-high border-b border-outline-variant">
          <tr>
            <th className="font-label-caps text-label-caps text-on-surface-variant uppercase p-3">
              Date
            </th>
            <th className="font-label-caps text-label-caps text-on-surface-variant uppercase p-3">
              Competition
            </th>
            <th className="font-label-caps text-label-caps text-on-surface-variant uppercase p-3">
              Opponent
            </th>
            <th className="font-label-caps text-label-caps text-on-surface-variant uppercase p-3">
              Playing Time
            </th>
            <th className="font-label-caps text-label-caps text-on-surface-variant uppercase p-3 text-right">
              Goals
            </th>
            <th className="font-label-caps text-label-caps text-on-surface-variant uppercase p-3 text-right">
              Assists
            </th>
          </tr>
        </thead>
        <tbody className="font-data-mono text-data-mono text-on-surface divide-y divide-outline-variant/30">
          {appearances.map((a, i) => (
            <tr
              key={a.matchId}
              className={`hover:bg-primary-container/10 transition-colors group relative h-row-height-compact ${
                i % 2 === 1 ? "bg-surface-container-low/50" : ""
              }`}
            >
              <td className="p-3">
                <div className="absolute left-0 top-0 bottom-0 w-1 bg-primary opacity-0 group-hover:opacity-100 transition-opacity" />
                {formatMatchDate(a.matchDate)}
              </td>
              <td className="p-3 text-on-surface-variant">
                {a.competitionName ?? "—"}
              </td>
              <td className="p-3 font-body-sm text-body-sm text-on-surface">
                {a.opponentName ?? "—"}
              </td>
              <td className="p-3">
                <span
                  className={`inline-flex items-center gap-1.5 px-2 py-0.5 rounded border border-outline-variant text-on-surface ${
                    a.isStarter
                      ? "bg-surface-container-highest"
                      : "bg-surface-container-highest/50"
                  }`}
                >
                  <span
                    className={`w-1.5 h-1.5 rounded-full ${
                      a.isStarter ? "bg-primary" : "bg-outline"
                    }`}
                  />
                  {a.isStarter ? "STARTER" : "SUB"}
                  {a.roleShort ? ` · ${a.roleShort}` : ""}
                  {" · "}
                  {formatMinuteRange(a.minuteIn, a.minuteOut)}
                </span>
              </td>
              <td className="p-3 text-right tabular-nums">
                {a.goals > 0 ? a.goals : "—"}
              </td>
              <td className="p-3 text-right tabular-nums">
                {a.assists > 0 ? a.assists : "—"}
              </td>
            </tr>
          ))}
        </tbody>
      </table>
    </div>
  );
}

import type { PlayerAppearanceDTO } from "../../types/player";
import { formatMatchDate, formatMinuteRange } from "../../lib/format";
import { EmptyState } from "../common/States";
import { ClubCell } from "./ClubCell";

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
    <div className="w-full overflow-x-auto rounded-lg">
      <table className="w-full text-left border-collapse">
        <thead className="bg-surface-container-highest text-outline font-label-caps text-label-caps uppercase tracking-wider h-9">
          <tr>
            <th className="px-4 text-left font-bold">Date</th>
            <th className="px-4 text-left font-bold">Competition</th>
            <th className="px-4 text-left font-bold">Club</th>
            <th className="px-4 text-left font-bold">Opponent</th>
            <th className="px-4 text-left font-bold">Playing Time</th>
            <th className="px-4 text-right font-bold">Goals</th>
            <th className="px-4 text-right font-bold">Assists</th>
          </tr>
        </thead>
        <tbody className="text-body-sm font-body-sm">
          {appearances.map((a, i) => {
            // minuteOut = null is meaningful (see formatMinuteRange) - only
            // derive total minutes when both bounds are present.
            const totalMinutes =
              a.minuteIn !== null && a.minuteOut !== null
                ? Math.max(0, a.minuteOut - a.minuteIn)
                : null;
            return (
              <tr
                key={a.matchId}
                className={`group relative h-10 transition-colors hover:bg-surface-container ${
                  i % 2 === 1 ? "bg-surface-container-high/40" : ""
                }`}
              >
                <td className="px-4 font-data-mono text-data-mono text-on-surface-variant whitespace-nowrap">
                  <div className="absolute left-0 top-0 bottom-0 w-1 bg-primary opacity-0 group-hover:opacity-100 transition-opacity" />
                  {formatMatchDate(a.matchDate)}
                </td>
                <td className="px-4 whitespace-nowrap text-on-surface font-medium">
                  {a.competitionName ?? "—"}
                </td>
                <td className="px-4 whitespace-nowrap">
                  <ClubCell team={a.playedForTeam} />
                </td>
                <td className="px-4 whitespace-nowrap text-on-surface font-medium">
                  {a.opponentName ?? "—"}
                </td>
                <td className="px-4 whitespace-nowrap font-data-mono text-data-mono text-on-surface-variant">
                  {a.isStarter ? (
                    <span className="text-primary font-medium">STARTER</span>
                  ) : (
                    <span className="text-secondary">SUB</span>
                  )}
                  {a.roleShort ? ` · ${a.roleShort}` : ""}
                  {" · "}
                  {formatMinuteRange(a.minuteIn, a.minuteOut)}
                  {totalMinutes !== null && (
                    <span className="text-on-surface font-semibold">
                      {" "}
                      ({totalMinutes}')
                    </span>
                  )}
                </td>
                <td
                  className={`px-4 text-right font-data-mono text-data-mono tabular-nums whitespace-nowrap ${
                    a.goals > 0
                      ? "text-primary font-bold"
                      : "text-on-surface-variant"
                  }`}
                >
                  {a.goals > 0 ? a.goals : "—"}
                </td>
                <td
                  className={`px-4 text-right font-data-mono text-data-mono tabular-nums whitespace-nowrap ${
                    a.assists > 0
                      ? "text-primary font-bold"
                      : "text-on-surface-variant"
                  }`}
                >
                  {a.assists > 0 ? a.assists : "—"}
                </td>
              </tr>
            );
          })}
        </tbody>
      </table>
    </div>
  );
}

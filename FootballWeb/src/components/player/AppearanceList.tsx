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
    <div className="overflow-x-auto rounded-lg border border-surface-border bg-surface">
      <table className="w-full min-w-[640px] text-sm">
        <thead>
          <tr className="border-b border-surface-border text-left text-xs uppercase tracking-wide text-ink-muted">
            <th className="px-3 py-2 font-medium">Date</th>
            <th className="px-3 py-2 font-medium">Opponent</th>
            <th className="px-3 py-2 font-medium">Competition</th>
            <th className="px-3 py-2 font-medium">Role</th>
            <th className="px-3 py-2 font-medium">Position</th>
            <th className="px-3 py-2 font-medium">Minutes</th>
            <th className="px-3 py-2 font-medium">Goals</th>
            <th className="px-3 py-2 font-medium">Assists</th>
          </tr>
        </thead>
        <tbody>
          {appearances.map((a) => (
            <tr
              key={a.matchId}
              className="border-b border-surface-border last:border-0"
            >
              <td className="px-3 py-2 text-ink">
                {formatMatchDate(a.matchDate)}
              </td>
              <td className="px-3 py-2 text-ink">{a.opponentName ?? "—"}</td>
              <td className="px-3 py-2 text-ink-muted">
                {a.competitionName ?? "—"}
              </td>
              <td className="px-3 py-2">
                <span
                  className={`rounded px-1.5 py-0.5 text-xs font-medium ${
                    a.isStarter
                      ? "bg-brand-50 text-brand-600"
                      : "bg-surface-subtle text-ink-muted"
                  }`}
                >
                  {a.isStarter ? "Starter" : "Sub"}
                </span>
              </td>
              <td className="px-3 py-2 text-ink-muted">
                {a.positionPlayed?.positionName ?? "—"}
                {a.roleShort ? ` (${a.roleShort})` : ""}
              </td>
              <td className="px-3 py-2 tabular-nums text-ink-muted">
                {formatMinuteRange(a.minuteIn, a.minuteOut)}
              </td>
            </tr>
          ))}
        </tbody>
      </table>
    </div>
  );
}

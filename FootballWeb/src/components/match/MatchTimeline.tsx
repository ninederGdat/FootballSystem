import type { MatchEventItemResponse } from "../../types/match";
import { formatMinute } from "../../lib/format";
import { EmptyState } from "../../components/common/States";

// Known event types get an icon; anything else falls back to a neutral
// marker so the timeline never hides or crashes on unrecognized data.
const EVENT_ICONS: Record<string, string> = {
  goal: "⚽",
  ownGoal: "⚽",
  penalty: "⚽",
  yellowCard: "🟨",
  redCard: "🟥",
  substitution: "🔄",
};

function iconFor(type: string): string {
  return EVENT_ICONS[type] ?? "•";
}

function labelFor(type: string): string {
  // camelCase -> "Camel Case" as a readable fallback for unknown types.
  const spaced = type.replace(/([a-z])([A-Z])/g, "$1 $2");
  return spaced.charAt(0).toUpperCase() + spaced.slice(1);
}

export function MatchTimeline({
  events,
}: {
  events: MatchEventItemResponse[];
}) {
  if (events.length === 0) {
    return (
      <EmptyState
        title="No events recorded"
        description="This match doesn't have timeline data yet."
      />
    );
  }

  const sorted = [...events].sort(
    (a, b) => a.minute - b.minute || (a.stoppageTime ?? 0) - (b.stoppageTime ?? 0),
  );

  return (
    <ol className="space-y-3">
      {sorted.map((event) => (
        <MatchEvent key={event.id} event={event} />
      ))}
    </ol>
  );
}

function MatchEvent({ event }: { event: MatchEventItemResponse }) {
  // player/team may be untracked locally (null) - name is still valid data.
  const playerName = event.player?.name ?? "Unknown player";

  return (
    <li className="flex gap-3 rounded-md border border-surface-border bg-surface p-3">
      <div className="w-12 shrink-0 text-right text-sm font-semibold tabular-nums text-ink-muted">
        {formatMinute(event.minute, event.stoppageTime)}
      </div>
      <div className="text-lg leading-none">{iconFor(event.type)}</div>
      <div className="min-w-0 flex-1">
        <p className="font-medium text-ink">{playerName}</p>
        <p className="text-xs text-ink-faint">{labelFor(event.type)}</p>
        {event.assist && (
          <p className="mt-0.5 text-xs text-ink-muted">
            Assist: {event.assist.name}
          </p>
        )}
        {event.description && (
          <p className="mt-0.5 text-xs text-ink-muted">{event.description}</p>
        )}
      </div>
    </li>
  );
}

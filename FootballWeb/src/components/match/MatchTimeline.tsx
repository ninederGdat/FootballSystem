import type { MatchEventItemResponse } from "../../types/match";
import { formatMinute } from "../../lib/format";
import { EmptyState } from "../../components/common/States";

// Known event types get a Material Symbols icon; anything else falls back
// to a neutral marker so the timeline never hides or crashes on
// unrecognized data.
const EVENT_ICONS: Record<string, string> = {
  goal: "sports_soccer",
  ownGoal: "sports_soccer",
  penalty: "sports_soccer",
  yellowCard: "square",
  redCard: "square",
  substitution: "sync_alt",
};

function iconFor(type: string): string {
  return EVENT_ICONS[type] ?? "circle";
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
    (a, b) =>
      a.minute - b.minute || (a.stoppageTime ?? 0) - (b.stoppageTime ?? 0),
  );

  return (
    <ol className="divide-y divide-outline-variant">
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
    <li className="flex gap-3 py-3 first:pt-0 last:pb-0">
      <div className="w-10 shrink-0 pt-0.5 text-right font-data-mono text-on-surface-variant">
        {formatMinute(event.minute, event.stoppageTime)}
      </div>
      <span className="flex h-8 w-8 shrink-0 items-center justify-center rounded-full bg-surface-variant">
        <span className="material-symbols-outlined text-[16px] leading-none text-primary">
          {iconFor(event.type)}
        </span>
      </span>
      <div className="min-w-0 flex-1">
        <p className="font-body-md font-medium text-on-surface">{playerName}</p>
        {event.assist ? (
          <p className="mt-0.5 font-body-sm text-on-surface-variant">
            Assist: {event.assist.name}
          </p>
        ) : (
          <p className="mt-0.5 font-label-caps uppercase text-on-surface-variant">
            {labelFor(event.type)}
          </p>
        )}
        {event.description && (
          <p className="mt-0.5 font-body-sm text-on-surface-variant">
            {event.description}
          </p>
        )}
      </div>
    </li>
  );
}

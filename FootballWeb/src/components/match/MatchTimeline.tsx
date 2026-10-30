import type { MatchEventItemResponse, MatchResponse } from "../../types/match";
import { formatMinute } from "../../lib/format";
import { EmptyState } from "../../components/common/States";

const GOAL_TYPES = new Set(["goal", "ownGoal", "penalty"]);
const COUNTED_GOAL_TYPES = new Set(["goal", "penalty"]);

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
  const spaced = type.replace(/([a-z])([A-Z])/g, "$1 $2");
  return spaced.charAt(0).toUpperCase() + spaced.slice(1);
}

interface TimelineRow {
  event: MatchEventItemResponse;
  runningScore: string | undefined;
  scoredByTracked: boolean | undefined;
}

function buildRows(events: MatchEventItemResponse[], match: MatchResponse) {
  const sorted = [...events].sort(
    (a, b) =>
      a.minute - b.minute || (a.stoppageTime ?? 0) - (b.stoppageTime ?? 0),
  );

  const canAttribute =
    match.teamId != null &&
    (match.homeOrAway === "home" || match.homeOrAway === "away");

  let home = 0;
  let away = 0;

  return sorted.map((event): TimelineRow => {
    let runningScore: string | undefined;
    let scoredByTracked: boolean | undefined;

    const isTracked = match.teamId != null && event.teamId === match.teamId;

    if (canAttribute && COUNTED_GOAL_TYPES.has(event.type)) {
      if (event.teamId == null) {
        // Goals can only be counted toward a side when the wire attributes
        // them to a team; unattributed goals are skipped, never guessed.
        runningScore = undefined;
      } else if (isTracked) {
        match.homeOrAway === "home" ? home++ : away++;
      } else {
        match.homeOrAway === "home" ? away++ : home++;
      }
      runningScore = `${home}\u2013${away}`;
      scoredByTracked = !!match.teamId && isTracked;
    }

    if (GOAL_TYPES.has(event.type)) {
      scoredByTracked = scoredByTracked ?? (match.teamId != null && isTracked);
    }

    return { event, runningScore, scoredByTracked };
  });
}

function dotClasses(type: string, tracked: boolean | undefined): string {
  switch (true) {
    case GOAL_TYPES.has(type):
      return tracked
        ? "bg-primary-container text-on-primary"
        : "bg-secondary-container text-on-secondary-container";
    case type === "yellowCard" || type === "redCard":
      return "bg-yellow-500 text-black";
    default:
      return "bg-surface-variant text-on-surface";
  }
}

function badgeClasses(type: string, tracked: boolean | undefined): string {
  if (GOAL_TYPES.has(type)) {
    if (tracked) return "bg-primary-container/20 text-primary-fixed";
    if (tracked === false)
      return "bg-secondary-container text-on-secondary-container";
    return "bg-surface-container-highest text-on-surface-variant";
  }
  if (type === "yellowCard") return "bg-yellow-400/20 text-yellow-400";
  if (type === "redCard") return "bg-error/20 text-error";
  return "bg-surface-container-highest text-on-surface-variant";
}

function minuteClasses(type: string, tracked: boolean | undefined): string {
  if (GOAL_TYPES.has(type)) {
    if (tracked) return "text-primary";
    if (tracked === false) return "text-secondary";
  }
  return "text-outline";
}

function isSubstitution(event: MatchEventItemResponse): boolean {
  return event.type === "substitution";
}

export function MatchTimeline({
  events,
  match,
}: {
  events: MatchEventItemResponse[];
  match: MatchResponse;
}) {
  if (events.length === 0) {
    return (
      <EmptyState
        title="No events recorded"
        description="This match doesn't have timeline data yet."
      />
    );
  }

  const rows = buildRows(events, match);

  return (
    <ol className="relative flex flex-col gap-3.5 pl-4 before:absolute before:left-2 before:top-2 before:bottom-2 before:w-0.5 before:bg-surface-container-highest">
      {rows.map(({ event, runningScore, scoredByTracked }) => (
        <TimelineEntry
          key={event.id}
          event={event}
          runningScore={runningScore}
          scoredByTracked={scoredByTracked}
        />
      ))}
    </ol>
  );
}

function TimelineEntry({
  event,
  runningScore,
  scoredByTracked,
}: {
  event: MatchEventItemResponse;
  runningScore: string | undefined;
  scoredByTracked: boolean | undefined;
}) {
  const playerName = event.player?.name ?? "Unknown player";

  return (
    <li className="relative flex items-start gap-3">
      <div
        className={`absolute -left-4 mt-0.5 flex h-4 w-4 items-center justify-center rounded-full text-[10px] ring-2 ring-background ${dotClasses(
          event.type,
          scoredByTracked,
        )}`}
      >
        <span className="material-symbols-outlined leading-none" style={{ fontSize: 10 }}>
          {iconFor(event.type)}
        </span>
      </div>

      <div className="flex-1 rounded-md bg-surface-container-low p-2.5">
        <div className="flex items-center justify-between gap-2">
          <div className="flex items-center gap-2">
            <span
              className={`font-data-mono text-data-mono font-bold ${minuteClasses(
                event.type,
                scoredByTracked,
              )}`}
            >
              {formatMinute(event.minute, event.stoppageTime)}
            </span>
            <span
              className={`rounded px-1.5 py-0.5 font-label-caps text-label-caps font-bold uppercase ${badgeClasses(
                event.type,
                scoredByTracked,
              )}`}
            >
              {labelFor(event.type)}
            </span>
            <span className="font-body-sm text-body-sm font-semibold text-on-surface">
              {event.type === "substitution" ? (
                <>
                  <span className="font-medium text-primary">IN:</span>{" "}
                  {playerName}
                </>
              ) : (
                playerName
              )}
            </span>
          </div>
          {runningScore && (
            <span className="font-data-mono text-data-mono text-outline">
              {runningScore}
            </span>
          )}
        </div>

        {!isSubstitution(event) && event.assist && !event.description && (
          <p className="mt-0.5 font-body-sm text-xs text-outline">
            Assist: {event.assist.name}
          </p>
        )}
        {!isSubstitution(event) &&
          event.assist &&
          event.description && (
            <p className="mt-0.5 font-body-sm text-xs text-outline">
              Assist: {event.assist.name} • {event.description}
            </p>
          )}
        {!isSubstitution(event) && !event.assist && event.description && (
          <p className="mt-0.5 font-body-sm text-xs text-outline">
            {event.description}
          </p>
        )}
        {!isSubstitution(event) && !event.assist && !event.description && (
          <p className="mt-0.5 font-label-caps uppercase text-on-surface-variant">
            {labelFor(event.type)}
          </p>
        )}
      </div>
    </li>
  );
}

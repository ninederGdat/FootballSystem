export function formatMatchDate(iso: string): string {
  const date = new Date(iso);
  return date.toLocaleDateString(undefined, {
    day: "2-digit",
    month: "short",
    year: "numeric",
  });
}

export function formatMinute(
  minute: number,
  stoppageTime: number | null,
): string {
  return stoppageTime ? `${minute}+${stoppageTime}'` : `${minute}'`;
}

// minuteOut = null is meaningful (still on the pitch / data not recorded
// downstream) - never coerce it to 90.
export function formatMinuteRange(
  minuteIn: number | null,
  minuteOut: number | null,
): string {
  if (minuteIn === null && minuteOut === null) return "—";
  const inStr = minuteIn ?? "0";
  const outStr = minuteOut ?? "—";
  return `${inStr}' – ${outStr}${typeof outStr === "number" ? "'" : ""}`;
}

export function formatStatus(status: string): string {
  const map: Record<string, string> = {
    FINISHED: "FT",
    NOT_STARTED: "Upcoming",
    IN_PLAY: "Live",
    POSTPONED: "Postponed",
    CANCELLED: "Cancelled",
  };
  return map[status] ?? status;
}

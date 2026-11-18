import { formatMatchDate } from "./format";

// Relative time for sync-run timestamps. A slightly-ahead server clock
// (negative delta) still counts as "just now".
export function formatRelativeTime(iso: string, now = Date.now()): string {
  const date = new Date(iso);
  const time = date.getTime();
  if (Number.isNaN(time)) return "—";

  const seconds = Math.floor((now - time) / 1000);
  if (seconds < 60) return "just now";

  const minutes = Math.floor(seconds / 60);
  if (minutes < 60) return `${minutes}m ago`;

  const hours = Math.floor(minutes / 60);
  if (hours < 24) return `${hours}h ago`;

  const days = Math.floor(hours / 24);
  if (days < 7) return `${days}d ago`;

  return formatMatchDate(iso);
}

// 703 -> "11m 43s", 45 -> "45s", 4000 -> "1h 6m"; negatives clamp to 0.
export function formatDuration(seconds: number | null): string {
  if (seconds == null || seconds <= 0) return "0s";

  const safe = Math.floor(seconds);
  const hours = Math.floor(safe / 3600);
  const minutes = Math.floor((safe % 3600) / 60);
  const secs = safe % 60;

  if (hours > 0) return `${hours}h ${minutes}m`;
  if (minutes > 0) return secs > 0 ? `${minutes}m ${secs}s` : `${minutes}m`;
  return `${secs}s`;
}

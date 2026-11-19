import type { SyncRun } from "../../types/sync";
import { formatDuration, formatRelativeTime } from "../../lib/relativeTime";
import { SYNC_RUN_STATUS_VIEWS } from "../../features/sync/syncRunStatus";

interface SyncRunItemProps {
  run: SyncRun;
  unread: boolean;
}

export function SyncRunItem({ run, unread }: SyncRunItemProps) {
  const view = SYNC_RUN_STATUS_VIEWS[run.status];
  const timestamp = formatRelativeTime(run.finishedAt ?? run.startedAt);
  const runUrl =
    run.runUrl !== null && run.runUrl.startsWith("https://")
      ? run.runUrl
      : null;
  const triggerLabel =
    run.trigger === null
      ? null
      : run.trigger === "schedule"
        ? "Scheduled"
        : run.trigger === "workflow_dispatch"
          ? "Manual run"
          : run.trigger;

  return (
    <li
      className={`relative p-3.5 transition-colors hover:bg-surface-container ${
        unread
          ? "border-l-2 border-primary bg-surface-container/60"
          : "border-l-2 border-transparent"
      }`}
    >
      <div className="flex items-start justify-between gap-2">
        <div className="flex flex-wrap items-center gap-2">
          <span
            className={`inline-flex items-center gap-1 rounded border px-1.5 py-0.5 text-[10px] font-medium font-data-mono ${view.chip}`}
          >
            <span
              className={`material-symbols-outlined text-[12px] ${
                view.spin ? "animate-spin motion-reduce:animate-none" : ""
              }`}
            >
              {view.icon}
            </span>
            {view.short}
          </span>
          <span className="text-xs font-semibold text-on-surface">
            {view.title}
          </span>
        </div>
        {unread && (
          <span className="mt-1.5 h-1.5 w-1.5 shrink-0 rounded-full bg-primary">
            <span className="sr-only">Unread</span>
          </span>
        )}
      </div>

      <div className="mt-2 flex flex-wrap items-center gap-2 text-[11px] font-data-mono text-on-surface-variant">
        {run.durationSeconds !== null && (
          <span className="rounded border border-outline-variant/40 bg-surface-container-high px-1.5 py-0.5 text-[10px] text-on-surface">
            {formatDuration(run.durationSeconds)}
          </span>
        )}
        <span className="text-[10px] text-on-surface-variant/80">
          {run.status === "running" ? `Started ${timestamp}` : timestamp}
        </span>
        {run.failedPhases.length > 0 && (
          <span className="text-[10px] text-error">
            Failed: {run.failedPhases.join(", ")}
          </span>
        )}
        {run.pendingStubs !== null && run.pendingStubs > 0 && (
          <span className="text-[10px]">
            {run.pendingStubs} player profiles still pending
          </span>
        )}
      </div>

      {(triggerLabel !== null || runUrl !== null) && (
        <div className="mt-2.5 flex items-center justify-between border-t border-outline-variant/30 pt-2">
          {triggerLabel !== null && (
            <span className="text-[10px] text-on-surface-variant">
              {triggerLabel}
            </span>
          )}
          {runUrl !== null && (
            <a
              href={runUrl}
              target="_blank"
              rel="noopener noreferrer"
              className="inline-flex items-center gap-0.5 text-[11px] font-medium text-primary hover:underline"
            >
              View run
              <span
                className="material-symbols-outlined text-[13px]"
                aria-hidden="true"
              >
                north_east
              </span>
            </a>
          )}
        </div>
      )}
    </li>
  );
}

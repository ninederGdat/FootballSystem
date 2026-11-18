import type { SyncRun } from "../../types/sync";
import { SyncRunItem } from "./SyncRunItem";

interface NotificationDropdownProps {
  runs: SyncRun[];
  isLoading: boolean;
  isError: boolean;
  unreadCount: number;
  isUnread: (run: SyncRun) => boolean;
}

export function NotificationDropdown({
  runs,
  isLoading,
  isError,
  unreadCount,
  isUnread,
}: NotificationDropdownProps) {
  return (
    <div
      role="region"
      aria-label="Sync notifications"
      className="absolute right-0 z-50 mt-2 w-96 max-w-[calc(100vw-2rem)] overflow-hidden rounded-xl border border-outline-variant bg-surface-container-low shadow-2xl"
    >
      <div className="flex items-center justify-between bg-surface-container px-4 py-3">
        <h3 className="text-xs font-bold uppercase tracking-wider font-data-mono text-on-surface">
          Notifications
        </h3>
        {unreadCount > 0 && (
          <span className="rounded-full border border-primary/30 bg-primary/20 px-1.5 py-0.5 text-[10px] font-semibold font-data-mono text-primary">
            {unreadCount} unread
          </span>
        )}
      </div>

      {isLoading ? (
        <div className="flex items-center justify-center gap-2 py-8 text-[13px] text-on-surface-variant">
          <span className="inline-block h-4 w-4 animate-spin rounded-full border-2 border-primary/30 border-t-primary motion-reduce:animate-none" />
          Loading sync runs...
        </div>
      ) : isError ? (
        <div className="px-4 py-8 text-center text-[13px] text-on-surface-variant">
          Couldn't load sync runs.
        </div>
      ) : runs.length === 0 ? (
        <div className="px-4 py-8 text-center text-[13px] text-on-surface-variant">
          No sync runs yet.
        </div>
      ) : (
        <ul className="max-h-[360px] divide-y divide-outline-variant/40 overflow-y-auto">
          {runs.map((run) => (
            <SyncRunItem key={run.id} run={run} unread={isUnread(run)} />
          ))}
        </ul>
      )}
    </div>
  );
}

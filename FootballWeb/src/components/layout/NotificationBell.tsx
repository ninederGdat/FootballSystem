import { useCallback, useEffect, useRef, useState } from "react";
import { useSyncNotifications } from "../../features/sync/useSyncNotifications";
import { NotificationDropdown } from "./NotificationDropdown";

export function NotificationBell() {
  const wrapperRef = useRef<HTMLDivElement>(null);
  const [open, setOpen] = useState(false);
  const { runs, isLoading, isError, unreadCount, isUnread, markAllRead } =
    useSyncNotifications();

  // One shared close path so the button, outside click and Escape all end
  // with the same "close, then mark read" flow.
  const close = useCallback(() => {
    setOpen(false);
    markAllRead();
  }, [markAllRead]);

  useEffect(() => {
    if (!open) return;

    const onMouseDown = (event: MouseEvent) => {
      if (!wrapperRef.current?.contains(event.target as Node)) close();
    };
    const onKeyDown = (event: KeyboardEvent) => {
      if (event.key === "Escape") close();
    };

    document.addEventListener("mousedown", onMouseDown);
    document.addEventListener("keydown", onKeyDown);
    return () => {
      document.removeEventListener("mousedown", onMouseDown);
      document.removeEventListener("keydown", onKeyDown);
    };
  }, [open, close]);

  const ariaLabel =
    unreadCount > 0
      ? `Notifications, ${unreadCount} unread`
      : "Notifications";

  return (
    <div ref={wrapperRef} className="relative">
      <button
        type="button"
        aria-label={ariaLabel}
        aria-haspopup="true"
        aria-expanded={open}
        onClick={() => (open ? close() : setOpen(true))}
        className="flex h-9 w-9 items-center justify-center rounded text-on-surface-variant transition-colors hover:bg-surface-container-high hover:text-on-surface"
      >
        <span className="material-symbols-outlined text-[20px]">
          notifications
        </span>
        {unreadCount > 0 && (
          <span className="absolute -right-1 -top-1 flex h-4 min-w-[16px] items-center justify-center rounded-full bg-primary px-1 text-[10px] font-bold font-data-mono text-on-primary">
            {unreadCount > 9 ? "9+" : unreadCount}
          </span>
        )}
      </button>
      {open && (
        <NotificationDropdown
          runs={runs}
          isLoading={isLoading}
          isError={isError}
          unreadCount={unreadCount}
          isUnread={isUnread}
        />
      )}
    </div>
  );
}

import type { SyncRunStatus } from "../../types/sync";

interface SyncRunStatusView {
  title: string;
  short: string;
  icon: string;
  chip: string;
  spin?: boolean;
}

// Map each backend status to its dropdown view. The chip pairs a Material
// Symbol with a text label so status is never colour-only. emerald/amber are
// not project tokens but compile via Tailwind defaults (accepted deviation
// from the AppShell palette).
export const SYNC_RUN_STATUS_VIEWS: Record<SyncRunStatus, SyncRunStatusView> = {
  running: {
    title: "Sync in progress",
    short: "Running",
    icon: "sync",
    chip: "bg-surface-container-high text-on-surface-variant border-outline-variant",
    spin: true,
  },
  success: {
    title: "Sync completed",
    short: "Success",
    icon: "check_circle",
    chip: "bg-emerald-500/20 text-emerald-400 border-emerald-500/30",
  },
  partial: {
    title: "Sync completed with errors",
    short: "Partial",
    icon: "warning",
    chip: "bg-amber-500/20 text-amber-400 border-amber-500/30",
  },
  failed: {
    title: "Sync failed",
    short: "Failed",
    icon: "error",
    chip: "bg-error/15 text-error border-error/30",
  },
  interrupted: {
    title: "Sync interrupted",
    short: "Interrupted",
    icon: "block",
    chip: "bg-error/15 text-error border-error/30",
  },
};

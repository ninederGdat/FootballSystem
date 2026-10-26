import { useEffect, useRef, useState } from "react";
import { TRACKED_TEAMS } from "../../lib/teams";
import { useContextTeam } from "./TeamContext";

export function ClubSwitcher() {
  const { team, setTeamId } = useContextTeam();
  const [open, setOpen] = useState(false);
  const ref = useRef<HTMLDivElement>(null);

  useEffect(() => {
    if (!open) return;
    const onDown = (e: MouseEvent) => {
      if (!ref.current?.contains(e.target as Node)) setOpen(false);
    };
    const onKey = (e: KeyboardEvent) => {
      if (e.key === "Escape") setOpen(false);
    };
    document.addEventListener("mousedown", onDown);
    document.addEventListener("keydown", onKey);
    return () => {
      document.removeEventListener("mousedown", onDown);
      document.removeEventListener("keydown", onKey);
    };
  }, [open]);

  return (
    <div ref={ref} className="relative">
      <div className="mb-2 font-label-caps text-[10px] uppercase text-outline-variant">
        Active Club Context
      </div>
      <button
        type="button"
        onClick={() => setOpen((o) => !o)}
        aria-haspopup="listbox"
        aria-expanded={open}
        className="group flex w-full items-center justify-between gap-2 rounded border border-outline-variant bg-surface-container p-2 text-left transition-colors hover:border-primary/50"
      >
        <div className="flex min-w-0 items-center gap-2">
          <div className="flex h-6 w-6 shrink-0 items-center justify-center rounded-full bg-primary-container text-[10px] font-bold text-on-primary-container">
            {team.abbr.slice(0, 3)}
          </div>
          <div className="flex min-w-0 flex-col">
            <span className="truncate font-body-sm font-semibold text-on-surface">
              {team.teamName}
            </span>
            <span className="truncate text-[10px] text-on-surface-variant">
              {team.leagueName}
            </span>
          </div>
        </div>
        <span className="material-symbols-outlined shrink-0 text-sm text-outline-variant transition-colors group-hover:text-primary">
          unfold_more
        </span>
      </button>

      {open && (
        <ul
          role="listbox"
          className="absolute bottom-full left-0 right-0 mb-1 overflow-hidden rounded border border-outline-variant bg-surface-container-high shadow-lg"
        >
          {TRACKED_TEAMS.map((t) => (
            <li
              key={t.teamId}
              role="option"
              aria-selected={t.teamId === team.teamId}
            >
              <button
                type="button"
                onClick={() => {
                  setTeamId(t.teamId);
                  setOpen(false);
                }}
                className={`flex w-full items-center gap-2 px-3 py-2 text-left font-body-sm transition-colors hover:bg-surface-variant ${
                  t.teamId === team.teamId ? "text-primary" : "text-on-surface"
                }`}
              >
                <span className="font-data-mono text-[10px]">{t.abbr}</span>
                {t.teamName}
              </button>
            </li>
          ))}
        </ul>
      )}
    </div>
  );
}

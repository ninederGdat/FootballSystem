export function SidebarLogo() {
  return (
    <div className="flex h-16 items-center gap-3 border-b border-outline-variant px-6">
      <div className="flex h-9 w-9 items-center justify-center rounded-xl bg-primary-container text-white shadow-lg shadow-primary-container/20">
        <span className="material-symbols-outlined text-[20px]">
          sports_soccer
        </span>
      </div>
      <div>
        <div className="text-sm font-bold tracking-tight text-on-surface">
          FootballSystem
        </div>
        <div className="font-data-mono text-[10px] uppercase tracking-wider text-on-surface-variant">
          Intelligence Unit
        </div>
      </div>
    </div>
  );
}

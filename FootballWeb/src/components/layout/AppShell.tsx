import { NavLink, Outlet } from "react-router-dom";

const navItems = [
  { to: "/", label: "Dashboard", icon: "dashboard" },
  { to: "/players", label: "Players", icon: "groups" },
  { to: "/transfers", label: "Transfers", icon: "compare_arrows" },
  { to: "/matches", label: "Matches", icon: "sports_soccer" },
];

export function AppShell() {
  return (
    <div className="flex min-h-screen md:flex-row">
      {/* Sidebar */}
      <aside className="flex w-full shrink-0 flex-col border-b border-outline-variant bg-surface-container md:h-screen md:w-64 md:border-b-0 md:border-r">
        <div className="px-5 py-5">
          <div className="font-headline-sm text-on-surface">FootballSystem</div>
          <div className="mt-0.5 font-label-caps uppercase text-outline">
            Intelligence Unit
          </div>
        </div>

        <nav className="flex flex-1 flex-col gap-1 px-3">
          {navItems.map((item) => (
            <NavLink
              key={item.to}
              to={item.to}
              end={item.to === "/"}
              className={({ isActive }) =>
                `flex items-center gap-3 rounded px-3 py-2 font-body-md transition-colors ${
                  isActive
                    ? "bg-primary-container/20 text-primary"
                    : "text-on-surface-variant hover:bg-surface-container-high hover:text-on-surface"
                }`
              }
            >
              <span className="material-symbols-outlined text-[20px]">
                {item.icon}
              </span>
              {item.label}
            </NavLink>
          ))}
        </nav>
      </aside>

      {/* Main column */}
      <div className="flex min-h-screen flex-1 flex-col bg-background">
        <header className="flex items-center justify-end gap-4 border-b border-outline-variant px-4 py-3 md:px-8">
          <button
            type="button"
            aria-label="Search"
            className="flex h-9 w-9 items-center justify-center rounded text-on-surface-variant transition-colors hover:bg-surface-container-high hover:text-on-surface"
          >
            <span className="material-symbols-outlined text-[20px]">
              search
            </span>
          </button>
          <button
            type="button"
            aria-label="Notifications"
            className="flex h-9 w-9 items-center justify-center rounded text-on-surface-variant transition-colors hover:bg-surface-container-high hover:text-on-surface"
          >
            <span className="material-symbols-outlined text-[20px]">
              notifications
            </span>
          </button>
          <button
            type="button"
            aria-label="Settings"
            className="flex h-9 w-9 items-center justify-center rounded text-on-surface-variant transition-colors hover:bg-surface-container-high hover:text-on-surface"
          >
            <span className="material-symbols-outlined text-[20px]">
              settings
            </span>
          </button>
        </header>

        <main className="flex-1">
          <div className="mx-auto max-w-6xl px-4 py-6 md:px-8 md:py-8">
            <Outlet />
          </div>
        </main>
      </div>
    </div>
  );
}

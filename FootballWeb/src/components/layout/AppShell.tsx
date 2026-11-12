import { Outlet } from "react-router-dom";
import { ClubSwitcher } from "../../features/team-context/ClubSwitcher";
import { TeamContextProvider } from "../../features/team-context/TeamContext";
import { SidebarNavItem } from "./SidebarNavItem";
import { SidebarLogo } from "./SidebarLogo";

const navItems = [
  { to: "/", label: "Dashboard", icon: "dashboard" },
  { to: "/players", label: "Players", icon: "groups" },
  { to: "/transfers", label: "Transfers", icon: "compare_arrows" },
  { to: "/matches", label: "Matches", icon: "sports_soccer" },
];

export function AppShell() {
  return (
    <TeamContextProvider>
      <AppShellLayout />
    </TeamContextProvider>
  );
}

function AppShellLayout() {
  return (
    <div className="flex min-h-screen md:flex-row">
      {/* Sidebar */}
      <aside className="flex w-full shrink-0 flex-col border-b border-outline-variant bg-surface-container-low md:sticky md:top-0 md:h-screen md:w-64 md:border-b-0 md:border-r">
        <SidebarLogo />

        <nav className="flex flex-1 flex-col gap-1 overflow-y-auto p-3">
          {navItems.map((item) => (
            <SidebarNavItem
              key={item.to}
              to={item.to}
              label={item.label}
              icon={item.icon}
              end={item.to === "/"}
            />
          ))}
        </nav>
        <div className="mt-auto border-t border-outline-variant p-3">
          <ClubSwitcher />
        </div>
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

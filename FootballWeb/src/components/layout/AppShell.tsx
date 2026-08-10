import { NavLink, Outlet } from "react-router-dom";

const navItems = [
  { to: "/matches", label: "Matches" },
  { to: "/players", label: "Players" },
];

export function AppShell() {
  return (
    <div className="flex min-h-screen flex-col md:flex-row">
      <aside className="flex shrink-0 flex-col border-b border-surface-border bg-brand-500 text-white md:h-screen md:w-56 md:border-b-0 md:border-r">
        <div className="px-5 py-5 text-lg font-semibold tracking-tight">
          FootballSystem
        </div>
        <nav className="flex gap-1 px-3 pb-3 md:flex-col md:pb-0">
          {navItems.map((item) => (
            <NavLink
              key={item.to}
              to={item.to}
              className={({ isActive }) =>
                `rounded-md px-3 py-2 text-sm font-medium transition-colors ${
                  isActive
                    ? "bg-white/15 text-white"
                    : "text-white/70 hover:bg-white/10 hover:text-white"
                }`
              }
            >
              {item.label}
            </NavLink>
          ))}
        </nav>
      </aside>

      <main className="flex-1 bg-surface-subtle">
        <div className="mx-auto max-w-5xl px-4 py-6 md:px-8 md:py-8">
          <Outlet />
        </div>
      </main>
    </div>
  );
}

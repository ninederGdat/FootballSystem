import { NavLink } from "react-router-dom";

interface SidebarNavItemProps {
  to: string;
  label: string;
  icon: string;
  end?: boolean;
}

export function SidebarNavItem({
  to,
  label,
  icon,
  end,
}: SidebarNavItemProps) {
  return (
    <NavLink
      to={to}
      end={end}
      className={({ isActive }) =>
        `group flex items-center gap-3 rounded-xl border-l-4 px-3 py-2.5 font-body-md font-medium transition-colors focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-primary/40 ${
          isActive
            ? "border-primary-container bg-surface-variant text-on-surface"
            : "border-transparent text-on-surface-variant hover:bg-surface-container-high hover:text-on-surface"
        }`
      }
    >
      {({ isActive }) => (
        <>
          <span
            className={`material-symbols-outlined text-[20px] ${
              isActive ? "text-primary-container" : ""
            }`}
          >
            {icon}
          </span>
          {label}
        </>
      )}
    </NavLink>
  );
}

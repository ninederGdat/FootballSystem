import type { ReactNode } from "react";

export function SectionCard({
  title,
  icon,
  right,
  children,
  className,
}: {
  title: string;
  icon?: string;
  right?: ReactNode;
  children: ReactNode;
  className?: string;
}) {
  return (
    <div
      className={`overflow-hidden rounded border border-outline-variant bg-surface-container ${className ?? ""}`}
    >
      <div className="flex items-center justify-between border-b border-outline-variant bg-surface-container-high px-4 py-2.5">
        <div className="flex items-center gap-2 font-label-caps uppercase text-on-surface-variant">
          {icon && (
            <span className="material-symbols-outlined text-[16px]">
              {icon}
            </span>
          )}
          {title}
        </div>
        {right}
      </div>
      <div className="p-4">{children}</div>
    </div>
  );
}

import type { PlayerRelation } from "../../types/player";

// Relation of a player to the context team being viewed.
export function RelationBadge({ relation }: { relation: PlayerRelation }) {
  const cls =
    "inline-flex items-center rounded px-2 py-0.5 text-[10px] font-bold uppercase tracking-wider border";
  if (relation === "current")
    return (
      <span className={`${cls} bg-primary/10 border-primary/30 text-primary`}>
        Current
      </span>
    );
  if (relation === "loaned_in")
    return (
      <span
        className={`${cls} bg-secondary-container/30 border-outline-variant text-on-secondary-container`}
      >
        Loaned in
      </span>
    );
  if (relation === "loaned_out")
    return (
      <span
        className={`${cls} bg-secondary-container/30 border-outline-variant text-on-secondary-container`}
      >
        Loaned out
      </span>
    );
  return (
    <span className={`${cls} bg-error-container/20 border-error/30 text-error`}>
      Transferred
    </span>
  );
}

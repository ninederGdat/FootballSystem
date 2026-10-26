export function StatusPill({
  status,
  injury,
}: {
  status: string | null;
  injury: string | null;
}) {
  if (!status) return <span className="text-on-surface-variant">-</span>;
  const injured = status === "Injured";
  return (
    <span
      title={injured ? (injury ?? undefined) : undefined}
      className={`inline-flex items-center gap-1.5 rounded-full border border-outline-variant bg-surface-container-high px-2 py-0.5 text-[10px] font-bold uppercase tracking-wider text-on-surface ${
        injured ? "opacity-70" : ""
      }`}
    >
      <span
        className={`h-1.5 w-1.5 rounded-full ${injured ? "bg-error" : "bg-primary-container"}`}
      />
      {injured ? "Injured" : "Active"}
    </span>
  );
}

// Presentational filter dropdown in the primary accent style.
export function FilterSelect({
  label,
  value,
  onChange,
  options,
}: {
  label: string;
  value: string;
  onChange: (v: string) => void;
  options: { value: string; label: string }[];
}) {
  return (
    <div className="flex items-center gap-2 rounded border border-primary/30 bg-primary/10 p-1">
      <span className="px-2 font-label-caps uppercase text-primary">
        {label}
      </span>
      <div className="relative">
        <select
          value={value}
          onChange={(e) => onChange(e.target.value)}
          className="cursor-pointer appearance-none border-none bg-transparent py-1 pl-2 pr-8 font-body-sm text-primary focus:ring-0 [color-scheme:dark]"
        >
          {options.map((option) => (
            <option
              key={option.value}
              value={option.value}
              className="bg-surface-container text-on-surface"
            >
              {option.label}
            </option>
          ))}
        </select>
        <span className="material-symbols-outlined pointer-events-none absolute right-1 top-1/2 -translate-y-1/2 text-[16px] text-primary">
          expand_more
        </span>
      </div>
    </div>
  );
}

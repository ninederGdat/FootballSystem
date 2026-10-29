// Presentational filter dropdown in the neutral prototype style.
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
    <div className="flex items-center gap-2 rounded border border-outline-variant bg-surface-container-low p-1">
      <span className="px-2 font-label-caps text-outline-variant">
        {label}
      </span>
      <div className="relative">
        <select
          value={value}
          onChange={(e) => onChange(e.target.value)}
          className="cursor-pointer appearance-none border-none bg-transparent py-1 pl-2 pr-8 font-body-sm text-on-surface focus:ring-0 [color-scheme:dark]"
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
        <span className="material-symbols-outlined pointer-events-none absolute right-1 top-1/2 -translate-y-1/2 text-[16px] text-outline-variant">
          expand_more
        </span>
      </div>
    </div>
  );
}

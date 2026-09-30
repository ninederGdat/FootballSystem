import { useState } from "react";
import { Link } from "react-router-dom";
import { ErrorState, LoadingState } from "../components/common/States";
import { useSeasons } from "../features/seasons/hooks";
import { useTransferStatistics } from "../features/transfers/hooks";
import type { TransferStatistics } from "../types/transfer";

function formatEuroAmount(amount: number): string {
  const absolute = Math.abs(amount);
  if (absolute >= 1_000_000) {
    return `€${(amount / 1_000_000).toLocaleString(undefined, { maximumFractionDigits: 1 })}M`;
  }
  if (absolute >= 1_000) {
    return `€${(amount / 1_000).toLocaleString(undefined, { maximumFractionDigits: 0 })}K`;
  }
  return `€${amount.toLocaleString()}`;
}

function MetricCard({
  icon,
  label,
  value,
  detail,
  tone,
}: {
  icon: string;
  label: string;
  value: string;
  detail: string;
  tone: "blue" | "green" | "slate";
}) {
  const tones = {
    blue: "bg-primary/10 text-primary",
    green: "bg-emerald-400/10 text-emerald-300",
    slate: "bg-surface-container-high text-on-surface-variant",
  };

  return (
    <section className="flex min-h-40 flex-col justify-between border border-outline-variant/50 bg-surface-container-low p-5 shadow-sm transition-colors hover:border-primary/40">
      <div className="mb-4 flex items-center gap-2.5">
        <span
          className={`flex h-8 w-8 items-center justify-center rounded ${tones[tone]}`}
        >
          <span className="material-symbols-outlined text-[18px]">{icon}</span>
        </span>
        <h2 className="font-label-caps text-label-caps uppercase text-on-surface-variant">
          {label}
        </h2>
      </div>
      <div>
        <p className="font-data-mono text-3xl font-semibold text-on-surface">
          {value}
        </p>
        <p className="mt-2 text-xs text-on-surface-variant">{detail}</p>
      </div>
    </section>
  );
}

function TransferComparison({
  statistics,
}: {
  statistics: TransferStatistics;
}) {
  const highestValue = Math.max(
    statistics.totalFeeToBuy,
    statistics.totalFeeToSell,
    1,
  );
  const scale = Math.ceil(highestValue / 50_000_000) * 50_000_000 || 50_000_000;
  const rows = [
    {
      label: "Spending",
      amount: statistics.totalFeeToBuy,
      color: "bg-primary-container",
    },
    {
      label: "Revenue",
      amount: statistics.totalFeeToSell,
      color: "bg-tertiary-fixed-dim",
    },
  ];

  return (
    <section className="border border-outline-variant/50 bg-surface-container-low p-5 shadow-sm md:p-6">
      <div className="mb-6 flex flex-col justify-between gap-3 border-b border-outline-variant/30 pb-5 sm:flex-row sm:items-start">
        <div>
          <div className="flex flex-wrap items-center gap-2">
            <h2 className="font-headline-sm text-lg font-semibold text-on-surface">
              Spending vs revenue
            </h2>
            <span className="border border-primary/30 bg-primary-container/20 px-2 py-0.5 font-data-mono text-[11px] text-primary">
              Net: {statistics.netSpend < 0 ? "−" : ""}
              {formatEuroAmount(Math.abs(statistics.netSpend))}
            </span>
          </div>
          <p className="mt-1 text-xs text-on-surface-variant">
            Chelsea transfer fees for {statistics.season.replace("-", "/")}
          </p>
        </div>
        <Link
          to="/transfers"
          className="inline-flex items-center gap-1 self-start text-xs font-medium text-primary hover:text-on-surface"
        >
          View transfers
          <span className="material-symbols-outlined text-[16px]">
            arrow_forward
          </span>
        </Link>
      </div>

      <div className="space-y-6" aria-label="Spending compared with revenue">
        {rows.map((row) => (
          <div key={row.label}>
            <div className="mb-2 flex items-baseline justify-between gap-3">
              <span className="text-sm text-on-surface">{row.label}</span>
              <span className="font-data-mono text-sm text-on-surface">
                {formatEuroAmount(row.amount)}
              </span>
            </div>
            <div className="h-3 overflow-hidden bg-surface-container-high">
              <div
                className={`h-full ${row.color} transition-[width] duration-500`}
                style={{
                  width: `${Math.max((row.amount / scale) * 100, row.amount ? 1 : 0)}%`,
                }}
              />
            </div>
          </div>
        ))}
      </div>

      <div className="mt-5 flex justify-between border-t border-outline-variant/30 pt-3 font-data-mono text-[11px] text-on-surface-variant">
        <span>€0</span>
        <span>{formatEuroAmount(scale)}</span>
      </div>
    </section>
  );
}

export function DashboardPage() {
  const [chosenSeason, setChosenSeason] = useState("");
  const seasons = useSeasons();
  const seasonOptions = seasons.data?.data ?? [];
  const activeSeason =
    seasonOptions.find((season) => season.code === chosenSeason)?.code ??
    seasonOptions[0]?.code;
  const statistics = useTransferStatistics(activeSeason);
  const current = statistics.data;

  return (
    <div className="space-y-7">
      <div className="flex flex-col justify-between gap-4 sm:flex-row sm:items-end">
        <div>
          <div className="mb-1 flex flex-wrap items-center gap-2">
            <span className="font-label-caps text-label-caps uppercase text-primary">
              {current?.teamName || "Chelsea F.C."} overview
            </span>
            <span className="h-1 w-1 rounded-full bg-outline-variant" />
            <span className="font-data-mono text-xs text-on-surface-variant">
              Premier League
            </span>
          </div>
          <h1 className="font-display-lg text-3xl font-bold text-on-surface">
            Dashboard
          </h1>
          <p className="mt-1 text-sm text-on-surface-variant">
            Transfer spending, revenue, and squad movements
          </p>
        </div>

        {seasonOptions.length > 0 && (
          <div
            className="flex max-w-full items-center gap-1 overflow-x-auto border border-outline-variant/60 bg-surface-container-low p-1"
            role="group"
            aria-label="Select season"
          >
            {seasonOptions.slice(0, 3).map((season) => {
              const isActive = season.code === activeSeason;
              return (
                <button
                  key={season.code}
                  type="button"
                  aria-pressed={isActive}
                  onClick={() => setChosenSeason(season.code)}
                  className={`whitespace-nowrap px-3 py-1.5 text-xs font-medium transition-colors ${
                    isActive
                      ? "bg-primary-container text-on-primary"
                      : "text-on-surface-variant hover:bg-surface-container-high hover:text-on-surface"
                  }`}
                >
                  {season.name}
                </button>
              );
            })}
          </div>
        )}
      </div>

      {seasons.isLoading ? (
        <LoadingState label="Loading seasons..." />
      ) : seasons.isError ? (
        <ErrorState error={seasons.error} />
      ) : statistics.isLoading ? (
        <LoadingState label="Loading transfer overview..." />
      ) : statistics.isError ? (
        <ErrorState error={statistics.error} />
      ) : !current ? (
        <div className="border border-outline-variant/50 bg-surface-container-low px-5 py-10 text-center text-sm text-on-surface-variant">
          No configured seasons are available.
        </div>
      ) : (
        <>
          <div className="grid grid-cols-1 gap-4 sm:grid-cols-2 xl:grid-cols-4">
            <MetricCard
              icon="south_east"
              label="Transfer in"
              value={formatEuroAmount(current.totalFeeToBuy)}
              detail={`${current.permanentBuyCount} permanent signings`}
              tone="blue"
            />
            <MetricCard
              icon="north_east"
              label="Transfer out"
              value={formatEuroAmount(current.totalFeeToSell)}
              detail={`${current.permanentSellCount} permanent sales`}
              tone="green"
            />
            <MetricCard
              icon="person_add"
              label="Players in"
              value={String(current.permanentBuyCount + current.loanInCount)}
              detail={`${current.permanentBuyCount} permanent · ${current.loanInCount} loans`}
              tone="blue"
            />
            <MetricCard
              icon="person_remove"
              label="Players out"
              value={String(current.permanentSellCount + current.loanOutCount)}
              detail={`${current.permanentSellCount} permanent · ${current.loanOutCount} loans`}
              tone="slate"
            />
          </div>

          <TransferComparison statistics={current} />
        </>
      )}
    </div>
  );
}

import { useState } from "react";
import { useParams } from "react-router-dom";
import { usePlayer, usePlayerAppearances } from "../features/players/hooks";
import { AppearanceList } from "../components/player/AppearanceList";
import { Pagination } from "../components/common/Pagination";
import { LoadingState, ErrorState } from "../components/common/States";
import { formatMarketValue } from "../lib/format";

export function PlayerDetailPage() {
  const { playerId } = useParams<{ playerId: string }>();
  const id = Number(playerId);
  const [page, setPage] = useState(1);

  const profile = usePlayer(id);
  const appearances = usePlayerAppearances(id, page, 20);

  if (!Number.isFinite(id)) {
    return <ErrorState error={new Error("Invalid player ID in URL.")} />;
  }

  if (profile.isLoading) return <LoadingState label="Loading player..." />;
  if (profile.error) return <ErrorState error={profile.error} />;
  if (!profile.data) return null;

  const player = profile.data;
  const isFit = player.status.status.toLowerCase() === "active";
  const isInjured = player.status.status.toLowerCase() === "injured";
  const statusDotClass = isFit
    ? "bg-[#10B981]"
    : isInjured
      ? "bg-error"
      : "bg-outline";

  return (
    <div>
      {/* Player Identity Block */}
      <section className="mb-gutter bg-surface-container-high border border-outline-variant rounded p-6">
        <div className="flex items-start justify-between flex-wrap gap-4">
          <div className="flex items-center gap-6">
            <div className="w-16 h-16 rounded-full bg-primary-container flex items-center justify-center border border-primary shrink-0">
              <span className="font-data-mono text-[24px] font-bold text-on-primary-container">
                {player.shirtNumber ?? "—"}
              </span>
            </div>
            <div>
              <h2 className="font-display-lg text-display-lg text-on-surface mb-1">
                {player.name}
              </h2>
              <div className="flex items-center gap-3 text-on-surface-variant font-body-sm text-body-sm flex-wrap">
                {player.preferredPosition && (
                  <span className="flex items-center gap-1">
                    <span className="material-symbols-outlined text-[16px]">
                      sports_martial_arts
                    </span>{" "}
                    {player.preferredPosition.positionName}
                  </span>
                )}
                {player.preferredPosition && player.nationality && (
                  <span className="w-1 h-1 rounded-full bg-outline" />
                )}
                {player.nationality && <span>{player.nationality}</span>}
                {player.currentTeam && (
                  <>
                    <span className="w-1 h-1 rounded-full bg-outline" />
                    <span>{player.currentTeam.teamName}</span>
                  </>
                )}
              </div>
            </div>
          </div>
          <div className="flex gap-6 text-right">
            <div>
              <p className="font-label-caps text-label-caps text-on-surface-variant uppercase mb-1">
                Market Value
              </p>
              <p className="font-data-mono text-[18px] text-on-surface font-bold">
                {player.contract.marketValue != null
                  ? formatMarketValue(player.contract.marketValue)
                  : "—"}
              </p>
            </div>
            <div>
              <p className="font-label-caps text-label-caps text-on-surface-variant uppercase mb-1">
                Contract
              </p>
              <p className="font-data-mono text-[18px] text-on-surface font-bold">
                {player.contract.contractUntil ?? "—"}
              </p>
            </div>
          </div>
        </div>

        {player.status.injuryDescription && (
          <p className="mt-4 rounded bg-error-container/20 border border-error-container px-3 py-2 font-body-sm text-body-sm text-error">
            {player.status.injuryDescription}
          </p>
        )}
      </section>

      <div className="grid grid-cols-12 gap-gutter">
        {/* Overview */}
        <div className="col-span-12 lg:col-span-3 flex flex-col gap-unit">
          <h3 className="font-headline-sm text-headline-sm text-on-surface mb-2">
            Overview
          </h3>
          <div className="bg-surface-container border border-outline-variant rounded p-4">
            <OverviewRow
              label="Position"
              value={player.preferredPosition?.positionName ?? "—"}
            />
            <OverviewRow
              label="Nationality"
              value={player.nationality ?? "—"}
            />
            <OverviewRow label="DOB" value={player.dateOfBirth ?? "—"} mono />
            <OverviewRow
              label="Shirt #"
              value={player.shirtNumber?.toString() ?? "—"}
              mono
            />
            <div className="flex justify-between items-center py-2 last:border-0">
              <span className="font-body-sm text-body-sm text-on-surface-variant">
                Status
              </span>
              <span className="inline-flex items-center gap-1.5 px-2 py-0.5 rounded-full bg-surface-container-highest border border-outline-variant font-label-caps text-label-caps uppercase text-on-surface">
                <span
                  className={`w-1.5 h-1.5 rounded-full ${statusDotClass}`}
                />
                {player.status.status}
              </span>
            </div>
          </div>
        </div>

        {/* Appearances */}
        <div className="col-span-12 lg:col-span-9">
          <div className="mb-4">
            <h3 className="font-headline-sm text-headline-sm text-on-surface">
              Appearances
            </h3>
            <p className="font-body-sm text-body-sm text-on-surface-variant">
              Match participation history
            </p>
          </div>

          {appearances.isLoading && (
            <LoadingState label="Loading appearances..." />
          )}
          {appearances.error && <ErrorState error={appearances.error} />}

          {appearances.data && (
            <div className="space-y-3">
              <AppearanceList appearances={appearances.data.data} />
              <Pagination
                pagination={appearances.data.pagination}
                onPageChange={setPage}
              />
            </div>
          )}
        </div>
      </div>
    </div>
  );
}

function OverviewRow({
  label,
  value,
  mono = false,
}: {
  label: string;
  value: string;
  mono?: boolean;
}) {
  return (
    <div className="flex justify-between py-2 border-b border-outline-variant/50 last:border-0">
      <span className="font-body-sm text-body-sm text-on-surface-variant">
        {label}
      </span>
      <span
        className={
          mono
            ? "font-data-mono text-data-mono text-on-surface text-right"
            : "font-body-sm text-body-sm text-on-surface font-semibold text-right"
        }
      >
        {value}
      </span>
    </div>
  );
}

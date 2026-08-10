import { useState } from "react";
import { useParams } from "react-router-dom";
import { usePlayer, usePlayerAppearances } from "../features/players/hooks";
import { AppearanceList } from "../components/player/AppearanceList";
import { Pagination } from "../components/common/Pagination";
import { LoadingState, ErrorState } from "../components/common/States";

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

  return (
    <div className="space-y-6">
      <div className="rounded-lg border border-surface-border bg-surface p-6">
        <div className="flex items-start gap-4">
          <div className="flex h-14 w-14 shrink-0 items-center justify-center rounded-full bg-brand-100 text-lg font-semibold text-brand-600">
            {player.shirtNumber ?? "?"}
          </div>
          <div>
            <h1 className="text-xl font-semibold text-ink">{player.name}</h1>
            <p className="mt-1 text-sm text-ink-muted">
              {[
                player.currentTeam?.teamName,
                player.preferredPosition?.positionName,
                player.nationality,
              ]
                .filter(Boolean)
                .join(" · ") || "—"}
            </p>
          </div>
        </div>

        <dl className="mt-4 grid grid-cols-2 gap-3 text-sm sm:grid-cols-4">
          <Field label="Status" value={player.status.status} />
          <Field
            label="Contract until"
            value={player.contract.contractUntil ?? "—"}
          />
          <Field
            label="Market value"
            value={
              player.contract.marketValue != null
                ? `€${player.contract.marketValue.toLocaleString()}`
                : "—"
            }
          />
          <Field
            label="Date of birth"
            value={player.dateOfBirth ?? "—"}
          />
        </dl>

        {player.status.injuryDescription && (
          <p className="mt-3 rounded bg-warning/10 px-3 py-2 text-sm text-warning">
            {player.status.injuryDescription}
          </p>
        )}
      </div>

      <section>
        <h2 className="mb-3 text-sm font-semibold uppercase tracking-wide text-ink-muted">
          Appearance History
        </h2>

        {appearances.isLoading && <LoadingState label="Loading appearances..." />}
        {appearances.error && <ErrorState error={appearances.error} />}

        {appearances.data && (
          <div className="space-y-3">
            <AppearanceList appearances={appearances.data.appearances} />
            <Pagination
              page={appearances.data.page}
              pageSize={appearances.data.pageSize}
              totalCount={appearances.data.totalCount}
              onPageChange={setPage}
            />
          </div>
        )}
      </section>
    </div>
  );
}

function Field({ label, value }: { label: string; value: string }) {
  return (
    <div>
      <dt className="text-xs uppercase tracking-wide text-ink-faint">
        {label}
      </dt>
      <dd className="mt-0.5 font-medium text-ink">{value}</dd>
    </div>
  );
}

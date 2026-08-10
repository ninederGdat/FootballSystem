import { useState } from "react";
import { usePlayerSearch } from "../features/players/hooks";
import { PlayerCard } from "../components/player/PlayerCard";
import { Pagination } from "../components/common/Pagination";
import {
  LoadingState,
  ErrorState,
  EmptyState,
} from "../components/common/States";

const PAGE_SIZE = 20;

export function PlayerListPage() {
  const [search, setSearch] = useState("");
  const [positionCode, setPositionCode] = useState("");
  const [nationality, setNationality] = useState("");
  const [page, setPage] = useState(1);

  const { data, isLoading, error } = usePlayerSearch({
    search: search || undefined,
    positionCode: positionCode || undefined,
    nationality: nationality || undefined,
    page,
    pageSize: PAGE_SIZE,
  });

  return (
    <div className="space-y-6">
      <h1 className="text-xl font-semibold text-ink">Players</h1>

      <div className="flex flex-wrap gap-2">
        <input
          value={search}
          onChange={(e) => {
            setSearch(e.target.value);
            setPage(1);
          }}
          placeholder="Search by name..."
          className="min-w-[200px] flex-1 rounded-md border border-surface-border bg-surface px-3 py-2 text-sm outline-none focus:border-brand-500"
        />
        <input
          value={positionCode}
          onChange={(e) => {
            setPositionCode(e.target.value);
            setPage(1);
          }}
          placeholder="Position code (e.g. LW)"
          className="w-44 rounded-md border border-surface-border bg-surface px-3 py-2 text-sm outline-none focus:border-brand-500"
        />
        <input
          value={nationality}
          onChange={(e) => {
            setNationality(e.target.value);
            setPage(1);
          }}
          placeholder="Nationality"
          className="w-40 rounded-md border border-surface-border bg-surface px-3 py-2 text-sm outline-none focus:border-brand-500"
        />
      </div>

      {isLoading && <LoadingState label="Searching players..." />}
      {error && <ErrorState error={error} />}

      {data && data.items.length === 0 && (
        <EmptyState
          title="No players found"
          description="Try a different search or clear the filters."
        />
      )}

      {data && data.items.length > 0 && (
        <>
          <div className="grid grid-cols-1 gap-3 sm:grid-cols-2 lg:grid-cols-3">
            {data.items.map((p) => (
              <PlayerCard key={p.playerId} player={p} />
            ))}
          </div>
          <Pagination
            page={data.page}
            pageSize={data.pageSize}
            totalCount={data.totalCount}
            onPageChange={setPage}
          />
        </>
      )}
    </div>
  );
}

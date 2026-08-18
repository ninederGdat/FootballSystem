import { Navigate, Route, Routes } from "react-router-dom";
import { AppShell } from "./components/layout/AppShell";
import { DashboardPage } from "./routes/DashboardPage";
import { MatchListPage } from "./routes/MatchListPage";
import { MatchDetailPage } from "./routes/MatchDetailPage";
import { PlayerListPage } from "./routes/PlayerListPage";
import { PlayerDetailPage } from "./routes/PlayerDetailPage";

export function App() {
  return (
    <Routes>
      <Route element={<AppShell />}>
        <Route index element={<DashboardPage />} />
        <Route path="matches" element={<MatchListPage />} />
        <Route path="matches/:matchId" element={<MatchDetailPage />} />
        <Route path="players" element={<PlayerListPage />} />
        <Route path="players/:playerId" element={<PlayerDetailPage />} />
        <Route path="*" element={<Navigate to="/matches" replace />} />
      </Route>
    </Routes>
  );
}

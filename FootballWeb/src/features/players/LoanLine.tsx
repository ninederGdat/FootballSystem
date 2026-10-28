import type { PlayerSummaryDTO } from "../../types/player";
import { formatLoanDate } from "../../lib/format";

// loaned_in: parentTeam = owning club ("from X"). loaned_out: currentTeam =
// the club the player is currently at ("at X"). Other relations render nothing.
export function LoanLine({ player }: { player: PlayerSummaryDTO }) {
  if (player.relation !== "loaned_in" && player.relation !== "loaned_out") {
    return null;
  }
  const loanedOut = player.relation === "loaned_out";
  const club = loanedOut
    ? player.currentTeam?.teamName
    : player.parentTeam?.teamName;
  if (!club) return null;
  const until = player.loanEndDate ? formatLoanDate(player.loanEndDate) : null;
  return (
    <p className="mt-1 font-body-sm text-body-sm text-on-surface-variant">
      {loanedOut ? `On loan at ${club}` : `On loan from ${club}`} until{" "}
      {until ?? "unknown date"}
    </p>
  );
}

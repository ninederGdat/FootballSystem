import type { PlayerTeamDTO } from "../../types/player";
import { formatLoanDate } from "../../lib/format";

export function ProfileLoanLine({
  currentTeam,
  parentTeam,
  loanEndDate,
}: {
  currentTeam: PlayerTeamDTO | null;
  parentTeam: PlayerTeamDTO | null;
  loanEndDate: string | null;
}) {
  if (!parentTeam || !currentTeam) return null;
  return (
    <p className="mt-1 font-body-sm text-body-sm text-on-surface-variant">
      On loan at {currentTeam.teamName} from {parentTeam.teamName} until{" "}
      {loanEndDate ? formatLoanDate(loanEndDate) : "unknown date"}
    </p>
  );
}

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
    <div className="inline-flex items-center gap-1.5 text-secondary font-body-sm text-body-sm pt-0.5">
      <span className="material-symbols-outlined text-[16px] text-primary">
        sync_alt
      </span>
      <span>
        On loan at {currentTeam.teamName} from {parentTeam.teamName} until{" "}
        {loanEndDate ? formatLoanDate(loanEndDate) : "unknown date"}
      </span>
    </div>
  );
}

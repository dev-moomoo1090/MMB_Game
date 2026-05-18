namespace MMBGame
{
    public class RearDeployAction : FiscalAction
    {
        public override string ActionName => "RearDeployAction";
        public override bool RequiresPieceSelection => true;
        public override bool RequiresValueInput => false;

        public override bool Execute(ChessPiece targetPiece, PlayerState actor, int inputValue, PoliticsManager manager)
        {
            if (targetPiece == null || actor == null || manager == null || targetPiece.isOffBoard)
            {
                return false;
            }

            if (!manager.BoardManager.RearDeploy(targetPiece))
            {
                return false;
            }

            PoliticalStatService.ChangeSupport(targetPiece, 30, ActionName);
            int beforeGoldPerTurn = actor.goldPerTurn;
            actor.goldPerTurn += targetPiece.taxPerTurn * 2;
            QaLog.Write("턴수입", "변경 행동=" + ActionName + " 색상=" + actor.color + " 변화량=" + (targetPiece.taxPerTurn * 2) + " 이전=" + beforeGoldPerTurn + " 이후=" + actor.goldPerTurn + " 대상=" + QaLog.PieceLabel(targetPiece));
            return true;
        }
    }

    public class FrontDeployAction : FiscalAction
    {
        public override string ActionName => "FrontDeployAction";
        public override bool RequiresPieceSelection => true;
        public override bool RequiresValueInput => false;

        public override bool Execute(ChessPiece targetPiece, PlayerState actor, int inputValue, PoliticsManager manager)
        {
            if (targetPiece == null || actor == null || manager == null || !targetPiece.isOffBoard)
            {
                return false;
            }

            if (!manager.BoardManager.FrontDeploy(targetPiece))
            {
                return false;
            }

            PoliticalStatService.ChangeSupport(targetPiece, -50, ActionName);
            int beforeGoldPerTurn = actor.goldPerTurn;
            actor.goldPerTurn -= targetPiece.taxPerTurn * 2;
            QaLog.Write("턴수입", "변경 행동=" + ActionName + " 색상=" + actor.color + " 변화량=" + (-targetPiece.taxPerTurn * 2) + " 이전=" + beforeGoldPerTurn + " 이후=" + actor.goldPerTurn + " 대상=" + QaLog.PieceLabel(targetPiece));
            return true;
        }
    }
}

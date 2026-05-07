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

            targetPiece.support += 30;
            actor.goldPerTurn += targetPiece.taxPerTurn * 2;
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

            targetPiece.support -= 50;
            actor.goldPerTurn -= targetPiece.taxPerTurn * 2;
            return true;
        }
    }
}

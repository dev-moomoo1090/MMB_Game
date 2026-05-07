namespace MMBGame
{
    public class AidAction : FiscalAction
    {
        public override string ActionName => "AidAction";
        public override bool RequiresPieceSelection => true;
        public override bool RequiresValueInput => true;

        public override bool Execute(ChessPiece targetPiece, PlayerState actor, int inputValue, PoliticsManager manager)
        {
            if (targetPiece == null || actor == null || inputValue < 0)
            {
                return false;
            }

            if (!actor.SpendGold(inputValue))
            {
                return false;
            }

            PoliticalStatService.ChangeSupport(targetPiece, System.Math.Max(1, inputValue / System.Math.Max(1, targetPiece.taxPerTurn)), ActionName);
            return true;
        }
    }

    public class RequisitionAction : FiscalAction
    {
        public override string ActionName => "RequisitionAction";
        public override bool RequiresPieceSelection => true;
        public override bool RequiresValueInput => true;

        public override bool Execute(ChessPiece targetPiece, PlayerState actor, int inputValue, PoliticsManager manager)
        {
            if (targetPiece == null || actor == null || inputValue < 0)
            {
                return false;
            }

            PoliticalStatService.ChangeSupport(targetPiece, -System.Math.Max(1, inputValue / System.Math.Max(1, targetPiece.taxPerTurn)), ActionName);
            actor.AddGold(inputValue);
            return true;
        }
    }
}

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

            targetPiece.support += System.Math.Max(1, inputValue / 10);
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

            targetPiece.support -= System.Math.Max(1, inputValue / 10);
            actor.AddGold(inputValue);
            return true;
        }
    }
}

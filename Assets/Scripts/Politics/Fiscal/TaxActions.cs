namespace MMBGame
{
    public class SpecialTaxAction : FiscalAction
    {
        public override string ActionName => "SpecialTax";
        public override bool RequiresPieceSelection => true;
        public override bool RequiresValueInput => false;

        public override bool Execute(ChessPiece targetPiece, PlayerState actor, int inputValue, PoliticsManager manager)
        {
            if (targetPiece == null)
            {
                return false;
            }

            targetPiece.taxModifier = 2;
            PoliticalStatService.ChangeSupport(targetPiece, -5, ActionName);
            return true;
        }
    }

    public class TaxExemptionAction : FiscalAction
    {
        public override string ActionName => "TaxExemption";
        public override bool RequiresPieceSelection => true;
        public override bool RequiresValueInput => false;

        public override bool Execute(ChessPiece targetPiece, PlayerState actor, int inputValue, PoliticsManager manager)
        {
            if (targetPiece == null)
            {
                return false;
            }

            targetPiece.taxModifier = 0;
            PoliticalStatService.ChangeSupport(targetPiece, 5, ActionName);
            return true;
        }
    }
}

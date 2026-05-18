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

            int beforeTaxModifier = targetPiece.taxModifier;
            targetPiece.taxModifier = 2;
            QaLog.Write("세금", "세금 배율 변경 행동=" + ActionName + " 기물=" + QaLog.PieceLabel(targetPiece) + " 이전=" + beforeTaxModifier + " 이후=" + targetPiece.taxModifier);
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

            int beforeTaxModifier = targetPiece.taxModifier;
            targetPiece.taxModifier = 0;
            QaLog.Write("세금", "세금 배율 변경 행동=" + ActionName + " 기물=" + QaLog.PieceLabel(targetPiece) + " 이전=" + beforeTaxModifier + " 이후=" + targetPiece.taxModifier);
            PoliticalStatService.ChangeSupport(targetPiece, 5, ActionName);
            return true;
        }
    }
}

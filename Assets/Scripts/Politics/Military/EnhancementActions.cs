namespace MMBGame
{
    public class OutpostAction : MilitaryAction
    {
        public override string ActionName => "OutpostAction";
        public override bool RequiresPieceSelection => true;
        public override bool RequiresPositionSelection => false;

        public override bool Execute(ChessPiece targetPiece, int targetFile, int targetRank, PieceColor actorColor, MilitaryManager manager)
        {
            if (!IsValidTarget(targetPiece, actorColor, PieceType.Knight, manager))
            {
                return false;
            }

            DelayedEffect effect = new DelayedEffect(DelayedEffectType.ApplyEnhancement, targetPiece.file, targetPiece.rank, actorColor, 0);
            effect.targetPiece = targetPiece;
            int deltaRank = actorColor == PieceColor.White ? 1 : -1;
            effect.patternsToAdd.Add(new MovePattern(1, deltaRank, false, true));
            effect.patternsToAdd.Add(new MovePattern(-1, deltaRank, false, true));
            manager.AddDelayedEffect(effect);
            return true;
        }

        private bool IsValidTarget(ChessPiece targetPiece, PieceColor actorColor, PieceType expectedType, MilitaryManager manager)
        {
            return targetPiece != null &&
                targetPiece.color == actorColor &&
                targetPiece.type == expectedType &&
                manager != null &&
                manager.BoardManager != null &&
                !CheckDetector.IsInCheck(manager.BoardManager.BoardState, actorColor);
        }
    }

    public class DivinePowerAction : MilitaryAction
    {
        public override string ActionName => "DivinePowerAction";
        public override bool RequiresPieceSelection => true;
        public override bool RequiresPositionSelection => false;

        public override bool Execute(ChessPiece targetPiece, int targetFile, int targetRank, PieceColor actorColor, MilitaryManager manager)
        {
            if (!IsValidTarget(targetPiece, actorColor, PieceType.Rook, manager))
            {
                return false;
            }

            DelayedEffect effect = new DelayedEffect(DelayedEffectType.ApplyEnhancement, targetPiece.file, targetPiece.rank, actorColor, 0);
            effect.targetPiece = targetPiece;
            effect.patternsToAdd.Add(new MovePattern(1, 0, true, false, false, true));
            effect.patternsToAdd.Add(new MovePattern(-1, 0, true, false, false, true));
            effect.patternsToAdd.Add(new MovePattern(0, 1, true, false, false, true));
            effect.patternsToAdd.Add(new MovePattern(0, -1, true, false, false, true));
            manager.AddDelayedEffect(effect);
            return true;
        }

        private bool IsValidTarget(ChessPiece targetPiece, PieceColor actorColor, PieceType expectedType, MilitaryManager manager)
        {
            return targetPiece != null &&
                targetPiece.color == actorColor &&
                targetPiece.type == expectedType &&
                manager != null &&
                manager.BoardManager != null &&
                !CheckDetector.IsInCheck(manager.BoardManager.BoardState, actorColor);
        }
    }

    public class MiracleAction : MilitaryAction
    {
        public override string ActionName => "MiracleAction";
        public override bool RequiresPieceSelection => true;
        public override bool RequiresPositionSelection => false;

        public override bool Execute(ChessPiece targetPiece, int targetFile, int targetRank, PieceColor actorColor, MilitaryManager manager)
        {
            if (!IsValidTarget(targetPiece, actorColor, PieceType.Bishop, manager))
            {
                return false;
            }

            DelayedEffect effect = new DelayedEffect(DelayedEffectType.ApplyEnhancement, targetPiece.file, targetPiece.rank, actorColor, 0);
            effect.targetPiece = targetPiece;
            int deltaRank = actorColor == PieceColor.White ? 1 : -1;
            effect.patternsToAdd.Add(new MovePattern(0, deltaRank, false, false, true));
            manager.AddDelayedEffect(effect);
            return true;
        }

        private bool IsValidTarget(ChessPiece targetPiece, PieceColor actorColor, PieceType expectedType, MilitaryManager manager)
        {
            return targetPiece != null &&
                targetPiece.color == actorColor &&
                targetPiece.type == expectedType &&
                manager != null &&
                manager.BoardManager != null &&
                !CheckDetector.IsInCheck(manager.BoardManager.BoardState, actorColor);
        }
    }

    public class MilitaryExemptionAction : MilitaryAction
    {
        public override string ActionName => "MilitaryExemptionAction";
        public override bool RequiresPieceSelection => true;
        public override bool RequiresPositionSelection => false;

        public override bool Execute(ChessPiece targetPiece, int targetFile, int targetRank, PieceColor actorColor, MilitaryManager manager)
        {
            if (!IsValidTarget(targetPiece, actorColor, PieceType.Pawn, manager))
            {
                return false;
            }

            DelayedEffect effect = new DelayedEffect(DelayedEffectType.ApplyEnhancement, targetPiece.file, targetPiece.rank, actorColor, 0);
            effect.targetPiece = targetPiece;
            int deltaRank = actorColor == PieceColor.White ? -1 : 1;
            effect.patternsToAdd.Add(new MovePattern(0, deltaRank, false, false, true));
            manager.AddDelayedEffect(effect);
            return true;
        }

        private bool IsValidTarget(ChessPiece targetPiece, PieceColor actorColor, PieceType expectedType, MilitaryManager manager)
        {
            return targetPiece != null &&
                targetPiece.color == actorColor &&
                targetPiece.type == expectedType &&
                manager != null &&
                manager.BoardManager != null &&
                !CheckDetector.IsInCheck(manager.BoardManager.BoardState, actorColor);
        }
    }
}

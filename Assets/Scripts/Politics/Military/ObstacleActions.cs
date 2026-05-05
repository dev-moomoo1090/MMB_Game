namespace MMBGame
{
    public class BarricadeAction : MilitaryAction
    {
        public override string ActionName => "BarricadeAction";
        public override bool RequiresPieceSelection => false;
        public override bool RequiresPositionSelection => true;

        public override bool Execute(ChessPiece targetPiece, int targetFile, int targetRank, PieceColor actorColor, MilitaryManager manager)
        {
            if (!IsValidRank(targetRank, actorColor))
            {
                return false;
            }

            if (manager.BoardManager == null || manager.BoardManager.BoardState.GetPiece(targetFile, targetRank) != null || HasBarricade(manager))
            {
                return false;
            }

            manager.AddDelayedEffect(new DelayedEffect(DelayedEffectType.PlaceBarricade, targetFile, targetRank, actorColor, 1));
            return true;
        }

        private bool HasBarricade(MilitaryManager manager)
        {
            BoardState state = manager.BoardManager.BoardState;
            for (int file = 0; file < 8; file++)
            {
                for (int rank = 0; rank < 8; rank++)
                {
                    ChessPiece piece = state.GetPiece(file, rank);
                    if (piece != null && piece.type == PieceType.Barricade)
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        private bool IsValidRank(int rank, PieceColor color)
        {
            if (color == PieceColor.White)
            {
                return rank >= 0 && rank <= 2;
            }

            if (color == PieceColor.Black)
            {
                return rank >= 5 && rank <= 7;
            }

            return false;
        }
    }

    public class TrebuchetAction : MilitaryAction
    {
        public override string ActionName => "TrebuchetAction";
        public override bool RequiresPieceSelection => false;
        public override bool RequiresPositionSelection => true;

        public override bool Execute(ChessPiece targetPiece, int targetFile, int targetRank, PieceColor actorColor, MilitaryManager manager)
        {
            if (!IsValidRank(targetRank, actorColor))
            {
                return false;
            }

            if (manager.BoardManager == null || manager.BoardManager.BoardState.GetPiece(targetFile, targetRank) != null || manager.HasTrebuchet(actorColor))
            {
                return false;
            }

            manager.AddDelayedEffect(new DelayedEffect(DelayedEffectType.PlaceTrebuchet, targetFile, targetRank, actorColor, 5));
            return true;
        }

        private bool IsValidRank(int rank, PieceColor color)
        {
            if (color == PieceColor.White)
            {
                return rank >= 0 && rank <= 2;
            }

            if (color == PieceColor.Black)
            {
                return rank >= 5 && rank <= 7;
            }

            return false;
        }
    }
}

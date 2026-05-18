namespace MMBGame
{
    public partial class MilitaryManager
    {
        public bool HasTrebuchet(PieceColor color)
        {
            if (boardManager == null || boardManager.BoardState == null)
            {
                return false;
            }

            for (int file = 0; file < 8; file++)
            {
                for (int rank = 0; rank < 8; rank++)
                {
                    ChessPiece piece = boardManager.BoardState.GetPiece(file, rank);
                    if (piece != null && piece.type == PieceType.Trebuchet && piece.color == color)
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        public ChessPiece GetTrebuchet(PieceColor color)
        {
            if (boardManager == null || boardManager.BoardState == null)
            {
                return null;
            }

            for (int file = 0; file < 8; file++)
            {
                for (int rank = 0; rank < 8; rank++)
                {
                    ChessPiece piece = boardManager.BoardState.GetPiece(file, rank);
                    if (piece != null && piece.type == PieceType.Trebuchet && piece.color == color)
                    {
                        return piece;
                    }
                }
            }

            return null;
        }

        public bool HasPendingBombard(PieceColor color)
        {
            for (int i = 0; i < pendingEffects.Count; i++)
            {
                DelayedEffect effect = pendingEffects[i];
                if (effect.effectType == DelayedEffectType.Bombard && effect.ownerColor == color)
                {
                    return true;
                }
            }

            return false;
        }
    }
}

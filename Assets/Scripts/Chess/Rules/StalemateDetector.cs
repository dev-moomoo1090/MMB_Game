namespace MMBGame
{
    public static class StalemateDetector
    {
        public static bool IsStalemate(BoardState state, PieceColor color)
        {
            if (CheckDetector.IsInCheck(state, color)) return false;
            return MoveValidator.GetAllLegalMoves(state, color).Count == 0;
        }
    }
}
namespace MMBGame
{
    public partial class BoardManager
    {
        private bool WasKingCaptured()
        {
            for (int i = 0; i < BoardState.capturedThisTurn.Count; i++)
            {
                ChessPiece piece = BoardState.capturedThisTurn[i];
                if (piece != null && piece.type == PieceType.King)
                {
                    return true;
                }
            }

            return false;
        }

        private bool TryGetKingCaptureWinner(out PieceColor winner)
        {
            bool whiteKingAlive = BoardState.HasKing(PieceColor.White);
            bool blackKingAlive = BoardState.HasKing(PieceColor.Black);
            if (whiteKingAlive && !blackKingAlive)
            {
                winner = PieceColor.White;
                return true;
            }

            if (!whiteKingAlive && blackKingAlive)
            {
                winner = PieceColor.Black;
                return true;
            }

            winner = PieceColor.None;
            return !whiteKingAlive && !blackKingAlive;
        }

        private PieceColor GetOpponent(PieceColor color)
        {
            if (color == PieceColor.White)
            {
                return PieceColor.Black;
            }

            if (color == PieceColor.Black)
            {
                return PieceColor.White;
            }

            return PieceColor.None;
        }
    }
}

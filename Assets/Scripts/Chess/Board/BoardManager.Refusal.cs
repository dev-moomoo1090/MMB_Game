namespace MMBGame
{
    public partial class BoardManager
    {
        private bool IsMoveRefused(Move move)
        {
            return move != null && refusedMoveKeys.Contains(GetMoveRefusalKey(move));
        }

        private void MarkMoveRefused(Move move, ChessPiece piece)
        {
            if (move == null)
            {
                return;
            }

            refusedMoveKeys.Add(GetMoveRefusalKey(move));
            QaLog.Write("명령수락", "거부된 행마 잠금 기물=" + QaLog.PieceLabel(piece) + " 출발=(" + move.fromFile + "," + move.fromRank + ") 목적지=(" + move.toFile + "," + move.toRank + ") 특수수=" + move.specialMove + " 승격=" + move.promotionPiece);
        }

        private void HandlePhaseChanged(GamePhase phase)
        {
            if (phase != GamePhase.ChessPhase)
            {
                refusedMoveKeys.Clear();
            }
        }

        private void HandleTurnChanged(PieceColor color)
        {
            refusedMoveKeys.Clear();
        }

        private string GetMoveRefusalKey(Move move)
        {
            return move.fromFile + ":" + move.fromRank + ":" + move.toFile + ":" + move.toRank + ":" + move.specialMove + ":" + move.promotionPiece;
        }
    }
}

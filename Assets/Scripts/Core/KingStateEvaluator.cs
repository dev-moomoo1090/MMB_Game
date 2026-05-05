namespace MMBGame
{
    public static class KingStateEvaluator
    {
        public static KingState Evaluate(PlayerState player, BoardState board)
        {
            int totalSupport = ComputeTotalSupport(player.color, board);
            int pieceCount = CountPieces(player.color, board);
            int baseline = pieceCount * 50;
            int supportDelta = totalSupport - baseline;

            bool highSupport = supportDelta >= 4;
            bool lowSupport = supportDelta <= -4;
            bool highHonor = player.honor >= 20;
            bool lowHonor = player.honor <= -20;

            if (highSupport && highHonor)
            {
                return KingState.Sage;
            }

            if (lowSupport && highHonor)
            {
                return KingState.DarkKing;
            }

            if (highSupport && lowHonor)
            {
                return KingState.Autocrat;
            }

            if (lowSupport && lowHonor)
            {
                return KingState.Tyrant;
            }

            return KingState.Neutral;
        }

        public static int ComputeTotalSupport(PieceColor color, BoardState board)
        {
            int total = 0;
            for (int file = 0; file < 8; file++)
            {
                for (int rank = 0; rank < 8; rank++)
                {
                    ChessPiece piece = board.GetPiece(file, rank);
                    if (piece != null && piece.color == color && piece.support >= 50)
                    {
                        total += piece.support;
                    }
                }
            }

            return total;
        }

        private static int CountPieces(PieceColor color, BoardState board)
        {
            int count = 0;
            for (int file = 0; file < 8; file++)
            {
                for (int rank = 0; rank < 8; rank++)
                {
                    ChessPiece piece = board.GetPiece(file, rank);
                    if (piece != null && piece.color == color)
                    {
                        count++;
                    }
                }
            }

            return count;
        }
    }
}

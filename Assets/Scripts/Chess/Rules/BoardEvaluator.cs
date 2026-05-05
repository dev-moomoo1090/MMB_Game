namespace MMBGame
{
    public static class BoardEvaluator
    {
        public static int CountAttackers(BoardState state, int file, int rank, PieceColor byColor)
        {
            int count = 0;
            int pawnDirection = byColor == PieceColor.White ? 1 : -1;

            for (int fileOffset = -1; fileOffset <= 1; fileOffset += 2)
            {
                ChessPiece piece = state.GetPiece(file + fileOffset, rank - pawnDirection);
                if (piece != null && piece.color == byColor && piece.type == PieceType.Pawn)
                {
                    count++;
                }
            }

            int[][] knightLeaps = new int[][]
            {
                new int[] { 1, 2 }, new int[] { 2, 1 }, new int[] { 2, -1 }, new int[] { 1, -2 },
                new int[] { -1, -2 }, new int[] { -2, -1 }, new int[] { -2, 1 }, new int[] { -1, 2 }
            };

            foreach (int[] leap in knightLeaps)
            {
                ChessPiece piece = state.GetPiece(file + leap[0], rank + leap[1]);
                if (piece != null && piece.color == byColor && piece.type == PieceType.Knight)
                {
                    count++;
                }
            }

            int[][] straightDirections = new int[][]
            {
                new int[] { 1, 0 }, new int[] { -1, 0 }, new int[] { 0, 1 }, new int[] { 0, -1 }
            };

            foreach (int[] direction in straightDirections)
            {
                int currentFile = file + direction[0];
                int currentRank = rank + direction[1];

                while (state.IsInBounds(currentFile, currentRank))
                {
                    ChessPiece piece = state.GetPiece(currentFile, currentRank);
                    if (piece != null)
                    {
                        if (piece.color == byColor && (piece.type == PieceType.Rook || piece.type == PieceType.Queen))
                        {
                            count++;
                        }
                        break;
                    }

                    currentFile += direction[0];
                    currentRank += direction[1];
                }
            }

            int[][] diagonalDirections = new int[][]
            {
                new int[] { 1, 1 }, new int[] { 1, -1 }, new int[] { -1, 1 }, new int[] { -1, -1 }
            };

            foreach (int[] direction in diagonalDirections)
            {
                int currentFile = file + direction[0];
                int currentRank = rank + direction[1];

                while (state.IsInBounds(currentFile, currentRank))
                {
                    ChessPiece piece = state.GetPiece(currentFile, currentRank);
                    if (piece != null)
                    {
                        if (piece.color == byColor && (piece.type == PieceType.Bishop || piece.type == PieceType.Queen))
                        {
                            count++;
                        }
                        break;
                    }

                    currentFile += direction[0];
                    currentRank += direction[1];
                }
            }

            for (int fileOffset = -1; fileOffset <= 1; fileOffset++)
            {
                for (int rankOffset = -1; rankOffset <= 1; rankOffset++)
                {
                    if (fileOffset == 0 && rankOffset == 0)
                    {
                        continue;
                    }

                    ChessPiece piece = state.GetPiece(file + fileOffset, rank + rankOffset);
                    if (piece != null && piece.color == byColor && piece.type == PieceType.King)
                    {
                        count++;
                    }
                }
            }

            return count;
        }

        public static int CountDefenders(BoardState state, int file, int rank, PieceColor color)
        {
            return CountAttackers(state, file, rank, color);
        }

        public static int GetPieceValue(PieceType type)
        {
            switch (type)
            {
                case PieceType.Pawn: return 1;
                case PieceType.Knight: return 3;
                case PieceType.Bishop: return 3;
                case PieceType.Rook: return 5;
                case PieceType.Queen: return 9;
                default: return 0;
            }
        }
    }
}

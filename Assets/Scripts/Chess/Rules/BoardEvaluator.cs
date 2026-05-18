using System.Collections.Generic;

namespace MMBGame
{
    public static class BoardEvaluator
    {
        public static int CountAttackers(BoardState state, int file, int rank, PieceColor byColor)
        {
            if (state == null || byColor == PieceColor.None || !state.IsInBounds(file, rank))
            {
                return 0;
            }

            int count = 0;
            for (int currentFile = 0; currentFile < 8; currentFile++)
            {
                for (int currentRank = 0; currentRank < 8; currentRank++)
                {
                    ChessPiece piece = state.GetPiece(currentFile, currentRank);
                    if (piece == null || PieceClassifier.IsObstacle(piece) || piece.GetMovementControllerColor() != byColor)
                    {
                        continue;
                    }

                    if (CanAttackSquare(state, piece, file, rank))
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

        private static bool CanAttackSquare(BoardState state, ChessPiece piece, int targetFile, int targetRank)
        {
            if (piece.file == targetFile && piece.rank == targetRank)
            {
                return false;
            }

            if (piece.type == PieceType.Pawn)
            {
                return PawnAttacksSquare(piece, targetFile, targetRank) ||
                    PatternsAttackSquare(state, piece, piece.oneTimeMovePatterns, targetFile, targetRank);
            }

            return PatternsAttackSquare(state, piece, piece.currentMovePatterns, targetFile, targetRank) ||
                PatternsAttackSquare(state, piece, piece.oneTimeMovePatterns, targetFile, targetRank);
        }

        private static bool PawnAttacksSquare(ChessPiece pawn, int targetFile, int targetRank)
        {
            PieceColor moveColor = pawn.GetMovementControllerColor();
            int direction = moveColor == PieceColor.White ? 1 : -1;
            return pawn.file + direction == targetFile && (pawn.rank + 1 == targetRank || pawn.rank - 1 == targetRank);
        }

        private static bool PatternsAttackSquare(BoardState state, ChessPiece piece, List<MovePattern> patterns, int targetFile, int targetRank)
        {
            if (patterns == null)
            {
                return false;
            }

            for (int i = 0; i < patterns.Count; i++)
            {
                MovePattern pattern = patterns[i];
                if (pattern == null || pattern.moveOnly)
                {
                    continue;
                }

                if (pattern.isCannon)
                {
                    if (CannonPatternAttacksSquare(state, piece, pattern, targetFile, targetRank))
                    {
                        return true;
                    }

                    continue;
                }

                if (pattern.isSliding)
                {
                    if (SlidingPatternAttacksSquare(state, piece, pattern, targetFile, targetRank))
                    {
                        return true;
                    }

                    continue;
                }

                if (piece.file + pattern.deltaFile == targetFile && piece.rank + pattern.deltaRank == targetRank)
                {
                    return true;
                }
            }

            return false;
        }

        private static bool SlidingPatternAttacksSquare(BoardState state, ChessPiece piece, MovePattern pattern, int targetFile, int targetRank)
        {
            int file = piece.file + pattern.deltaFile;
            int rank = piece.rank + pattern.deltaRank;
            while (state.IsInBounds(file, rank))
            {
                if (file == targetFile && rank == targetRank)
                {
                    return true;
                }

                ChessPiece blocker = state.GetPiece(file, rank);
                if (blocker != null)
                {
                    return false;
                }

                file += pattern.deltaFile;
                rank += pattern.deltaRank;
            }

            return false;
        }

        private static bool CannonPatternAttacksSquare(BoardState state, ChessPiece piece, MovePattern pattern, int targetFile, int targetRank)
        {
            bool jumpedPiece = false;
            int file = piece.file + pattern.deltaFile;
            int rank = piece.rank + pattern.deltaRank;
            while (state.IsInBounds(file, rank))
            {
                ChessPiece target = state.GetPiece(file, rank);
                if (file == targetFile && rank == targetRank)
                {
                    return jumpedPiece;
                }

                if (target != null)
                {
                    if (PieceClassifier.IsObstacle(target))
                    {
                        return false;
                    }

                    if (jumpedPiece)
                    {
                        return false;
                    }

                    jumpedPiece = true;
                }

                file += pattern.deltaFile;
                rank += pattern.deltaRank;
            }

            return false;
        }
    }
}

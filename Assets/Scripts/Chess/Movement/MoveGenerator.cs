using System.Collections.Generic;

namespace MMBGame
{
    public static class MoveGenerator
    {
        public static List<Move> GeneratePseudoLegalMoves(BoardState state, int file, int rank)
        {
            var piece = state.GetPiece(file, rank);
            if (piece == null) return new List<Move>();
            var moves = new List<Move>();
            if (piece.type == PieceType.Pawn)
            {
                GeneratePawnMoves(state, piece, moves);
                GenerateMovesFromPatterns(state, piece, piece.oneTimeMovePatterns, moves);
            }
            else if (piece.type == PieceType.King)
                GenerateKingMoves(state, piece, moves);
            else
                GeneratePatternMoves(state, piece, moves);
            return moves;
        }

        private static void GeneratePatternMoves(BoardState state, ChessPiece piece, List<Move> moves)
        {
            GenerateMovesFromPatterns(state, piece, piece.currentMovePatterns, moves);
            GenerateMovesFromPatterns(state, piece, piece.oneTimeMovePatterns, moves);
        }

        private static void GenerateMovesFromPatterns(BoardState state, ChessPiece piece, List<MovePattern> patterns, List<Move> moves)
        {
            foreach (MovePattern pat in patterns)
            {
                if (pat.isCannon)
                {
                    GenerateCannonMoves(state, piece, pat, moves);
                    continue;
                }

                if (pat.isSliding)
                {
                    int f = piece.file + pat.deltaFile, r = piece.rank + pat.deltaRank;
                    while (state.IsInBounds(f, r))
                    {
                        ChessPiece target = state.GetPiece(f, r);
                        if (target == null)
                        {
                            if (!pat.captureOnly) moves.Add(new Move(piece.file, piece.rank, f, r));
                            f += pat.deltaFile; r += pat.deltaRank;
                        }
                        else
                        {
                            if (IsObstacle(target))
                            {
                                break;
                            }

                            if (target.color != piece.color && !pat.moveOnly)
                                moves.Add(new Move(piece.file, piece.rank, f, r));
                            break;
                        }
                    }
                }
                else
                {
                    int f = piece.file + pat.deltaFile, r = piece.rank + pat.deltaRank;
                    if (!state.IsInBounds(f, r)) continue;
                    ChessPiece target = state.GetPiece(f, r);
                    if (target == null && !pat.captureOnly)
                        moves.Add(new Move(piece.file, piece.rank, f, r));
                    else if (target != null && !IsObstacle(target) && target.color != piece.color && !pat.moveOnly)
                        moves.Add(new Move(piece.file, piece.rank, f, r));
                }
            }
        }

        private static void GenerateCannonMoves(BoardState state, ChessPiece piece, MovePattern pattern, List<Move> moves)
        {
            bool jumpedPiece = false;
            int file = piece.file + pattern.deltaFile;
            int rank = piece.rank + pattern.deltaRank;

            while (state.IsInBounds(file, rank))
            {
                ChessPiece target = state.GetPiece(file, rank);
                if (target == null)
                {
                    if (!jumpedPiece && !pattern.captureOnly)
                    {
                        moves.Add(new Move(piece.file, piece.rank, file, rank));
                    }

                    file += pattern.deltaFile;
                    rank += pattern.deltaRank;
                    continue;
                }

                if (IsObstacle(target))
                {
                    break;
                }

                if (!jumpedPiece)
                {
                    jumpedPiece = true;
                    file += pattern.deltaFile;
                    rank += pattern.deltaRank;
                    continue;
                }

                if (target.color != piece.color && !pattern.moveOnly)
                {
                    moves.Add(new Move(piece.file, piece.rank, file, rank));
                }

                break;
            }
        }

        private static bool IsObstacle(ChessPiece piece)
        {
            return piece.type == PieceType.Barricade || piece.type == PieceType.Trebuchet;
        }

        private static void GeneratePawnMoves(BoardState state, ChessPiece pawn, List<Move> moves)
        {
            int dir = pawn.color == PieceColor.White ? 1 : -1;
            int startRank = pawn.color == PieceColor.White ? 1 : 6;
            int promoRank = pawn.color == PieceColor.White ? 7 : 0;
            int fwd = pawn.rank + dir;

            if (state.IsInBounds(pawn.file, fwd) && state.GetPiece(pawn.file, fwd) == null)
            {
                if (fwd == promoRank)
                    AddPromotionMoves(moves, pawn.file, pawn.rank, pawn.file, fwd);
                else
                {
                    moves.Add(new Move(pawn.file, pawn.rank, pawn.file, fwd));
                    if (pawn.rank == startRank && state.GetPiece(pawn.file, pawn.rank + dir * 2) == null)
                        moves.Add(new Move(pawn.file, pawn.rank, pawn.file, pawn.rank + dir * 2, SpecialMoveType.PawnDoubleAdvance));
                }
            }

            foreach (int df in new[] { -1, 1 })
            {
                int cf = pawn.file + df;
                if (!state.IsInBounds(cf, fwd)) continue;
                ChessPiece target = state.GetPiece(cf, fwd);
                if (target != null && !IsObstacle(target) && target.color != pawn.color)
                {
                    if (fwd == promoRank) AddPromotionMoves(moves, pawn.file, pawn.rank, cf, fwd);
                    else moves.Add(new Move(pawn.file, pawn.rank, cf, fwd));
                }
                if (state.enPassantAvailable && cf == state.enPassantFile && fwd == state.enPassantRank)
                    moves.Add(new Move(pawn.file, pawn.rank, cf, fwd, SpecialMoveType.EnPassant));
            }
        }

        private static void AddPromotionMoves(List<Move> moves, int ff, int fr, int tf, int tr)
        {
            moves.Add(new Move(ff, fr, tf, tr, SpecialMoveType.Promotion, PieceType.Queen));
            moves.Add(new Move(ff, fr, tf, tr, SpecialMoveType.Promotion, PieceType.Rook));
            moves.Add(new Move(ff, fr, tf, tr, SpecialMoveType.Promotion, PieceType.Bishop));
            moves.Add(new Move(ff, fr, tf, tr, SpecialMoveType.Promotion, PieceType.Knight));
        }

        private static void GenerateKingMoves(BoardState state, ChessPiece king, List<Move> moves)
        {
            GeneratePatternMoves(state, king, moves);
            if (king.hasMoved) return;
            int rank = king.rank;
            bool ksRight = king.color == PieceColor.White ? state.whiteKingsideCastle : state.blackKingsideCastle;
            bool qsRight = king.color == PieceColor.White ? state.whiteQueensideCastle : state.blackQueensideCastle;
            if (ksRight && state.GetPiece(5, rank) == null && state.GetPiece(6, rank) == null)
            {
                var rook = state.GetPiece(7, rank);
                if (rook != null && rook.type == PieceType.Rook && !rook.hasMoved)
                    moves.Add(new Move(king.file, rank, 6, rank, SpecialMoveType.CastleKingside));
            }
            if (qsRight && state.GetPiece(3, rank) == null && state.GetPiece(2, rank) == null && state.GetPiece(1, rank) == null)
            {
                var rook = state.GetPiece(0, rank);
                if (rook != null && rook.type == PieceType.Rook && !rook.hasMoved)
                    moves.Add(new Move(king.file, rank, 2, rank, SpecialMoveType.CastleQueenside));
            }
        }
    }
}

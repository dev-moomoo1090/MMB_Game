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
            int startFile = pawn.color == PieceColor.White ? 1 : 6;
            int promoFile = pawn.color == PieceColor.White ? 7 : 0;
            int fwd = pawn.file + dir;

            if (state.IsInBounds(fwd, pawn.rank) && state.GetPiece(fwd, pawn.rank) == null)
            {
                if (fwd == promoFile)
                    AddPromotionMoves(moves, pawn.file, pawn.rank, fwd, pawn.rank);
                else
                {
                    moves.Add(new Move(pawn.file, pawn.rank, fwd, pawn.rank));
                    if (pawn.file == startFile && state.GetPiece(pawn.file + dir * 2, pawn.rank) == null)
                        moves.Add(new Move(pawn.file, pawn.rank, pawn.file + dir * 2, pawn.rank, SpecialMoveType.PawnDoubleAdvance));
                }
            }

            foreach (int dr in new[] { -1, 1 })
            {
                int cr = pawn.rank + dr;
                if (!state.IsInBounds(fwd, cr)) continue;
                ChessPiece target = state.GetPiece(fwd, cr);
                if (target != null && !IsObstacle(target) && target.color != pawn.color)
                {
                    if (fwd == promoFile) AddPromotionMoves(moves, pawn.file, pawn.rank, fwd, cr);
                    else moves.Add(new Move(pawn.file, pawn.rank, fwd, cr));
                }
                if (state.enPassantAvailable && fwd == state.enPassantFile && cr == state.enPassantRank)
                    moves.Add(new Move(pawn.file, pawn.rank, fwd, cr, SpecialMoveType.EnPassant));
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
            int file = king.file;
            bool ksRight = king.color == PieceColor.White ? state.whiteKingsideCastle : state.blackKingsideCastle;
            bool qsRight = king.color == PieceColor.White ? state.whiteQueensideCastle : state.blackQueensideCastle;
            if (ksRight && state.GetPiece(file, 5) == null && state.GetPiece(file, 6) == null)
            {
                var rook = state.GetPiece(file, 7);
                if (rook != null && rook.type == PieceType.Rook && !rook.hasMoved)
                    moves.Add(new Move(file, king.rank, file, 6, SpecialMoveType.CastleKingside));
            }
            if (qsRight && state.GetPiece(file, 3) == null && state.GetPiece(file, 2) == null && state.GetPiece(file, 1) == null)
            {
                var rook = state.GetPiece(file, 0);
                if (rook != null && rook.type == PieceType.Rook && !rook.hasMoved)
                    moves.Add(new Move(file, king.rank, file, 2, SpecialMoveType.CastleQueenside));
            }
        }
    }
}

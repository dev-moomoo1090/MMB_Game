using System.Collections.Generic;

namespace MMBGame
{
    public static class MoveValidator
    {
        public static List<Move> GetLegalMoves(BoardState state, int file, int rank)
        {
            var piece = state.GetPiece(file, rank);
            if (piece == null) return new List<Move>();
            var pseudoMoves = MoveGenerator.GeneratePseudoLegalMoves(state, file, rank);
            var legalMoves = new List<Move>();
            foreach (var move in pseudoMoves)
            {
                if (move.specialMove == SpecialMoveType.CastleKingside || move.specialMove == SpecialMoveType.CastleQueenside)
                {
                    if (IsLegalCastle(state, move, piece)) legalMoves.Add(move);
                    continue;
                }
                var sim = SimulateMove(state, move);
                if (!CheckDetector.IsInCheck(sim, piece.color)) legalMoves.Add(move);
            }
            return legalMoves;
        }

        public static List<Move> GetAllLegalMoves(BoardState state, PieceColor color)
        {
            var all = new List<Move>();
            for (int f = 0; f < 8; f++)
                for (int r = 0; r < 8; r++)
                {
                    var p = state.GetPiece(f, r);
                    if (p != null && p.color == color) all.AddRange(GetLegalMoves(state, f, r));
                }
            return all;
        }

        public static BoardState SimulateMove(BoardState state, Move move)
        {
            var sim = state.Clone();
            SpecialMoves.ApplyMove(sim, move);
            return sim;
        }

        private static bool IsLegalCastle(BoardState state, Move move, ChessPiece king)
        {
            if (CheckDetector.IsInCheck(state, king.color)) return false;
            PieceColor enemy = king.color == PieceColor.White ? PieceColor.Black : PieceColor.White;
            int file = king.file;
            if (move.specialMove == SpecialMoveType.CastleKingside)
                return !CheckDetector.IsSquareAttacked(state, file, 5, enemy)
                    && !CheckDetector.IsSquareAttacked(state, file, 6, enemy);
            return !CheckDetector.IsSquareAttacked(state, file, 3, enemy)
                && !CheckDetector.IsSquareAttacked(state, file, 2, enemy);
        }
    }
}

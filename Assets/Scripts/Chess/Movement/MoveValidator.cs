using System.Collections.Generic;

namespace MMBGame
{
    public static class MoveValidator
    {
        public static List<Move> GetLegalMoves(BoardState state, int file, int rank)
        {
            var piece = state.GetPiece(file, rank);
            if (piece == null) return new List<Move>();
            return MoveGenerator.GeneratePseudoLegalMoves(state, file, rank);
        }

        public static List<Move> GetAllLegalMoves(BoardState state, PieceColor color)
        {
            var all = new List<Move>();
            for (int f = 0; f < 8; f++)
                for (int r = 0; r < 8; r++)
                {
                    var p = state.GetPiece(f, r);
                    if (p != null && p.GetMovementControllerColor() == color) all.AddRange(GetLegalMoves(state, f, r));
                }
            return all;
        }

        public static BoardState SimulateMove(BoardState state, Move move)
        {
            var sim = state.Clone();
            SpecialMoves.ApplyMove(sim, move);
            return sim;
        }

    }
}

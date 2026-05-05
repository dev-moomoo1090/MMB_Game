using System.Collections.Generic;

namespace MMBGame
{
    public static class DrawDetector
    {
        public static bool IsThreefoldRepetition(BoardState state) => state.IsThreefoldRepetition();

        public static bool IsInsufficientMaterial(BoardState state)
        {
            var pieces = new List<ChessPiece>();
            for (int f = 0; f < 8; f++)
                for (int r = 0; r < 8; r++)
                {
                    var p = state.GetPiece(f, r);
                    if (p != null) pieces.Add(p);
                }
            if (pieces.Count == 2) return true;
            if (pieces.Count == 3)
            {
                foreach (var p in pieces)
                    if (p.type == PieceType.Bishop || p.type == PieceType.Knight) return true;
            }
            if (pieces.Count == 4)
            {
                var bishops = pieces.FindAll(p => p.type == PieceType.Bishop);
                if (bishops.Count == 2 && pieces.FindAll(p => p.type == PieceType.King).Count == 2)
                {
                    bool sameSquareColor = (bishops[0].file + bishops[0].rank) % 2 == (bishops[1].file + bishops[1].rank) % 2;
                    if (sameSquareColor) return true;
                }
            }
            return false;
        }
    }
}
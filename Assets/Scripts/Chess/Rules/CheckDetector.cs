namespace MMBGame
{
    public static class CheckDetector
    {
        public static bool IsInCheck(BoardState state, PieceColor color)
        {
            int kf = -1, kr = -1;
            for (int f = 0; f < 8; f++)
                for (int r = 0; r < 8; r++)
                {
                    var p = state.GetPiece(f, r);
                    if (p != null && p.type == PieceType.King && p.color == color)
                    { kf = f; kr = r; }
                }
            if (kf == -1) return false;
            PieceColor enemy = color == PieceColor.White ? PieceColor.Black : PieceColor.White;
            return IsSquareAttacked(state, kf, kr, enemy);
        }

        public static bool IsSquareAttacked(BoardState state, int file, int rank, PieceColor byColor)
        {
            int pDir = byColor == PieceColor.White ? 1 : -1;
            for (int df = -1; df <= 1; df += 2)
            {
                var p = state.GetPiece(file + df, rank - pDir);
                if (p != null && p.type == PieceType.Pawn && p.color == byColor) return true;
            }

            int[][] knightLeaps = new int[][] {
                new int[]{1,2}, new int[]{2,1}, new int[]{2,-1}, new int[]{1,-2},
                new int[]{-1,-2}, new int[]{-2,-1}, new int[]{-2,1}, new int[]{-1,2}
            };
            foreach (var leap in knightLeaps)
            {
                var p = state.GetPiece(file + leap[0], rank + leap[1]);
                if (p != null && p.type == PieceType.Knight && p.color == byColor) return true;
            }

            int[][] straight = new int[][] {
                new int[]{1,0}, new int[]{-1,0}, new int[]{0,1}, new int[]{0,-1}
            };
            foreach (var d in straight)
            {
                int f = file + d[0], r = rank + d[1];
                while (state.IsInBounds(f, r))
                {
                    var p = state.GetPiece(f, r);
                    if (p != null)
                    {
                        if (p.color == byColor && (p.type == PieceType.Rook || p.type == PieceType.Queen)) return true;
                        break;
                    }
                    f += d[0]; r += d[1];
                }
            }

            int[][] diagonal = new int[][] {
                new int[]{1,1}, new int[]{1,-1}, new int[]{-1,1}, new int[]{-1,-1}
            };
            foreach (var d in diagonal)
            {
                int f = file + d[0], r = rank + d[1];
                while (state.IsInBounds(f, r))
                {
                    var p = state.GetPiece(f, r);
                    if (p != null)
                    {
                        if (p.color == byColor && (p.type == PieceType.Bishop || p.type == PieceType.Queen)) return true;
                        break;
                    }
                    f += d[0]; r += d[1];
                }
            }

            for (int df = -1; df <= 1; df++)
                for (int dr = -1; dr <= 1; dr++)
                {
                    if (df == 0 && dr == 0) continue;
                    var p = state.GetPiece(file + df, rank + dr);
                    if (p != null && p.type == PieceType.King && p.color == byColor) return true;
                }

            return false;
        }
    }
}
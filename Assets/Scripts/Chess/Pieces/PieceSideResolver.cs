namespace MMBGame
{
    public static class PieceSideResolver
    {
        public static PieceSide Resolve(PieceType type, int rank)
        {
            if (type == PieceType.King || type == PieceType.Queen || type == PieceType.Barricade || type == PieceType.Trebuchet)
            {
                return PieceSide.None;
            }

            return rank <= 3 ? PieceSide.Queenside : PieceSide.Kingside;
        }

        public static PieceLane ResolveLane(PieceType type, int rank)
        {
            if (type == PieceType.Barricade || type == PieceType.Trebuchet || rank < 0 || rank > 7)
            {
                return PieceLane.None;
            }

            return (PieceLane)(rank + 1);
        }

        public static int GetLaneIndex(PieceLane lane)
        {
            return lane == PieceLane.None ? -1 : (int)lane - 1;
        }
    }
}

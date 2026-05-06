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
    }
}

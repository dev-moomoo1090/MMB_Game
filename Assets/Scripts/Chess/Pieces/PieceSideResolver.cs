namespace MMBGame
{
    public static class PieceSideResolver
    {
        public static PieceSide Resolve(PieceType type, int file)
        {
            if (type == PieceType.King || type == PieceType.Queen || type == PieceType.Barricade || type == PieceType.Trebuchet)
            {
                return PieceSide.None;
            }

            return file <= 3 ? PieceSide.Queenside : PieceSide.Kingside;
        }
    }
}

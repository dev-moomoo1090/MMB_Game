namespace MMBGame
{
    public static class PieceClassifier
    {
        public static bool IsObstacle(PieceType type)
        {
            return type == PieceType.Barricade || type == PieceType.Trebuchet;
        }

        public static bool IsObstacle(ChessPiece piece)
        {
            return piece != null && IsObstacle(piece.type);
        }

        public static bool IsPoliticalPiece(ChessPiece piece)
        {
            return piece != null && !IsObstacle(piece.type);
        }
    }
}

namespace MMBGame
{
    public class Move
    {
        public int fromFile;
        public int fromRank;
        public int toFile;
        public int toRank;
        public SpecialMoveType specialMove;
        public PieceType promotionPiece;

        public Move(int fromFile, int fromRank, int toFile, int toRank,
            SpecialMoveType specialMove = SpecialMoveType.None,
            PieceType promotionPiece = PieceType.Queen)
        {
            this.fromFile = fromFile;
            this.fromRank = fromRank;
            this.toFile = toFile;
            this.toRank = toRank;
            this.specialMove = specialMove;
            this.promotionPiece = promotionPiece;
        }

        public override bool Equals(object obj)
        {
            if (obj is Move other)
                return fromFile == other.fromFile && fromRank == other.fromRank
                    && toFile == other.toFile && toRank == other.toRank
                    && specialMove == other.specialMove;
            return false;
        }

        public override int GetHashCode()
        {
            return (fromFile << 12) | (fromRank << 9) | (toFile << 6) | (toRank << 3) | (int)specialMove;
        }
    }
}

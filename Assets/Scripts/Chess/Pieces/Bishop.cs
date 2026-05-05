namespace MMBGame
{
    public class Bishop : ChessPiece
    {
        public Bishop(PieceColor color, int file, int rank) : base(color, PieceType.Bishop, file, rank)
        {
            pieceName = color == PieceColor.White ? "White Bishop" : "Black Bishop";
        }

        protected override void SetupMovePatterns()
        {
            originalMovePatterns.Add(new MovePattern(1, 1, true));
            originalMovePatterns.Add(new MovePattern(1, -1, true));
            originalMovePatterns.Add(new MovePattern(-1, 1, true));
            originalMovePatterns.Add(new MovePattern(-1, -1, true));
        }

        public override ChessPiece Clone()
        {
            var b = new Bishop(color, file, rank);
            CopyStateTo(b);
            return b;
        }
    }
}

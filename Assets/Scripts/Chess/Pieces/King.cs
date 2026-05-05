namespace MMBGame
{
    public class King : ChessPiece
    {
        public King(PieceColor color, int file, int rank) : base(color, PieceType.King, file, rank)
        {
            pieceName = color == PieceColor.White ? "White King" : "Black King";
        }

        protected override void SetupMovePatterns()
        {
            originalMovePatterns.Add(new MovePattern(1, 0));
            originalMovePatterns.Add(new MovePattern(-1, 0));
            originalMovePatterns.Add(new MovePattern(0, 1));
            originalMovePatterns.Add(new MovePattern(0, -1));
            originalMovePatterns.Add(new MovePattern(1, 1));
            originalMovePatterns.Add(new MovePattern(1, -1));
            originalMovePatterns.Add(new MovePattern(-1, 1));
            originalMovePatterns.Add(new MovePattern(-1, -1));
        }

        public override ChessPiece Clone()
        {
            var k = new King(color, file, rank);
            CopyStateTo(k);
            return k;
        }
    }
}

namespace MMBGame
{
    public class Knight : ChessPiece
    {
        public Knight(PieceColor color, int file, int rank) : base(color, PieceType.Knight, file, rank)
        {
            pieceName = color == PieceColor.White ? "White Knight" : "Black Knight";
        }

        protected override void SetupMovePatterns()
        {
            originalMovePatterns.Add(new MovePattern(1, 2));
            originalMovePatterns.Add(new MovePattern(2, 1));
            originalMovePatterns.Add(new MovePattern(2, -1));
            originalMovePatterns.Add(new MovePattern(1, -2));
            originalMovePatterns.Add(new MovePattern(-1, -2));
            originalMovePatterns.Add(new MovePattern(-2, -1));
            originalMovePatterns.Add(new MovePattern(-2, 1));
            originalMovePatterns.Add(new MovePattern(-1, 2));
        }

        public override ChessPiece Clone()
        {
            var n = new Knight(color, file, rank);
            CopyStateTo(n);
            return n;
        }
    }
}

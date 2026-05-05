namespace MMBGame
{
    public class Queen : ChessPiece
    {
        public Queen(PieceColor color, int file, int rank) : base(color, PieceType.Queen, file, rank)
        {
            pieceName = color == PieceColor.White ? "White Queen" : "Black Queen";
        }

        protected override void SetupMovePatterns()
        {
            originalMovePatterns.Add(new MovePattern(1, 0, true));
            originalMovePatterns.Add(new MovePattern(-1, 0, true));
            originalMovePatterns.Add(new MovePattern(0, 1, true));
            originalMovePatterns.Add(new MovePattern(0, -1, true));
            originalMovePatterns.Add(new MovePattern(1, 1, true));
            originalMovePatterns.Add(new MovePattern(1, -1, true));
            originalMovePatterns.Add(new MovePattern(-1, 1, true));
            originalMovePatterns.Add(new MovePattern(-1, -1, true));
        }

        public override ChessPiece Clone()
        {
            var q = new Queen(color, file, rank);
            CopyStateTo(q);
            return q;
        }
    }
}

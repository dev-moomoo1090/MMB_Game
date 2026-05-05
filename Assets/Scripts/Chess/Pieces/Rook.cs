namespace MMBGame
{
    public class Rook : ChessPiece
    {
        public Rook(PieceColor color, int file, int rank) : base(color, PieceType.Rook, file, rank)
        {
            pieceName = color == PieceColor.White ? "White Rook" : "Black Rook";
        }

        protected override void SetupMovePatterns()
        {
            originalMovePatterns.Add(new MovePattern(1, 0, true));
            originalMovePatterns.Add(new MovePattern(-1, 0, true));
            originalMovePatterns.Add(new MovePattern(0, 1, true));
            originalMovePatterns.Add(new MovePattern(0, -1, true));
        }

        public override ChessPiece Clone()
        {
            var r = new Rook(color, file, rank);
            CopyStateTo(r);
            return r;
        }
    }
}

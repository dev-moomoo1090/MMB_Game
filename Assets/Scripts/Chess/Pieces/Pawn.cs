namespace MMBGame
{
    public class Pawn : ChessPiece
    {
        public Pawn(PieceColor color, int file, int rank) : base(color, PieceType.Pawn, file, rank)
        {
            pieceName = color == PieceColor.White ? "White Pawn" : "Black Pawn";
        }

        protected override void SetupMovePatterns()
        {
            int dir = color == PieceColor.White ? 1 : -1;
            originalMovePatterns.Add(new MovePattern(0, dir, false, false, true));
            originalMovePatterns.Add(new MovePattern(1, dir, false, true, false));
            originalMovePatterns.Add(new MovePattern(-1, dir, false, true, false));
        }

        public override ChessPiece Clone()
        {
            var p = new Pawn(color, file, rank);
            CopyStateTo(p);
            return p;
        }
    }
}

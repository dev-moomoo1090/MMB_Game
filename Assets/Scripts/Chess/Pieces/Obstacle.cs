namespace MMBGame
{
    public class Barricade : ChessPiece
    {
        public Barricade(PieceColor color, int file, int rank)
            : base(color, PieceType.Barricade, file, rank)
        {
            pieceName = "Barricade";
            support = 0;
            taxPerTurn = 0;
        }

        protected override void SetupMovePatterns()
        {
        }

        public override ChessPiece Clone()
        {
            var barricade = new Barricade(color, file, rank);
            CopyStateTo(barricade);
            return barricade;
        }
    }

    public class Trebuchet : ChessPiece
    {
        public Trebuchet(PieceColor color, int file, int rank)
            : base(color, PieceType.Trebuchet, file, rank)
        {
            pieceName = "Trebuchet";
            support = 0;
            taxPerTurn = 0;
        }

        protected override void SetupMovePatterns()
        {
        }

        public override ChessPiece Clone()
        {
            var trebuchet = new Trebuchet(color, file, rank);
            CopyStateTo(trebuchet);
            return trebuchet;
        }
    }
}

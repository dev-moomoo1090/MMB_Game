namespace MMBGame
{
    public class Square
    {
        public int file;
        public int rank;
        public ChessPiece piece;

        public Square(int file, int rank)
        {
            this.file = file;
            this.rank = rank;
        }

        public bool IsEmpty => piece == null;
    }
}

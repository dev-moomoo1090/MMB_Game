using System.Text;

namespace MMBGame
{
    public static class FenConverter
    {
        public static string ToFen(BoardState state)
        {
            var sb = new StringBuilder();

            for (int rank = 7; rank >= 0; rank--)
            {
                int empty = 0;
                for (int file = 0; file < 8; file++)
                {
                    ChessPiece piece = state.GetPiece(file, rank);
                    if (piece == null)
                    {
                        empty++;
                    }
                    else
                    {
                        if (empty > 0)
                        {
                            sb.Append(empty);
                            empty = 0;
                        }
                        sb.Append(GetFenChar(piece));
                    }
                }
                if (empty > 0) sb.Append(empty);
                if (rank > 0) sb.Append('/');
            }

            sb.Append(state.currentTurn == PieceColor.White ? " w " : " b ");

            string castling = BuildCastlingString(state);
            sb.Append(castling.Length > 0 ? castling : "-");

            if (state.enPassantAvailable)
                sb.Append(' ').Append((char)('a' + state.enPassantFile)).Append(state.enPassantRank + 1);
            else
                sb.Append(" -");

            sb.Append(" 0 1");

            return sb.ToString();
        }

        private static string BuildCastlingString(BoardState state)
        {
            var sb = new StringBuilder();
            if (state.whiteKingsideCastle) sb.Append('K');
            if (state.whiteQueensideCastle) sb.Append('Q');
            if (state.blackKingsideCastle) sb.Append('k');
            if (state.blackQueensideCastle) sb.Append('q');
            return sb.ToString();
        }

        private static char GetFenChar(ChessPiece piece)
        {
            char c;
            switch (piece.type)
            {
                case PieceType.Pawn:   c = 'p'; break;
                case PieceType.Rook:   c = 'r'; break;
                case PieceType.Knight: c = 'n'; break;
                case PieceType.Bishop: c = 'b'; break;
                case PieceType.Queen:  c = 'q'; break;
                case PieceType.King:   c = 'k'; break;
                default:               c = '?'; break;
            }
            return piece.color == PieceColor.White ? char.ToUpper(c) : c;
        }
    }
}

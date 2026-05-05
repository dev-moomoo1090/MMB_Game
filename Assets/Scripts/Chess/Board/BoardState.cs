using System.Collections.Generic;
using System.Text;

namespace MMBGame
{
    public class BoardState
    {
        public Square[,] squares;
        public PieceColor currentTurn;
        public bool enPassantAvailable;
        public int enPassantFile;
        public int enPassantRank;
        public bool whiteKingsideCastle;
        public bool whiteQueensideCastle;
        public bool blackKingsideCastle;
        public bool blackQueensideCastle;
        public List<Move> moveHistory;
        public Dictionary<string, int> positionHistory;
        public List<ChessPiece> offBoardPieces;

        public BoardState()
        {
            squares = new Square[8, 8];
            for (int f = 0; f < 8; f++)
                for (int r = 0; r < 8; r++)
                    squares[f, r] = new Square(f, r);
            currentTurn = PieceColor.White;
            whiteKingsideCastle = true;
            whiteQueensideCastle = true;
            blackKingsideCastle = true;
            blackQueensideCastle = true;
            enPassantAvailable = false;
            moveHistory = new List<Move>();
            positionHistory = new Dictionary<string, int>();
            offBoardPieces = new List<ChessPiece>();
        }

        public ChessPiece GetPiece(int file, int rank)
        {
            if (!IsInBounds(file, rank)) return null;
            return squares[file, rank].piece;
        }

        public bool IsInBounds(int file, int rank) => file >= 0 && file <= 7 && rank >= 0 && rank <= 7;

        public string GetPositionKey()
        {
            var sb = new StringBuilder();
            for (int r = 7; r >= 0; r--)
                for (int f = 0; f < 8; f++)
                {
                    var p = squares[f, r].piece;
                    sb.Append(p == null ? '.' : GetPieceChar(p));
                }
            sb.Append(currentTurn == PieceColor.White ? 'w' : 'b');
            sb.Append(whiteKingsideCastle ? 'K' : '-');
            sb.Append(whiteQueensideCastle ? 'Q' : '-');
            sb.Append(blackKingsideCastle ? 'k' : '-');
            sb.Append(blackQueensideCastle ? 'q' : '-');
            if (enPassantAvailable) sb.Append((char)('a' + enPassantFile)).Append(enPassantRank + 1);
            return sb.ToString();
        }

        private char GetPieceChar(ChessPiece p)
        {
            char c;
            switch (p.type)
            {
                case PieceType.Pawn:   c = 'p'; break;
                case PieceType.Rook:   c = 'r'; break;
                case PieceType.Knight: c = 'n'; break;
                case PieceType.Bishop: c = 'b'; break;
                case PieceType.Queen:  c = 'q'; break;
                case PieceType.King:   c = 'k'; break;
                default:               c = '?'; break;
            }
            return p.color == PieceColor.White ? char.ToUpper(c) : c;
        }

        public void RecordPosition()
        {
            string key = GetPositionKey();
            positionHistory.TryGetValue(key, out int count);
            positionHistory[key] = count + 1;
        }

        public bool IsThreefoldRepetition()
        {
            string key = GetPositionKey();
            return positionHistory.TryGetValue(key, out int count) && count >= 3;
        }

        public List<ChessPiece> GetAllPieces()
        {
            List<ChessPiece> pieces = new List<ChessPiece>();
            for (int file = 0; file < 8; file++)
            {
                for (int rank = 0; rank < 8; rank++)
                {
                    ChessPiece piece = GetPiece(file, rank);
                    if (piece != null)
                    {
                        pieces.Add(piece);
                    }
                }
            }

            pieces.AddRange(offBoardPieces);
            return pieces;
        }

        public BoardState Clone()
        {
            var clone = new BoardState();
            for (int f = 0; f < 8; f++)
                for (int r = 0; r < 8; r++)
                {
                    var p = squares[f, r].piece;
                    clone.squares[f, r].piece = p != null ? p.Clone() : null;
                }
            clone.currentTurn = currentTurn;
            clone.enPassantAvailable = enPassantAvailable;
            clone.enPassantFile = enPassantFile;
            clone.enPassantRank = enPassantRank;
            clone.whiteKingsideCastle = whiteKingsideCastle;
            clone.whiteQueensideCastle = whiteQueensideCastle;
            clone.blackKingsideCastle = blackKingsideCastle;
            clone.blackQueensideCastle = blackQueensideCastle;
            clone.moveHistory = new List<Move>(moveHistory);
            clone.positionHistory = new Dictionary<string, int>(positionHistory);
            clone.offBoardPieces = new List<ChessPiece>();
            for (int i = 0; i < offBoardPieces.Count; i++)
            {
                clone.offBoardPieces.Add(offBoardPieces[i].Clone());
            }
            return clone;
        }
    }
}

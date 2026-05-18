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
        public List<ChessPiece> capturedThisTurn;
        public bool roadActive;
        public int roadAFile;
        public int roadARank;
        public int roadBFile;
        public int roadBRank;
        public int globalAcceptanceWeight;
        public int globalDefectionWeight;
        public int globalRebellionWeight;

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
            capturedThisTurn = new List<ChessPiece>();
            globalAcceptanceWeight = 0;
            globalDefectionWeight = 0;
            globalRebellionWeight = 0;
            ClearRoad();
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

        public bool HasKing(PieceColor color)
        {
            for (int file = 0; file < 8; file++)
            {
                for (int rank = 0; rank < 8; rank++)
                {
                    ChessPiece piece = GetPiece(file, rank);
                    if (piece != null && piece.color == color && piece.type == PieceType.King)
                    {
                        return true;
                    }
                }
            }

            for (int i = 0; i < offBoardPieces.Count; i++)
            {
                ChessPiece piece = offBoardPieces[i];
                if (piece != null && piece.color == color && piece.type == PieceType.King)
                {
                    return true;
                }
            }

            return false;
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

        public void SetRoad(int firstFile, int firstRank, int secondFile, int secondRank)
        {
            roadActive = true;
            roadAFile = firstFile;
            roadARank = firstRank;
            roadBFile = secondFile;
            roadBRank = secondRank;
        }

        public void ClearRoad()
        {
            roadActive = false;
            roadAFile = -1;
            roadARank = -1;
            roadBFile = -1;
            roadBRank = -1;
        }

        public bool IsRoadEndpoint(int file, int rank)
        {
            return roadActive &&
                ((roadAFile == file && roadARank == rank) || (roadBFile == file && roadBRank == rank));
        }

        public bool TryGetRoadDestination(int file, int rank, out int targetFile, out int targetRank)
        {
            targetFile = -1;
            targetRank = -1;
            if (!roadActive)
            {
                return false;
            }

            if (roadAFile == file && roadARank == rank)
            {
                targetFile = roadBFile;
                targetRank = roadBRank;
                return true;
            }

            if (roadBFile == file && roadBRank == rank)
            {
                targetFile = roadAFile;
                targetRank = roadARank;
                return true;
            }

            return false;
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
            clone.globalAcceptanceWeight = globalAcceptanceWeight;
            clone.globalDefectionWeight = globalDefectionWeight;
            clone.globalRebellionWeight = globalRebellionWeight;
            if (roadActive)
            {
                clone.SetRoad(roadAFile, roadARank, roadBFile, roadBRank);
            }
            else
            {
                clone.ClearRoad();
            }

            for (int i = 0; i < offBoardPieces.Count; i++)
            {
                clone.offBoardPieces.Add(offBoardPieces[i].Clone());
            }
            return clone;
        }
    }
}

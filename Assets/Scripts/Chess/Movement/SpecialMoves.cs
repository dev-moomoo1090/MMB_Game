namespace MMBGame
{
    public static class SpecialMoves
    {
        public static void ApplyMove(BoardState state, Move move)
        {
            var piece = state.GetPiece(move.fromFile, move.fromRank);
            if (piece == null) return;
            state.enPassantAvailable = false;
            UpdateCastlingRights(state, piece, move);
            state.squares[move.fromFile, move.fromRank].piece = null;
            switch (move.specialMove)
            {
                case SpecialMoveType.PawnDoubleAdvance: ApplyPawnDoubleAdvance(state, piece, move); break;
                case SpecialMoveType.EnPassant:         ApplyEnPassant(state, piece, move); break;
                case SpecialMoveType.CastleKingside:    ApplyCastleKingside(state, piece); break;
                case SpecialMoveType.CastleQueenside:   ApplyCastleQueenside(state, piece); break;
                case SpecialMoveType.Promotion:         ApplyPromotion(state, piece, move); break;
                default:                                PlacePiece(state, piece, move.toFile, move.toRank); break;
            }
            piece.hasMoved = true;
            ClearRoadIfUsed(state, move);
            state.currentTurn = state.currentTurn == PieceColor.White ? PieceColor.Black : PieceColor.White;
            state.moveHistory.Add(move);
        }

        private static void ClearRoadIfUsed(BoardState state, Move move)
        {
            if (!state.IsRoadEndpoint(move.fromFile, move.fromRank) || !state.IsRoadEndpoint(move.toFile, move.toRank))
            {
                return;
            }

            state.ClearRoad();
        }

        private static void PlacePiece(BoardState state, ChessPiece piece, int file, int rank)
        {
            piece.file = file; piece.rank = rank;
            state.squares[file, rank].piece = piece;
        }

        private static void ApplyPawnDoubleAdvance(BoardState state, ChessPiece pawn, Move move)
        {
            PlacePiece(state, pawn, move.toFile, move.toRank);
            state.enPassantAvailable = true;
            state.enPassantFile = (move.fromFile + move.toFile) / 2;
            state.enPassantRank = move.toRank;
        }

        private static void ApplyEnPassant(BoardState state, ChessPiece pawn, Move move)
        {
            state.squares[move.fromFile, move.toRank].piece = null;
            PlacePiece(state, pawn, move.toFile, move.toRank);
        }

        private static void ApplyCastleKingside(BoardState state, ChessPiece king)
        {
            int file = king.file;
            var rook = state.GetPiece(file, 7);
            state.squares[file, 7].piece = null;
            PlacePiece(state, king, file, 6);
            if (rook != null) { rook.hasMoved = true; PlacePiece(state, rook, file, 5); }
        }

        private static void ApplyCastleQueenside(BoardState state, ChessPiece king)
        {
            int file = king.file;
            var rook = state.GetPiece(file, 0);
            state.squares[file, 0].piece = null;
            PlacePiece(state, king, file, 2);
            if (rook != null) { rook.hasMoved = true; PlacePiece(state, rook, file, 3); }
        }

        private static void ApplyPromotion(BoardState state, ChessPiece pawn, Move move)
        {
            ChessPiece promoted;
            switch (move.promotionPiece)
            {
                case PieceType.Rook:   promoted = new Rook(pawn.color, move.toFile, move.toRank); break;
                case PieceType.Bishop: promoted = new Bishop(pawn.color, move.toFile, move.toRank); break;
                case PieceType.Knight: promoted = new Knight(pawn.color, move.toFile, move.toRank); break;
                default:               promoted = new Queen(pawn.color, move.toFile, move.toRank); break;
            }
            promoted.hasMoved = true;
            promoted.movementControllerColor = pawn.GetMovementControllerColor();
            state.squares[move.toFile, move.toRank].piece = promoted;
        }

        private static void UpdateCastlingRights(BoardState state, ChessPiece piece, Move move)
        {
            if (piece.type == PieceType.King)
            {
                if (piece.color == PieceColor.White) { state.whiteKingsideCastle = false; state.whiteQueensideCastle = false; }
                else { state.blackKingsideCastle = false; state.blackQueensideCastle = false; }
            }
            else if (piece.type == PieceType.Rook)
            {
                if (piece.color == PieceColor.White)
                {
                    if (piece.file == 0 && piece.rank == 7) state.whiteKingsideCastle = false;
                    if (piece.file == 0 && piece.rank == 0) state.whiteQueensideCastle = false;
                }
                else
                {
                    if (piece.file == 7 && piece.rank == 7) state.blackKingsideCastle = false;
                    if (piece.file == 7 && piece.rank == 0) state.blackQueensideCastle = false;
                }
            }
            if (move.toFile == 0 && move.toRank == 0) state.whiteQueensideCastle = false;
            if (move.toFile == 0 && move.toRank == 7) state.whiteKingsideCastle = false;
            if (move.toFile == 7 && move.toRank == 0) state.blackQueensideCastle = false;
            if (move.toFile == 7 && move.toRank == 7) state.blackKingsideCastle = false;
        }
    }
}

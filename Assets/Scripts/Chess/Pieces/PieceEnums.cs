namespace MMBGame
{
    public enum PieceColor { None, White, Black }
    public enum PieceSide { None, Queenside, Kingside }
    public enum PieceLane { None, A, B, C, D, E, F, G, H }
    public enum PieceType { Pawn, Rook, Knight, Bishop, Queen, King, Barricade, Trebuchet }
    public enum SpecialMoveType { None, PawnDoubleAdvance, EnPassant, CastleKingside, CastleQueenside, Promotion }
    public enum GameResult { InProgress, Check, Checkmate, Stalemate, Draw }
    public enum KingState
    {
        Neutral,
        Sage,
        Benevolent = Sage,
        DarkKing,
        Incompetent = DarkKing,
        Autocrat,
        Dictatorship = Autocrat,
        Tyrant
    }
}

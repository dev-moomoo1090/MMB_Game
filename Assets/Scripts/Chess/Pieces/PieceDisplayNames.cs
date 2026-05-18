namespace MMBGame
{
    public static class PieceDisplayNames
    {
        public static string GetShortName(ChessPiece piece)
        {
            if (piece == null)
            {
                return "기물";
            }

            return GetColorName(piece.color) + GetSideCode(piece.side) + GetTypeName(piece.type);
        }

        public static string GetSimpleName(ChessPiece piece)
        {
            if (piece == null)
            {
                return "기물";
            }

            return GetColorName(piece.color) + " " + GetTypeName(piece.type);
        }

        public static string GetDisplayName(ChessPiece piece)
        {
            if (piece == null)
            {
                return "기물";
            }

            string laneName = GetLaneName(piece.lane);
            if (!string.IsNullOrEmpty(laneName))
            {
                return GetColorName(piece.color) + " " + laneName + " " + GetTypeName(piece.type);
            }

            string sideName = GetSideName(piece.side);
            if (string.IsNullOrEmpty(sideName))
            {
                return GetColorName(piece.color) + " " + GetTypeName(piece.type);
            }

            return GetColorName(piece.color) + " " + sideName + " " + GetTypeName(piece.type);
        }

        public static string GetColorName(PieceColor color)
        {
            if (color == PieceColor.White)
            {
                return "백";
            }

            if (color == PieceColor.Black)
            {
                return "흑";
            }

            return "무색";
        }

        public static string GetSideCode(PieceSide side)
        {
            if (side == PieceSide.Kingside)
            {
                return "K";
            }

            if (side == PieceSide.Queenside)
            {
                return "Q";
            }

            return string.Empty;
        }

        public static string GetSideName(PieceSide side)
        {
            if (side == PieceSide.Kingside)
            {
                return "킹사이드";
            }

            if (side == PieceSide.Queenside)
            {
                return "퀸사이드";
            }

            return string.Empty;
        }

        public static string GetLaneName(PieceLane lane)
        {
            return lane == PieceLane.None ? string.Empty : lane + "열";
        }

        public static string GetTypeName(PieceType type)
        {
            switch (type)
            {
                case PieceType.Pawn: return "폰";
                case PieceType.Rook: return "룩";
                case PieceType.Knight: return "나이트";
                case PieceType.Bishop: return "비숍";
                case PieceType.Queen: return "퀸";
                case PieceType.King: return "킹";
                case PieceType.Barricade: return "바리케이드";
                case PieceType.Trebuchet: return "트레뷰셋";
                default: return type.ToString();
            }
        }
    }
}

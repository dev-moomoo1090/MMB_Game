using System.Collections.Generic;

namespace MMBGame
{
    public static class PieceSetupDefaults
    {
        public static void Fill(List<PieceSetupDefinition> definitions)
        {
            if (definitions == null)
            {
                return;
            }

            definitions.Clear();
            AddSidePair(definitions, PieceColor.White, PieceType.Pawn);
            AddSidePair(definitions, PieceColor.White, PieceType.Rook);
            AddSidePair(definitions, PieceColor.White, PieceType.Knight);
            AddSidePair(definitions, PieceColor.White, PieceType.Bishop);
            Add(definitions, PieceColor.White, PieceSide.None, PieceType.Queen);
            Add(definitions, PieceColor.White, PieceSide.None, PieceType.King);
            AddSidePair(definitions, PieceColor.Black, PieceType.Pawn);
            AddSidePair(definitions, PieceColor.Black, PieceType.Rook);
            AddSidePair(definitions, PieceColor.Black, PieceType.Knight);
            AddSidePair(definitions, PieceColor.Black, PieceType.Bishop);
            Add(definitions, PieceColor.Black, PieceSide.None, PieceType.Queen);
            Add(definitions, PieceColor.Black, PieceSide.None, PieceType.King);
        }

        private static void AddSidePair(List<PieceSetupDefinition> definitions, PieceColor color, PieceType type)
        {
            Add(definitions, color, PieceSide.Queenside, type);
            Add(definitions, color, PieceSide.Kingside, type);
        }

        private static void Add(List<PieceSetupDefinition> definitions, PieceColor color, PieceSide side, PieceType type)
        {
            definitions.Add(new PieceSetupDefinition
            {
                color = color,
                side = side,
                type = type,
                initialTaxPerTurn = 1,
                initialSupport = 50,
                baseMovePatterns = CreatePatterns(color, type)
            });
        }

        private static List<MovePattern> CreatePatterns(PieceColor color, PieceType type)
        {
            List<MovePattern> patterns = new List<MovePattern>();
            int direction = color == PieceColor.White ? 1 : -1;
            if (type == PieceType.Pawn)
            {
                patterns.Add(new MovePattern(0, direction, false, false, true));
                patterns.Add(new MovePattern(1, direction, false, true));
                patterns.Add(new MovePattern(-1, direction, false, true));
            }
            else if (type == PieceType.Rook)
            {
                patterns.Add(new MovePattern(1, 0, true));
                patterns.Add(new MovePattern(-1, 0, true));
                patterns.Add(new MovePattern(0, 1, true));
                patterns.Add(new MovePattern(0, -1, true));
            }
            else if (type == PieceType.Knight)
            {
                patterns.Add(new MovePattern(1, 2));
                patterns.Add(new MovePattern(2, 1));
                patterns.Add(new MovePattern(2, -1));
                patterns.Add(new MovePattern(1, -2));
                patterns.Add(new MovePattern(-1, -2));
                patterns.Add(new MovePattern(-2, -1));
                patterns.Add(new MovePattern(-2, 1));
                patterns.Add(new MovePattern(-1, 2));
            }
            else if (type == PieceType.Bishop)
            {
                patterns.Add(new MovePattern(1, 1, true));
                patterns.Add(new MovePattern(1, -1, true));
                patterns.Add(new MovePattern(-1, 1, true));
                patterns.Add(new MovePattern(-1, -1, true));
            }
            else if (type == PieceType.Queen)
            {
                patterns.AddRange(CreatePatterns(color, PieceType.Rook));
                patterns.AddRange(CreatePatterns(color, PieceType.Bishop));
            }
            else if (type == PieceType.King)
            {
                patterns.Add(new MovePattern(1, 0));
                patterns.Add(new MovePattern(-1, 0));
                patterns.Add(new MovePattern(0, 1));
                patterns.Add(new MovePattern(0, -1));
                patterns.Add(new MovePattern(1, 1));
                patterns.Add(new MovePattern(1, -1));
                patterns.Add(new MovePattern(-1, 1));
                patterns.Add(new MovePattern(-1, -1));
            }

            return patterns;
        }
    }
}

using System.Collections.Generic;

namespace MMBGame
{
    public static class PieceSetupDefaults
    {
        private static readonly int[] PAWN_SUPPORTS = { 45, 40, 35, 30, 70, 65, 60, 55 };
        private static readonly int[] MAJOR_SUPPORTS = { 30, 15, 45, 20, 100, 80, 65, 70 };

        public static void Fill(List<PieceSetupDefinition> definitions)
        {
            if (definitions == null)
            {
                return;
            }

            definitions.Clear();
            AddColor(definitions, PieceColor.White);
            AddColor(definitions, PieceColor.Black);
        }

        public static bool UpgradeIfNeeded(List<PieceSetupDefinition> definitions)
        {
            if (definitions == null || !NeedsUpgrade(definitions))
            {
                return false;
            }

            List<PieceSetupDefinition> existingDefinitions = new List<PieceSetupDefinition>(definitions);
            Fill(definitions);

            for (int i = 0; i < definitions.Count; i++)
            {
                CopyVisualReferences(definitions[i], FindBestMatch(existingDefinitions, definitions[i]));
            }

            return true;
        }

        private static void AddColor(List<PieceSetupDefinition> definitions, PieceColor color)
        {
            AddMajor(definitions, color, PieceLane.A, PieceType.Rook);
            AddMajor(definitions, color, PieceLane.B, PieceType.Knight);
            AddMajor(definitions, color, PieceLane.C, PieceType.Bishop);
            AddMajor(definitions, color, PieceLane.D, PieceType.Queen);
            AddMajor(definitions, color, PieceLane.E, PieceType.King);
            AddMajor(definitions, color, PieceLane.F, PieceType.Bishop);
            AddMajor(definitions, color, PieceLane.G, PieceType.Knight);
            AddMajor(definitions, color, PieceLane.H, PieceType.Rook);

            for (int i = 0; i < 8; i++)
            {
                Add(definitions, color, ResolveSide((PieceLane)(i + 1)), (PieceLane)(i + 1), PieceType.Pawn);
            }
        }

        private static void AddMajor(List<PieceSetupDefinition> definitions, PieceColor color, PieceLane lane, PieceType type)
        {
            Add(definitions, color, ResolveSide(type, lane), lane, type);
        }

        private static void Add(List<PieceSetupDefinition> definitions, PieceColor color, PieceSide side, PieceLane lane, PieceType type)
        {
            definitions.Add(new PieceSetupDefinition
            {
                color = color,
                side = side,
                lane = lane,
                type = type,
                initialTaxPerTurn = GetInitialTax(type),
                initialSupport = GetInitialSupport(type, lane),
                baseMovePatterns = CreatePatterns(color, type)
            });
        }

        private static PieceSide ResolveSide(PieceLane lane)
        {
            int laneIndex = PieceSideResolver.GetLaneIndex(lane);
            return laneIndex >= 0 && laneIndex <= 3 ? PieceSide.Queenside : PieceSide.Kingside;
        }

        private static PieceSide ResolveSide(PieceType type, PieceLane lane)
        {
            if (type == PieceType.King || type == PieceType.Queen)
            {
                return PieceSide.None;
            }

            return ResolveSide(lane);
        }

        private static int GetInitialSupport(PieceType type, PieceLane lane)
        {
            int laneIndex = PieceSideResolver.GetLaneIndex(lane);
            if (laneIndex < 0 || laneIndex >= PAWN_SUPPORTS.Length)
            {
                return 50;
            }

            return type == PieceType.Pawn ? PAWN_SUPPORTS[laneIndex] : MAJOR_SUPPORTS[laneIndex];
        }

        private static int GetInitialTax(PieceType type)
        {
            switch (type)
            {
                case PieceType.Pawn: return 20;
                case PieceType.Knight: return 30;
                case PieceType.Bishop: return 30;
                case PieceType.Rook: return 50;
                case PieceType.Queen: return 90;
                default: return 0;
            }
        }

        private static bool NeedsUpgrade(List<PieceSetupDefinition> definitions)
        {
            return !HasDefinition(definitions, PieceColor.White, PieceType.Pawn, PieceLane.A)
                || !HasDefinition(definitions, PieceColor.White, PieceType.Pawn, PieceLane.H)
                || !HasDefinition(definitions, PieceColor.Black, PieceType.Pawn, PieceLane.A)
                || !HasDefinition(definitions, PieceColor.Black, PieceType.Pawn, PieceLane.H)
                || !HasDefinition(definitions, PieceColor.White, PieceType.King, PieceLane.E)
                || !HasDefinition(definitions, PieceColor.Black, PieceType.King, PieceLane.E);
        }

        private static bool HasDefinition(List<PieceSetupDefinition> definitions, PieceColor color, PieceType type, PieceLane lane)
        {
            for (int i = 0; i < definitions.Count; i++)
            {
                PieceSetupDefinition definition = definitions[i];
                if (definition != null && definition.color == color && definition.type == type && definition.lane == lane)
                {
                    return true;
                }
            }

            return false;
        }

        private static PieceSetupDefinition FindBestMatch(List<PieceSetupDefinition> definitions, PieceSetupDefinition target)
        {
            PieceSetupDefinition sideMatch = null;
            PieceSetupDefinition colorTypeMatch = null;
            for (int i = 0; i < definitions.Count; i++)
            {
                PieceSetupDefinition definition = definitions[i];
                if (definition == null || definition.color != target.color || definition.type != target.type)
                {
                    continue;
                }

                if (definition.lane == target.lane)
                {
                    return definition;
                }

                if (definition.side == target.side && sideMatch == null)
                {
                    sideMatch = definition;
                }

                if (colorTypeMatch == null)
                {
                    colorTypeMatch = definition;
                }
            }

            return sideMatch ?? colorTypeMatch;
        }

        private static void CopyVisualReferences(PieceSetupDefinition target, PieceSetupDefinition source)
        {
            if (target == null || source == null)
            {
                return;
            }

            target.prefab = source.prefab;
            target.selectedPrefab = source.selectedPrefab;
            target.sprite = source.sprite;
            target.selectedSprite = source.selectedSprite;
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

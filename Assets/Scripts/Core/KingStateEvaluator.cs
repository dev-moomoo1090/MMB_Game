using System.Collections.Generic;

namespace MMBGame
{
    public static class KingStateEvaluator
    {
        private static readonly Dictionary<PieceColor, KingState> currentStates = new Dictionary<PieceColor, KingState>();
        private static readonly HashSet<PieceColor> fixedBenevolentColors = new HashSet<PieceColor>();

        public static KingState Evaluate(PlayerState player, BoardState board)
        {
            if (player == null || board == null)
            {
                return KingState.Neutral;
            }

            if (fixedBenevolentColors.Contains(player.color))
            {
                return KingState.Sage;
            }

            int totalSupport = ComputeTotalSupport(player.color, board);
            bool highSupport = totalSupport >= 4;
            bool lowSupport = totalSupport <= -4;
            bool highHonor = player.honor >= 20;
            bool lowHonor = player.honor <= -20;

            if (highSupport && highHonor) return KingState.Sage;
            if (highSupport && lowHonor) return KingState.Autocrat;
            if (lowSupport && highHonor) return KingState.DarkKing;
            if (lowSupport && lowHonor) return KingState.Tyrant;
            return KingState.Neutral;
        }

        public static int ComputeTotalSupport(PieceColor color, BoardState board)
        {
            if (board == null)
            {
                return 0;
            }

            int total = 0;
            List<ChessPiece> pieces = board.GetAllPieces();
            for (int i = 0; i < pieces.Count; i++)
            {
                ChessPiece piece = pieces[i];
                if (piece == null || piece.color != color)
                {
                    continue;
                }

                if (piece.support > 50)
                {
                    total += 1;
                }
                else if (piece.support < 50)
                {
                    total -= 1;
                }
            }

            return total;
        }

        public static void ResetRuntimeState()
        {
            currentStates.Clear();
            fixedBenevolentColors.Clear();
        }

        public static void SetCurrentState(PieceColor color, KingState state)
        {
            if (color != PieceColor.None)
            {
                currentStates[color] = state;
            }
        }

        public static KingState GetCurrentState(PieceColor color)
        {
            if (fixedBenevolentColors.Contains(color))
            {
                return KingState.Sage;
            }

            return currentStates.TryGetValue(color, out KingState state) ? state : KingState.Neutral;
        }

        public static void FixBenevolent(PieceColor color)
        {
            if (color == PieceColor.None)
            {
                return;
            }

            fixedBenevolentColors.Add(color);
            currentStates[color] = KingState.Sage;
        }

        public static bool IsFixedBenevolent(PieceColor color)
        {
            return fixedBenevolentColors.Contains(color);
        }

        public static bool IsBenevolent(PieceColor color)
        {
            return GetCurrentState(color) == KingState.Sage;
        }

        public static bool IsDictatorship(PieceColor color)
        {
            return GetCurrentState(color) == KingState.Autocrat;
        }

        public static bool IsTyrant(PieceColor color)
        {
            return GetCurrentState(color) == KingState.Tyrant;
        }

        public static bool SuppressesRefusal(PieceColor color)
        {
            return IsDictatorship(color);
        }

        public static float GetMilitaryCostMultiplier(PieceColor color)
        {
            return IsDictatorship(color) ? 0.5f : 1f;
        }

        public static float GetTaxIncomeMultiplier(PieceColor color)
        {
            if (IsFixedBenevolent(color))
            {
                return 3f;
            }

            if (IsDictatorship(color))
            {
                return 1.5f;
            }

            if (IsTyrant(color))
            {
                return 5f;
            }

            return 1f;
        }

        public static float GetIncomingPoliticalPenaltyMultiplier(PieceColor color)
        {
            return IsBenevolent(color) ? 0.5f : 1f;
        }

        public static int GetAcceptanceWeightModifier(PieceColor color)
        {
            return IsTyrant(color) ? 400 : 0;
        }
    }
}

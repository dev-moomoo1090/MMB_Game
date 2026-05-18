using UnityEngine;

namespace MMBGame
{
    public partial class KingStateEffectApplier
    {
        private void ApplyBenevolentEffect(BoardState board, PlayerState player, PieceColor color)
        {
            int supportGain = Mathf.CeilToInt(player.honor / 50f);
            if (supportGain > 0)
            {
                ApplyToAllPieces(board, color, piece => PoliticalStatService.ChangeSupport(piece, supportGain, "Benevolent"));
            }

            int highSupportCount = CountPieces(board, color, piece => IsPoliticalPiece(piece) && piece.support >= 51);
            int totalPieces = CountPieces(board, color, IsPoliticalPiece);
            if (highSupportCount <= 0 || totalPieces <= 0)
            {
                return;
            }

            int totalSupportScore = SumSupport(board, color);
            int honorGain = Mathf.FloorToInt((float)totalSupportScore / highSupportCount * totalPieces);
            if (honorGain > 0)
            {
                player.AddHonor(honorGain);
            }
        }

        private void ApplyIncompetentEffect(BoardState board, PieceColor color)
        {
            if (!incompetentPenaltyAppliedColors.Contains(color))
            {
                board.globalAcceptanceWeight -= INCOMPETENT_INITIAL_ACCEPTANCE_PENALTY;
                ApplyToAllPieces(board, color, ApplyIncompetentInitialPenalty);
                incompetentPenaltyAppliedColors.Add(color);
            }

            board.globalAcceptanceWeight += INCOMPETENT_TURN_ACCEPTANCE_GAIN;
            ApplyToAllPieces(board, color, ApplyIncompetentTurnGrowth);
            incompetentTurnCounts[color]++;
            if (incompetentTurnCounts[color] < INCOMPETENT_SURVIVAL_TURNS)
            {
                return;
            }

            board.globalAcceptanceWeight += INCOMPETENT_FIXED_ACCEPTANCE_GAIN;
            ApplyToAllPieces(board, color, ApplyIncompetentFixedBonus);
            KingStateEvaluator.FixBenevolent(color);
            KingStateEvaluator.SetCurrentState(color, KingState.Sage);
            incompetentTurnCounts[color] = 0;
        }

        private void ApplyIncompetentInitialPenalty(ChessPiece piece)
        {
            piece.taxPerTurn = Mathf.Max(1, Mathf.FloorToInt(piece.taxPerTurn * INCOMPETENT_INITIAL_TAX_RATE));
            PoliticalStatService.ChangeSupport(piece, -INCOMPETENT_INITIAL_SUPPORT_PENALTY, "IncompetentInitial");
        }

        private void ApplyIncompetentTurnGrowth(ChessPiece piece)
        {
            piece.taxPerTurn = Mathf.Max(piece.taxPerTurn, Mathf.CeilToInt(piece.taxPerTurn * INCOMPETENT_TURN_TAX_RATE));
            PoliticalStatService.ChangeSupport(piece, INCOMPETENT_TURN_SUPPORT_GAIN, "IncompetentTurn");
        }

        private void ApplyIncompetentFixedBonus(ChessPiece piece)
        {
            piece.taxPerTurn = Mathf.CeilToInt(piece.taxPerTurn * INCOMPETENT_FIXED_TAX_RATE);
            PoliticalStatService.ChangeSupport(piece, INCOMPETENT_FIXED_SUPPORT_GAIN, "IncompetentFixed");
        }
    }
}

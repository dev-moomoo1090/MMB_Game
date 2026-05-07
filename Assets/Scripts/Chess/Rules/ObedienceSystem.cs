using UnityEngine;

namespace MMBGame
{
    public static class ObedienceSystem
    {
        public static bool IsRefused(ChessPiece piece)
        {
            return IsRefused(piece, null);
        }

        public static bool IsRefused(ChessPiece piece, BoardState state)
        {
            if (piece == null || KingStateEvaluator.SuppressesRefusal(piece.color))
            {
                return false;
            }

            return RollRefusal(piece, state);
        }

        public static bool RollRefusal(ChessPiece piece)
        {
            return RollRefusal(piece, null);
        }

        public static bool RollRefusal(ChessPiece piece, BoardState state)
        {
            float refusalChance = GetRefusalChance(piece, state);
            if (refusalChance <= 0f)
            {
                return false;
            }

            float roll = Random.Range(0f, 100f);
            return roll < refusalChance;
        }

        public static float GetRefusalChance(ChessPiece piece)
        {
            return GetRefusalChance(piece, null);
        }

        public static float GetRefusalChance(ChessPiece piece, BoardState state)
        {
            if (piece == null)
            {
                return 0f;
            }

            float acceptanceChance = GetMovementAcceptanceChance(piece, state);
            return Mathf.Clamp(100f - acceptanceChance, 0f, 100f);
        }

        public static float GetMovementAcceptanceChance(ChessPiece piece, BoardState state)
        {
            if (piece == null)
            {
                return 0f;
            }

            int defenders = 0;
            int attackers = 0;
            int globalWeight = state != null ? state.globalAcceptanceWeight : 0;
            globalWeight += KingStateEvaluator.GetAcceptanceWeightModifier(piece.color);
            if (state != null && state.IsInBounds(piece.file, piece.rank))
            {
                PieceColor opponent = piece.color == PieceColor.White ? PieceColor.Black : PieceColor.White;
                defenders = BoardEvaluator.CountDefenders(state, piece.file, piece.rank, piece.color);
                attackers = BoardEvaluator.CountAttackers(state, piece.file, piece.rank, opponent);
            }

            return Mathf.Clamp(
                50 + piece.support + defenders * 50 - attackers * 50 + globalWeight + GetPieceAcceptanceWeight(piece),
                0f,
                100f);
        }

        public static float GetGeneralAcceptanceChance(ChessPiece piece, BoardState state)
        {
            if (piece == null)
            {
                return 0f;
            }

            int globalWeight = state != null ? state.globalAcceptanceWeight : 0;
            globalWeight += KingStateEvaluator.GetAcceptanceWeightModifier(piece.color);
            return Mathf.Clamp(50 + piece.support + globalWeight + GetPieceAcceptanceWeight(piece), 0f, 100f);
        }

        private static int GetPieceAcceptanceWeight(ChessPiece piece)
        {
            return piece.acceptWeight - 50 + piece.disposition;
        }
    }
}

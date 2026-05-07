using UnityEngine;

namespace MMBGame
{
    public static class ObedienceSystem
    {
        public static bool IsRefused(ChessPiece piece)
        {
            if (piece == null || KingStateEvaluator.SuppressesRefusal(piece.color))
            {
                return false;
            }

            return RollRefusal(piece);
        }

        public static bool RollRefusal(ChessPiece piece)
        {
            float refusalChance = GetRefusalChance(piece);
            if (refusalChance <= 0f)
            {
                return false;
            }

            float roll = Random.Range(0f, 100f);
            return roll < refusalChance;
        }

        public static float GetRefusalChance(ChessPiece piece)
        {
            if (piece == null)
            {
                return 0f;
            }

            float refusalChance = (50 - piece.support) * 0.5f;
            refusalChance -= piece.disposition * 0.1f;
            return Mathf.Clamp(refusalChance, 0f, 50f);
        }
    }
}

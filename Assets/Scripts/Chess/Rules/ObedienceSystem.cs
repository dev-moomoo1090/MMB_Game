using UnityEngine;

namespace MMBGame
{
    public static class ObedienceSystem
    {
        public static bool IsRefused(ChessPiece piece)
        {
            float refusalChance = (50 - piece.support) * 0.5f;
            refusalChance -= piece.disposition * 0.1f;
            refusalChance = Mathf.Clamp(refusalChance, 0f, 50f);

            if (refusalChance <= 0f)
            {
                return false;
            }

            float roll = Random.Range(0f, 100f);
            return roll < refusalChance;
        }
    }
}

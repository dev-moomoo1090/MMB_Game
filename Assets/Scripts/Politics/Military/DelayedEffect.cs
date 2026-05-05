using System.Collections.Generic;

namespace MMBGame
{
    public enum DelayedEffectType
    {
        PlaceBarricade,
        PlaceTrebuchet,
        Bombard,
        ApplyEnhancement
    }

    public class DelayedEffect
    {
        public DelayedEffectType effectType;
        public int targetFile;
        public int targetRank;
        public PieceColor ownerColor;
        public int turnsRemaining;
        public ChessPiece targetPiece;
        public List<MovePattern> patternsToAdd;

        public DelayedEffect(DelayedEffectType type, int file, int rank, PieceColor color, int turns)
        {
            effectType = type;
            targetFile = file;
            targetRank = rank;
            ownerColor = color;
            turnsRemaining = turns;
            patternsToAdd = new List<MovePattern>();
        }
    }
}

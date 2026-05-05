using System.Collections.Generic;
using UnityEngine;

namespace MMBGame
{
    [System.Serializable]
    public class PieceSetupDefinition
    {
        public PieceColor color = PieceColor.White;
        public PieceSide side = PieceSide.None;
        public PieceType type = PieceType.Pawn;
        public GameObject prefab;
        public GameObject selectedPrefab;
        public Sprite sprite;
        public Sprite selectedSprite;
        public int initialTaxPerTurn = 1;
        public int initialSupport = 50;
        public List<MovePattern> baseMovePatterns = new List<MovePattern>();
    }
}

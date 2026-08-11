using System.Collections.Generic;
using UnityEngine;

namespace MMBGame
{
    [System.Serializable]
    public class PieceSetupDefinition
    {
        public PieceColor color = PieceColor.White;
        public PieceSide side = PieceSide.None;
        public PieceLane lane = PieceLane.None;
        public PieceType type = PieceType.Pawn;
        public GameObject prefab;
        public GameObject selectedPrefab;
        public Sprite sprite;
        public Sprite selectedSprite;
        public Sprite[] attackSprites;
        public float attackFrameDuration = 0.1f;
        public float attackScale = 2f;
        public int initialTaxPerTurn = 1;
        public int initialSupport = 50;
        public List<MovePattern> baseMovePatterns = new List<MovePattern>();
    }
}

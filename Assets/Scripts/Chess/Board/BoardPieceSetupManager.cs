using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace MMBGame
{
    public class BoardPieceSetupManager : MonoBehaviour
    {
        [SerializeField] private BoardPieceVisuals pieceVisuals;
        [SerializeField] private List<PieceSetupDefinition> pieceDefinitions = new List<PieceSetupDefinition>();

        public IReadOnlyList<PieceSetupDefinition> PieceDefinitions => pieceDefinitions;

        private void Awake()
        {
            EnsureDefaultDefinitions();
        }

        private void OnEnable()
        {
#if UNITY_EDITOR
            SchedulePrefabAssignment();
#endif
        }

        private void Reset()
        {
            EnsureDefaultDefinitions();
#if UNITY_EDITOR
            SchedulePrefabAssignment();
#endif
        }

        private void OnValidate()
        {
            EnsureDefaultDefinitions();
#if UNITY_EDITOR
            SchedulePrefabAssignment();
#endif
        }

        public void ApplySetup(ChessPiece piece)
        {
            if (piece == null)
            {
                return;
            }

            piece.side = PieceSideResolver.Resolve(piece.type, piece.rank);
            piece.lane = PieceSideResolver.ResolveLane(piece.type, piece.rank);
            piece.ApplySetup(GetPieceDefinition(piece.color, piece.side, piece.type, piece.lane));
        }

        public void ApplyPromotionSetup(BoardState boardState, Move move)
        {
            if (boardState == null || move.specialMove != SpecialMoveType.Promotion)
            {
                return;
            }

            ChessPiece promotedPiece = boardState.GetPiece(move.toFile, move.toRank);
            if (promotedPiece == null)
            {
                return;
            }

            promotedPiece.side = PieceSideResolver.Resolve(promotedPiece.type, move.toRank);
            promotedPiece.lane = PieceSideResolver.ResolveLane(promotedPiece.type, move.toRank);
            promotedPiece.ApplySetup(GetPieceDefinition(promotedPiece.color, promotedPiece.side, promotedPiece.type, promotedPiece.lane));
            promotedPiece.hasMoved = true;
        }

        public void SyncVisuals(BoardState boardState)
        {
            EnsureVisuals();
            if (pieceVisuals != null)
            {
                pieceVisuals.Sync(boardState, pieceDefinitions);
            }
        }

        public bool TryPlayAttackMotion(int attackerFile, int attackerRank, int approachFile, int approachRank, out Coroutine coroutine)
        {
            EnsureVisuals();
            if (pieceVisuals != null)
            {
                return pieceVisuals.TryPlayAttackMotion(attackerFile, attackerRank, approachFile, approachRank, out coroutine);
            }

            coroutine = null;
            return false;
        }

        public PieceSetupDefinition GetPieceDefinition(PieceColor color, PieceSide side, PieceType type, PieceLane lane = PieceLane.None)
        {
            PieceSetupDefinition sideDefinition = null;
            PieceSetupDefinition fallbackDefinition = null;
            for (int i = 0; i < pieceDefinitions.Count; i++)
            {
                PieceSetupDefinition definition = pieceDefinitions[i];
                if (definition == null || definition.color != color || definition.type != type)
                {
                    continue;
                }

                if (lane != PieceLane.None && definition.lane == lane)
                {
                    return definition;
                }

                if (definition.side == side && definition.lane == PieceLane.None)
                {
                    sideDefinition = definition;
                }

                if (definition.side == PieceSide.None && definition.lane == PieceLane.None)
                {
                    fallbackDefinition = definition;
                }
            }

            return sideDefinition ?? fallbackDefinition;
        }

        private void EnsureVisuals()
        {
            pieceVisuals = SceneComponentResolver.ResolveOrAdd(pieceVisuals, gameObject);
        }

        [ContextMenu("Fill Default Piece Definitions")]
        private void FillDefaultPieceDefinitions()
        {
            PieceSetupDefaults.Fill(pieceDefinitions);
        }

        private void EnsureDefaultDefinitions()
        {
            if (pieceDefinitions.Count == 0)
            {
                FillDefaultPieceDefinitions();
                return;
            }

            if (PieceSetupDefaults.UpgradeIfNeeded(pieceDefinitions))
            {
#if UNITY_EDITOR
                SchedulePrefabAssignment();
#endif
            }
        }

#if UNITY_EDITOR
        [ContextMenu("Assign Missing Prefab References")]
        private void AssignMissingPrefabReferences()
        {
            AssignMissingPrefabReferencesImmediate();
        }

        private void SchedulePrefabAssignment()
        {
            EditorApplication.delayCall -= AssignMissingPrefabReferencesDelayed;
            EditorApplication.delayCall += AssignMissingPrefabReferencesDelayed;
        }

        private void AssignMissingPrefabReferencesDelayed()
        {
            if (this == null)
            {
                return;
            }

            AssignMissingPrefabReferencesImmediate();
            EditorUtility.SetDirty(this);
        }

        private void AssignMissingPrefabReferencesImmediate()
        {
            EnsureDefaultDefinitions();
            bool changed = false;
            for (int i = 0; i < pieceDefinitions.Count; i++)
            {
                changed |= AssignMissingPrefabReferences(pieceDefinitions[i]);
            }

            if (changed)
            {
                EditorUtility.SetDirty(this);
            }
        }

        private bool AssignMissingPrefabReferences(PieceSetupDefinition definition)
        {
            if (definition == null)
            {
                return false;
            }

            bool changed = false;
            if (definition.prefab == null)
            {
                definition.prefab = LoadPrefab(definition, string.Empty);
                changed = definition.prefab != null;
            }

            if (definition.selectedPrefab == null)
            {
                definition.selectedPrefab = LoadPrefab(definition, "_Highlight");
                changed = changed || definition.selectedPrefab != null;
            }

            return changed;
        }

        private GameObject LoadPrefab(PieceSetupDefinition definition, string suffix)
        {
            string path = GetPrefabPath(definition, suffix);
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            if (prefab != null)
            {
                return prefab;
            }

            prefab = AssetDatabase.LoadAssetAtPath<GameObject>(GetSidePrefabPath(definition, suffix));
            if (prefab != null)
            {
                return prefab;
            }

            return AssetDatabase.LoadAssetAtPath<GameObject>(GetPawnFallbackPath(definition.color, definition.side, suffix));
        }

        private string GetPrefabPath(PieceSetupDefinition definition, string suffix)
        {
            string laneName = definition.lane == PieceLane.None ? string.Empty : "_" + definition.lane;
            string path = "Assets/Prefabs/Piece/" + definition.color + "_" + definition.type + laneName + suffix + ".prefab";
            if (definition.lane == PieceLane.None)
            {
                string sideName = definition.side == PieceSide.None ? string.Empty : "_" + definition.side;
                path = "Assets/Prefabs/Piece/" + definition.color + "_" + definition.type + sideName + suffix + ".prefab";
            }

            return path;
        }

        private string GetSidePrefabPath(PieceSetupDefinition definition, string suffix)
        {
            string sideName = definition.side == PieceSide.None ? string.Empty : "_" + definition.side;
            return "Assets/Prefabs/Piece/" + definition.color + "_" + definition.type + sideName + suffix + ".prefab";
        }

        private string GetPawnFallbackPath(PieceColor color, PieceSide side, string suffix)
        {
            string sideName = side == PieceSide.Kingside ? "_Kingside" : "_Queenside";
            return "Assets/Prefabs/Piece/" + color + "_Pawn" + sideName + suffix + ".prefab";
        }
#endif
    }
}

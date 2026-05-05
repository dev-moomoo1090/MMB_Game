using System.Collections.Generic;
using UnityEngine;

namespace MMBGame
{
    public class BoardPieceVisuals : MonoBehaviour
    {
        [SerializeField] private Transform boardRoot;
        [SerializeField] private Transform pieceRoot;
        [SerializeField] private Vector3 pieceOffset = new Vector3(0f, 0.25f, -0.1f);
        [SerializeField] private Vector3 pieceScale = Vector3.one;
        [SerializeField] private int sortingOrder = 20;

        private readonly List<GameObject> spawnedPieces = new List<GameObject>();
        private BoardPieceVisual selectedVisual;

        public void Sync(BoardState boardState, IReadOnlyList<PieceSetupDefinition> definitions)
        {
            Clear();
            selectedVisual = null;
            if (boardState == null)
            {
                return;
            }

            EnsurePieceRoot();
            for (int file = 0; file < 8; file++)
            {
                for (int rank = 0; rank < 8; rank++)
                {
                    ChessPiece piece = boardState.GetPiece(file, rank);
                    if (piece != null)
                    {
                        SpawnPiece(piece, definitions);
                    }
                }
            }
        }

        public void Clear()
        {
            for (int i = spawnedPieces.Count - 1; i >= 0; i--)
            {
                if (spawnedPieces[i] != null)
                {
                    Destroy(spawnedPieces[i]);
                }
            }

            spawnedPieces.Clear();
            selectedVisual = null;
        }

        public void SelectVisual(BoardPieceVisual visual)
        {
            if (selectedVisual != null && selectedVisual != visual)
            {
                selectedVisual.SetSelected(false);
            }

            selectedVisual = visual;
            if (selectedVisual != null)
            {
                selectedVisual.SetSelected(true);
            }
        }

        private void SpawnPiece(ChessPiece piece, IReadOnlyList<PieceSetupDefinition> definitions)
        {
            PieceSetupDefinition definition = FindDefinition(piece, definitions);
            if (definition == null || (definition.prefab == null && definition.sprite == null))
            {
                return;
            }

            GameObject pieceObject = new GameObject(piece.pieceName);
            pieceObject.transform.SetParent(pieceRoot, false);
            pieceObject.transform.position = GetBoardPosition(piece.file, piece.rank) + pieceOffset;
            pieceObject.transform.localScale = pieceScale;

            BoardPieceVisual pieceVisual = pieceObject.AddComponent<BoardPieceVisual>();
            if (definition.prefab != null)
            {
                GameObject normalModel = Instantiate(definition.prefab, pieceObject.transform);
                normalModel.name = "Normal";
                ApplySortingOrder(normalModel);
                GameObject selectedModel = null;
                if (definition.selectedPrefab != null)
                {
                    selectedModel = Instantiate(definition.selectedPrefab, pieceObject.transform);
                    selectedModel.name = "Selected";
                    ApplySortingOrder(selectedModel);
                }

                pieceVisual.Initialize(this, piece, normalModel, selectedModel);
            }
            else
            {
                SpriteRenderer spriteRenderer = pieceObject.AddComponent<SpriteRenderer>();
                spriteRenderer.sprite = definition.sprite;
                spriteRenderer.sortingOrder = sortingOrder;
                pieceVisual.Initialize(this, piece, definition.sprite, definition.selectedSprite);
            }

            spawnedPieces.Add(pieceObject);
        }

        private void ApplySortingOrder(GameObject root)
        {
            SpriteRenderer[] renderers = root.GetComponentsInChildren<SpriteRenderer>();
            for (int i = 0; i < renderers.Length; i++)
            {
                renderers[i].sortingOrder = sortingOrder;
            }
        }

        private PieceSetupDefinition FindDefinition(ChessPiece piece, IReadOnlyList<PieceSetupDefinition> definitions)
        {
            if (definitions == null)
            {
                return null;
            }

            PieceSetupDefinition fallbackDefinition = null;
            for (int i = 0; i < definitions.Count; i++)
            {
                PieceSetupDefinition definition = definitions[i];
                if (definition == null || definition.color != piece.color || definition.type != piece.type)
                {
                    continue;
                }

                if (definition.side == piece.side)
                {
                    return definition;
                }

                if (definition.side == PieceSide.None)
                {
                    fallbackDefinition = definition;
                }
            }

            return fallbackDefinition;
        }

        private Vector3 GetBoardPosition(int file, int rank)
        {
            Transform tile = FindTile(file, rank);
            if (tile != null)
            {
                return tile.position;
            }

            const float halfW = 1.17f;
            const float halfH = 0.59f;
            return new Vector3((file - rank) * halfW, -(file + rank) * halfH, 0f);
        }

        private Transform FindTile(int file, int rank)
        {
            Transform root = ResolveBoardRoot();
            Transform directTile = root.Find("Tile_" + file + "_" + rank);
            if (directTile != null)
            {
                return directTile;
            }

            GameObject tileObject = GameObject.Find("Tile_" + file + "_" + rank);
            return tileObject != null ? tileObject.transform : null;
        }

        private Transform ResolveBoardRoot()
        {
            if (boardRoot != null)
            {
                return boardRoot;
            }

            GameObject rootObject = GameObject.Find("ChessBoard");
            if (rootObject != null)
            {
                boardRoot = rootObject.transform;
                return boardRoot;
            }

            return transform;
        }

        private void EnsurePieceRoot()
        {
            if (pieceRoot != null)
            {
                return;
            }

            GameObject rootObject = new GameObject("PieceRoot");
            rootObject.transform.SetParent(transform, false);
            pieceRoot = rootObject.transform;
        }
    }
}

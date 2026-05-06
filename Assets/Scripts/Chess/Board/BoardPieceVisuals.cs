using System.Collections.Generic;
using UnityEngine;

namespace MMBGame
{
    public class BoardPieceVisuals : MonoBehaviour
    {
        [SerializeField] private Transform boardRoot;
        [SerializeField] private Transform pieceRoot;
        [SerializeField] private Vector3 pieceOffset = new Vector3(0f, 0f, -0.1f);
        [SerializeField] private Vector3 pieceScale = new Vector3(0.55f, 0.55f, 0.55f);
        [SerializeField] private int sortingOrder = 20;

        private readonly List<GameObject> spawnedPieces = new List<GameObject>();
        private BoardPieceVisual selectedVisual;
        private BoardInteraction boardInteraction;

        private void Awake()
        {
            EnsureInteraction();
        }

        public void Sync(BoardState boardState, IReadOnlyList<PieceSetupDefinition> definitions)
        {
            Clear();
            selectedVisual = null;
            if (boardState == null)
            {
                return;
            }

            EnsurePieceRoot();
            EnsureInteraction();
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
            if (boardInteraction != null)
            {
                boardInteraction.ClearSelection();
            }
        }

        public void SelectVisual(BoardPieceVisual visual)
        {
            EnsureInteraction();
            if (boardInteraction != null)
            {
                boardInteraction.HandlePieceClicked(visual);
                return;
            }

            SelectVisualOnly(visual);
        }

        public void SelectVisualOnly(BoardPieceVisual visual)
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

        public void ClearSelection()
        {
            if (selectedVisual != null)
            {
                selectedVisual.SetSelected(false);
            }

            selectedVisual = null;
        }

        public bool TryGetLogicalSquare(Vector3 worldPosition, out int file, out int rank)
        {
            return BoardCoordinateMapper.TryGetLogicalSquare(ResolveBoardRoot(), worldPosition, out file, out rank);
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
                AlignModelToTileCenter(normalModel);
                ApplySortingOrder(normalModel);
                GameObject selectedModel = null;
                if (definition.selectedPrefab != null)
                {
                    selectedModel = Instantiate(definition.selectedPrefab, pieceObject.transform);
                    selectedModel.name = "Selected";
                    AlignModelToTileCenter(selectedModel);
                    ApplySortingOrder(selectedModel);
                }

                pieceVisual.Initialize(this, piece, normalModel, selectedModel);
            }
            else
            {
                SpriteRenderer spriteRenderer = pieceObject.AddComponent<SpriteRenderer>();
                spriteRenderer.sprite = definition.sprite;
                spriteRenderer.sortingOrder = sortingOrder;
                AlignSpriteToTileCenter(spriteRenderer);
                pieceVisual.Initialize(this, piece, definition.sprite, definition.selectedSprite);
            }

            spawnedPieces.Add(pieceObject);
        }

        private void AlignModelToTileCenter(GameObject model)
        {
            SpriteRenderer renderer = model.GetComponentInChildren<SpriteRenderer>();
            if (renderer == null)
            {
                return;
            }

            Bounds bounds = renderer.localBounds;
            model.transform.localPosition -= new Vector3(bounds.center.x, bounds.min.y, 0f);
        }

        private void AlignSpriteToTileCenter(SpriteRenderer renderer)
        {
            if (renderer == null || renderer.sprite == null)
            {
                return;
            }

            Bounds bounds = renderer.localBounds;
            renderer.transform.localPosition -= new Vector3(bounds.center.x, bounds.min.y, 0f);
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
                SpriteRenderer tileRenderer = tile.GetComponent<SpriteRenderer>();
                if (tileRenderer != null)
                {
                    return tileRenderer.bounds.center;
                }

                return tile.position;
            }

            return BoardCoordinateMapper.GetFallbackWorldPosition(file, rank);
        }

        private Transform FindTile(int file, int rank)
        {
            Transform root = ResolveBoardRoot();
            Transform directTile = root.Find(BoardCoordinateMapper.GetTileName(file, rank));
            if (directTile != null)
            {
                return directTile;
            }

            GameObject tileObject = GameObject.Find(BoardCoordinateMapper.GetTileName(file, rank));
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

        private void EnsureInteraction()
        {
            if (boardInteraction != null)
            {
                boardInteraction.Initialize(this);
                return;
            }

            boardInteraction = GetComponent<BoardInteraction>();
            if (boardInteraction == null)
            {
                boardInteraction = gameObject.AddComponent<BoardInteraction>();
            }

            boardInteraction.Initialize(this);
        }
    }
}

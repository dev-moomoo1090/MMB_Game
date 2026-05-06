using System.Collections.Generic;
using UnityEngine;

namespace MMBGame
{
    public class BoardMoveIndicators : MonoBehaviour
    {
        [SerializeField] private Transform boardRoot;
        [SerializeField] private float circleSize = 0.45f;
        [SerializeField] private int sortingOrder = 15;
        [SerializeField] private Color moveColor = new Color(1f, 1f, 1f, 0.45f);
        [SerializeField] private Color captureColor = new Color(0.35f, 0.35f, 0.35f, 0.55f);

        private readonly List<GameObject> indicators = new List<GameObject>();
        private Sprite circleSprite;
        private Transform indicatorRoot;

        public void Show(BoardManager boardManager, BoardPieceVisual visual)
        {
            Hide();
            if (boardManager == null || boardManager.BoardState == null || visual == null)
            {
                return;
            }

            List<Move> moves = boardManager.GetLegalMoves(visual.File, visual.Rank);
            for (int i = 0; i < moves.Count; i++)
            {
                Move move = moves[i];
                ChessPiece target = boardManager.BoardState.GetPiece(move.toFile, move.toRank);
                CreateIndicator(move.toFile, move.toRank, target != null ? captureColor : moveColor);
            }
        }

        public void Hide()
        {
            for (int i = indicators.Count - 1; i >= 0; i--)
            {
                if (indicators[i] != null)
                {
                    Destroy(indicators[i]);
                }
            }

            indicators.Clear();
        }

        private void CreateIndicator(int file, int rank, Color color)
        {
            EnsureRoot();
            EnsureCircleSprite();
            GameObject indicator = new GameObject("MoveIndicator");
            indicator.transform.SetParent(indicatorRoot, false);
            indicator.transform.position = GetSquareCenter(file, rank) + new Vector3(0f, 0f, -0.05f);
            indicator.transform.localScale = new Vector3(circleSize, circleSize, 1f);

            SpriteRenderer renderer = indicator.AddComponent<SpriteRenderer>();
            renderer.sprite = circleSprite;
            renderer.color = color;
            renderer.sortingOrder = sortingOrder;
            indicators.Add(indicator);
        }

        private Vector3 GetSquareCenter(int file, int rank)
        {
            Transform tile = FindTile(file, rank);
            if (tile != null)
            {
                SpriteRenderer renderer = tile.GetComponent<SpriteRenderer>();
                if (renderer != null)
                {
                    return renderer.bounds.center;
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

        private void EnsureRoot()
        {
            if (indicatorRoot != null)
            {
                return;
            }

            GameObject rootObject = new GameObject("MoveIndicatorRoot");
            rootObject.transform.SetParent(transform, false);
            indicatorRoot = rootObject.transform;
        }

        private void EnsureCircleSprite()
        {
            if (circleSprite != null)
            {
                return;
            }

            const int size = 64;
            Texture2D texture = new Texture2D(size, size, TextureFormat.RGBA32, false);
            texture.filterMode = FilterMode.Bilinear;
            float radius = (size - 2) * 0.5f;
            Vector2 center = new Vector2((size - 1) * 0.5f, (size - 1) * 0.5f);
            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    float alpha = Vector2.Distance(new Vector2(x, y), center) <= radius ? 1f : 0f;
                    texture.SetPixel(x, y, new Color(1f, 1f, 1f, alpha));
                }
            }

            texture.Apply();
            circleSprite = Sprite.Create(texture, new Rect(0f, 0f, size, size), new Vector2(0.5f, 0.5f), size);
        }
    }
}

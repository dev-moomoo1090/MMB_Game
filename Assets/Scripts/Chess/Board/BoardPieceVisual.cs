using UnityEngine;

namespace MMBGame
{
    [RequireComponent(typeof(SpriteRenderer))]
    [RequireComponent(typeof(BoxCollider2D))]
    public class BoardPieceVisual : MonoBehaviour
    {
        private BoardPieceVisuals owner;
        private SpriteRenderer spriteRenderer;
        private Sprite normalSprite;
        private Sprite selectedSprite;
        private GameObject normalModel;
        private GameObject selectedModel;
        private int file;
        private int rank;
        private ChessPiece piece;

        public int File => file;
        public int Rank => rank;
        public ChessPiece Piece => piece;

        public void Initialize(BoardPieceVisuals newOwner, ChessPiece newPiece, Sprite newNormalSprite, Sprite newSelectedSprite)
        {
            owner = newOwner;
            piece = newPiece;
            file = newPiece.file;
            rank = newPiece.rank;
            normalSprite = newNormalSprite;
            selectedSprite = newSelectedSprite != null ? newSelectedSprite : newNormalSprite;
            spriteRenderer = GetComponent<SpriteRenderer>();
            spriteRenderer.sprite = normalSprite;
            FitColliderToSprite();
        }

        public void Initialize(BoardPieceVisuals newOwner, ChessPiece newPiece, GameObject newNormalModel, GameObject newSelectedModel)
        {
            owner = newOwner;
            piece = newPiece;
            file = newPiece.file;
            rank = newPiece.rank;
            normalModel = newNormalModel;
            selectedModel = newSelectedModel;
            SetSelected(false);
            FitColliderToChildren();
        }

        public void SetSelected(bool selected)
        {
            if (normalModel != null || selectedModel != null)
            {
                if (normalModel != null)
                {
                    normalModel.SetActive(!selected || selectedModel == null);
                }

                if (selectedModel != null)
                {
                    selectedModel.SetActive(selected);
                }

                FitColliderToChildren();
                return;
            }

            if (spriteRenderer == null)
            {
                spriteRenderer = GetComponent<SpriteRenderer>();
            }

            spriteRenderer.sprite = selected ? selectedSprite : normalSprite;
            FitColliderToSprite();
        }

        private void OnMouseDown()
        {
            owner?.SelectVisual(this);
        }

        private void FitColliderToSprite()
        {
            BoxCollider2D boxCollider = GetComponent<BoxCollider2D>();
            if (spriteRenderer == null || spriteRenderer.sprite == null || boxCollider == null)
            {
                return;
            }

            boxCollider.offset = spriteRenderer.sprite.bounds.center;
            boxCollider.size = spriteRenderer.sprite.bounds.size;
        }

        private void FitColliderToChildren()
        {
            BoxCollider2D boxCollider = GetComponent<BoxCollider2D>();
            if (boxCollider == null)
            {
                return;
            }

            SpriteRenderer[] renderers = GetComponentsInChildren<SpriteRenderer>();
            Bounds? bounds = null;
            for (int i = 0; i < renderers.Length; i++)
            {
                if (!renderers[i].gameObject.activeInHierarchy)
                {
                    continue;
                }

                if (bounds.HasValue)
                {
                    Bounds combinedBounds = bounds.Value;
                    combinedBounds.Encapsulate(renderers[i].bounds);
                    bounds = combinedBounds;
                }
                else
                {
                    bounds = renderers[i].bounds;
                }
            }

            if (!bounds.HasValue)
            {
                return;
            }

            Vector3 localMin = transform.InverseTransformPoint(bounds.Value.min);
            Vector3 localMax = transform.InverseTransformPoint(bounds.Value.max);
            Bounds localBounds = new Bounds();
            localBounds.SetMinMax(Vector3.Min(localMin, localMax), Vector3.Max(localMin, localMax));
            boxCollider.offset = localBounds.center;
            boxCollider.size = localBounds.size;
        }
    }
}

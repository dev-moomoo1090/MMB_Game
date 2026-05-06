using UnityEngine;
using UnityEngine.InputSystem;

namespace MMBGame
{
    public class BoardInteraction : MonoBehaviour
    {
        private BoardManager boardManager;
        private BoardPieceVisuals pieceVisuals;
        private BoardMoveIndicators moveIndicators;
        private BoardPieceProfileDisplay profileDisplay;
        private BoardPieceVisual selectedVisual;

        public void Initialize(BoardPieceVisuals newPieceVisuals)
        {
            pieceVisuals = newPieceVisuals;
            if (boardManager == null)
            {
                boardManager = FindFirstObjectByType<BoardManager>();
            }

            EnsureMoveIndicators();
            EnsureProfileDisplay();
        }

        public void ClearSelection()
        {
            selectedVisual = null;
            if (moveIndicators != null)
            {
                moveIndicators.Hide();
            }

            if (profileDisplay != null)
            {
                profileDisplay.Clear();
            }
        }

        public void HandlePieceClicked(BoardPieceVisual visual)
        {
            if (visual == null)
            {
                return;
            }

            if (selectedVisual != null && TryMoveSelectedTo(visual.File, visual.Rank))
            {
                return;
            }

            selectedVisual = visual;
            pieceVisuals.SelectVisualOnly(visual);
            ShowSelectedMoves();
            ShowSelectedProfile();
        }

        private void Update()
        {
            Mouse mouse = Mouse.current;
            if (mouse == null || !mouse.leftButton.wasPressedThisFrame || Camera.main == null)
            {
                return;
            }

            Vector2 screenPosition = mouse.position.ReadValue();
            Vector3 worldPosition = Camera.main.ScreenToWorldPoint(new Vector3(screenPosition.x, screenPosition.y, -Camera.main.transform.position.z));
            Collider2D collider = Physics2D.OverlapPoint(worldPosition);
            if (collider != null)
            {
                BoardPieceVisual visual = collider.GetComponent<BoardPieceVisual>();
                if (visual != null)
                {
                    HandlePieceClicked(visual);
                    return;
                }
            }

            if (selectedVisual == null || pieceVisuals == null)
            {
                return;
            }

            if (pieceVisuals.TryGetLogicalSquare(worldPosition, out int file, out int rank))
            {
                TryMoveSelectedTo(file, rank);
            }
        }

        private bool TryMoveSelectedTo(int file, int rank)
        {
            if (selectedVisual == null)
            {
                return false;
            }

            if (boardManager == null)
            {
                boardManager = FindFirstObjectByType<BoardManager>();
            }

            if (boardManager == null || !boardManager.TryMove(selectedVisual.File, selectedVisual.Rank, file, rank))
            {
                return false;
            }

            selectedVisual = null;
            pieceVisuals.ClearSelection();
            if (moveIndicators != null)
            {
                moveIndicators.Hide();
            }

            if (profileDisplay != null)
            {
                profileDisplay.Clear();
            }
            return true;
        }

        private void ShowSelectedMoves()
        {
            EnsureBoardManager();
            EnsureMoveIndicators();
            if (moveIndicators != null)
            {
                moveIndicators.Show(boardManager, selectedVisual);
            }
        }

        private void ShowSelectedProfile()
        {
            EnsureBoardManager();
            EnsureProfileDisplay();
            if (profileDisplay != null)
            {
                profileDisplay.Show(boardManager, selectedVisual);
            }
        }

        private void EnsureBoardManager()
        {
            if (boardManager == null)
            {
                boardManager = FindFirstObjectByType<BoardManager>();
            }
        }

        private void EnsureMoveIndicators()
        {
            if (moveIndicators != null)
            {
                return;
            }

            moveIndicators = GetComponent<BoardMoveIndicators>();
            if (moveIndicators == null)
            {
                moveIndicators = gameObject.AddComponent<BoardMoveIndicators>();
            }
        }

        private void EnsureProfileDisplay()
        {
            if (profileDisplay != null)
            {
                return;
            }

            profileDisplay = GetComponent<BoardPieceProfileDisplay>();
            if (profileDisplay == null)
            {
                profileDisplay = gameObject.AddComponent<BoardPieceProfileDisplay>();
            }
        }
    }
}

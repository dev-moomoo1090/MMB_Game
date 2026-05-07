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
        private TurnManager turnManager;
        private int selectedTargetFile = -1;
        private int selectedTargetRank = -1;

        public ChessPiece SelectedPiece => selectedVisual != null ? selectedVisual.Piece : null;
        public int SelectedTargetFile => selectedTargetFile;
        public int SelectedTargetRank => selectedTargetRank;

        public void Initialize(BoardPieceVisuals newPieceVisuals)
        {
            pieceVisuals = newPieceVisuals;
            if (boardManager == null)
            {
                boardManager = FindFirstObjectByType<BoardManager>();
            }

            if (turnManager == null)
            {
                turnManager = FindFirstObjectByType<TurnManager>();
            }

            EnsureMoveIndicators();
            EnsureProfileDisplay();
        }

        private void OnEnable()
        {
            EventBus.Instance.OnActionExecuted += HandleActionExecuted;
            EventBus.Instance.OnPhaseChanged += HandlePhaseChanged;
        }

        private void OnDisable()
        {
            EventBus.Instance.OnActionExecuted -= HandleActionExecuted;
            EventBus.Instance.OnPhaseChanged -= HandlePhaseChanged;
        }

        public void ClearSelection()
        {
            if (pieceVisuals != null)
            {
                pieceVisuals.ClearSelection();
            }

            selectedVisual = null;
            selectedTargetFile = -1;
            selectedTargetRank = -1;
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

            if (selectedVisual == visual)
            {
                ClearSelection();
                return;
            }

            selectedTargetFile = visual.File;
            selectedTargetRank = visual.Rank;

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

            if (FrontDeployPanel.IsOpen || PrisonerPanel.IsOpen)
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

            if (pieceVisuals == null)
            {
                return;
            }

            if (pieceVisuals.TryGetLogicalSquare(worldPosition, out int file, out int rank))
            {
                selectedTargetFile = file;
                selectedTargetRank = rank;
                if (selectedVisual != null)
                {
                    TryMoveSelectedTo(file, rank);
                }
            }
        }

        private bool TryMoveSelectedTo(int file, int rank)
        {
            if (selectedVisual == null)
            {
                return false;
            }

            if (!IsChessPhase())
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
            if (!IsChessPhase())
            {
                if (moveIndicators != null)
                {
                    moveIndicators.Hide();
                }

                return;
            }

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

        public void RefreshSelectionProfile()
        {
            if (selectedVisual == null)
            {
                return;
            }

            ShowSelectedMoves();
            ShowSelectedProfile();
        }

        private void HandleActionExecuted(PieceColor color, string actionName)
        {
            RefreshSelectionProfile();
        }

        private void HandlePhaseChanged(GamePhase phase)
        {
            if (phase != GamePhase.ChessPhase && moveIndicators != null)
            {
                moveIndicators.Hide();
            }

            RefreshSelectionProfile();
        }

        private bool IsChessPhase()
        {
            if (turnManager == null)
            {
                turnManager = FindFirstObjectByType<TurnManager>();
            }

            return turnManager != null && turnManager.CurrentPhase == GamePhase.ChessPhase;
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

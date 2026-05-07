using UnityEngine;
using UnityEngine.InputSystem;

namespace MMBGame
{
    public enum CommandActionCategory
    {
        Fiscal,
        Military,
        Political
    }

    [RequireComponent(typeof(BoxCollider2D))]
    public class CommandActionButton : MonoBehaviour
    {
        [SerializeField] private CommandActionCategory category;
        [SerializeField] private string actionName;
        [SerializeField] private int inputValue;
        [SerializeField] private PieceColor actorColor = PieceColor.White;
        [SerializeField] private ChessPiece selectedPiece;
        [SerializeField] private int targetFile = -1;
        [SerializeField] private int targetRank = -1;

        private PoliticsManager politicsManager;
        private MilitaryManager militaryManager;
        private PoliticalManager politicalManager;
        private TurnManager turnManager;
        private BoardInteraction boardInteraction;
        private CommandActionButtonVisualFeedback visualFeedback;
        private bool isHovered;
        private bool isPressed;

        public void Configure(CommandActionCategory newCategory, string newActionName)
        {
            category = newCategory;
            actionName = newActionName;
            gameObject.name = newActionName + "Button";
        }

        private void Awake()
        {
            politicsManager = FindFirstObjectByType<PoliticsManager>();
            militaryManager = FindFirstObjectByType<MilitaryManager>();
            politicalManager = FindFirstObjectByType<PoliticalManager>();
            turnManager = FindFirstObjectByType<TurnManager>();
            boardInteraction = FindFirstObjectByType<BoardInteraction>();
            visualFeedback = GetComponent<CommandActionButtonVisualFeedback>();
            if (visualFeedback == null)
            {
                visualFeedback = gameObject.AddComponent<CommandActionButtonVisualFeedback>();
            }

            visualFeedback.Initialize();
        }

        private void Update()
        {
            UpdatePointerState();
        }


        private void OnDisable()
        {
            if (visualFeedback != null)
            {
                visualFeedback.SetHovered(false);
            }

            CommandActionTooltip.Hide();
            SetPressed(false);
        }

        public bool Execute()
        {
            if (!CanExecuteInCurrentPhase())
            {
                return false;
            }

            return ExecuteWithValue(ResolveInputValue(ResolveSelectedPiece()));
        }

        private void HandleClick()
        {
            if (!CanExecuteInCurrentPhase())
            {
                return;
            }

            if (actionName == "FrontDeployAction")
            {
                ShowFrontDeployPanel();
                return;
            }

            if (CommandActionInputRequirements.RequiresValue(actionName))
            {
                CommandActionInputPrompt.Show(actionName, value => ExecuteWithValue(value));
                return;
            }

            Execute();
        }

        private bool CanExecuteInCurrentPhase()
        {
            if (turnManager == null)
            {
                turnManager = FindFirstObjectByType<TurnManager>();
            }

            return turnManager == null || turnManager.CurrentPhase == GamePhase.PoliticsPhase;
        }

        private void ShowFrontDeployPanel()
        {
            if (politicsManager == null)
            {
                return;
            }

            BoardManager boardManager = politicsManager.BoardManager;
            if (boardManager?.BoardState == null)
            {
                return;
            }

            BoardPieceSetupManager setupManager = FindFirstObjectByType<BoardPieceSetupManager>();
            PieceColor color = ResolveActorColor();

            FrontDeployPanel.Show(color, boardManager.BoardState, setupManager, piece =>
            {
                bool result = politicsManager.ExecuteFiscalAction("FrontDeployAction", piece, 0);
                RefreshProfileIfNeeded(result);
                if (result)
                {
                    ShowFiscalResultThenAdvance();
                }
            });
        }

        private bool ExecuteWithValue(int value)
        {
            if (!CanExecuteInCurrentPhase())
            {
                return false;
            }

            if (string.IsNullOrEmpty(actionName))
            {
                return false;
            }

            ChessPiece target = ResolveSelectedPiece();
            PieceColor color = ResolveActorColor();
            int resolvedFile = targetFile >= 0 ? targetFile : ResolveTargetFile();
            int resolvedRank = targetRank >= 0 ? targetRank : ResolveTargetRank();
            bool result;

            if (category == CommandActionCategory.Fiscal)
            {
                result = politicsManager != null && politicsManager.ExecuteFiscalAction(actionName, target, value);
            }
            else if (category == CommandActionCategory.Military)
            {
                result = militaryManager != null && militaryManager.ExecuteMilitaryAction(actionName, target, resolvedFile, resolvedRank, color);
            }
            else
            {
                result = politicalManager != null && politicalManager.ExecutePoliticalAction(actionName, target, color, value);
            }

            if (!result && category == CommandActionCategory.Political && politicalManager != null && !string.IsNullOrEmpty(politicalManager.LastFailureReason))
            {
                ActionResultPanel.Show("?됰룞 寃곌낵", politicalManager.LastFailureReason, null);
            }

            RefreshProfileIfNeeded(result);
            if (result && category == CommandActionCategory.Fiscal)
            {
                ShowFiscalResultThenAdvance();
            }
            else if (result)
            {
                ShowActionResultThenContinue(target, value, resolvedFile, resolvedRank, color);
            }

            return result;
        }

        private void ShowFiscalResultThenAdvance()
        {
            string resultText = politicsManager != null && !string.IsNullOrEmpty(politicsManager.LastFiscalActionResultText)
                ? politicsManager.LastFiscalActionResultText
                : "재정 행동이 실행되었습니다.";
            ActionResultPanel.Show("재정 행동 결과", resultText, AdvanceTurn);
        }

        private void ShowActionResultThenContinue(ChessPiece target, int value, int file, int rank, PieceColor color)
        {
            ActionResultContext context = new ActionResultContext
            {
                actionName = actionName,
                targetName = target != null ? GetPieceShortName(target) : "대상",
                inputValue = value,
                supportDelta = 0,
                goldDelta = 0,
                goldPerTurnDelta = 0,
                taxModifierBefore = target != null ? target.taxModifier : 1,
                taxModifierAfter = target != null ? target.taxModifier : 1,
                targetFile = file,
                targetRank = rank
            };

            string resultText = ActionResultText.Resolve(actionName, context);
            System.Action confirmAction = IsFreeAction(color) ? null : AdvanceTurn;
            ActionResultPanel.Show("행동 결과", resultText, confirmAction);
        }

        private bool IsFreeAction(PieceColor color)
        {
            return category == CommandActionCategory.Political &&
                actionName == "처형" &&
                KingStateEvaluator.IsTyrant(color);
        }

        private void AdvanceTurn()
        {
            if (turnManager == null)
            {
                turnManager = FindFirstObjectByType<TurnManager>();
            }

            turnManager?.EndPhase();
        }

        private ChessPiece ResolveSelectedPiece()
        {
            if (selectedPiece != null)
            {
                return selectedPiece;
            }

            if (boardInteraction == null)
            {
                boardInteraction = FindFirstObjectByType<BoardInteraction>();
            }

            return boardInteraction != null ? boardInteraction.SelectedPiece : null;
        }

        private PieceColor ResolveActorColor()
        {
            if (turnManager == null)
            {
                turnManager = FindFirstObjectByType<TurnManager>();
            }

            if (turnManager != null && turnManager.CurrentColor != PieceColor.None)
            {
                return turnManager.CurrentColor;
            }

            return actorColor;
        }

        private int ResolveTargetFile()
        {
            if (boardInteraction == null)
                boardInteraction = FindFirstObjectByType<BoardInteraction>();
            if (boardInteraction != null && boardInteraction.SelectedTargetFile >= 0)
                return boardInteraction.SelectedTargetFile;
            ChessPiece piece = ResolveSelectedPiece();
            return piece != null ? piece.file : -1;
        }

        private int ResolveTargetRank()
        {
            if (boardInteraction == null)
                boardInteraction = FindFirstObjectByType<BoardInteraction>();
            if (boardInteraction != null && boardInteraction.SelectedTargetRank >= 0)
                return boardInteraction.SelectedTargetRank;
            ChessPiece piece = ResolveSelectedPiece();
            return piece != null ? piece.rank : -1;
        }

        private int ResolveInputValue(ChessPiece target)
        {
            if (inputValue > 0)
            {
                return inputValue;
            }

            if (target != null)
            {
                return Mathf.Max(1, target.taxPerTurn);
            }

            return 10;
        }

        private void UpdatePointerState()
        {
            if (ActionResultPanel.IsOpen)
            {
                return;
            }

            if (visualFeedback == null || visualFeedback.Collider == null || Camera.main == null)
            {
                return;
            }

            Mouse mouse = Mouse.current;
            if (mouse == null)
            {
                return;
            }

            Vector2 screenPosition = mouse.position.ReadValue();
            Vector3 worldPosition = Camera.main.ScreenToWorldPoint(new Vector3(screenPosition.x, screenPosition.y, -Camera.main.transform.position.z));
            bool containsPointer = visualFeedback.Collider.OverlapPoint(worldPosition);

            if (containsPointer != isHovered)
            {
                isHovered = containsPointer;
                visualFeedback.SetHovered(isHovered);
                if (isHovered)
                {
                    CommandActionTooltip.Show(actionName, transform);
                }
            }

            if (containsPointer && mouse.leftButton.wasPressedThisFrame)
            {
                SetPressed(true);
                CommandActionTooltip.Hide();
                HandleClick();
            }

            if (isPressed && !mouse.leftButton.isPressed)
            {
                SetPressed(false);
            }
        }

        private void SetPressed(bool pressed)
        {
            if (isPressed == pressed)
            {
                return;
            }

            isPressed = pressed;
            if (visualFeedback != null)
            {
                visualFeedback.SetPressed(pressed);
            }
        }

        private void RefreshProfileIfNeeded(bool actionResult)
        {
            if (!actionResult)
            {
                return;
            }

            if (boardInteraction == null)
            {
                boardInteraction = FindFirstObjectByType<BoardInteraction>();
            }

            boardInteraction?.RefreshSelectionProfile();
        }

        private string GetPieceShortName(ChessPiece piece)
        {
            if (piece == null)
            {
                return "기물";
            }

            return GetColorName(piece.color) + GetSideName(piece.side) + GetTypeName(piece.type);
        }

        private string GetColorName(PieceColor color)
        {
            return color == PieceColor.Black ? "흑" : "백";
        }

        private string GetSideName(PieceSide side)
        {
            if (side == PieceSide.Kingside)
            {
                return "K";
            }

            if (side == PieceSide.Queenside)
            {
                return "Q";
            }

            return string.Empty;
        }

        private string GetTypeName(PieceType type)
        {
            switch (type)
            {
                case PieceType.Pawn: return "폰";
                case PieceType.Rook: return "룩";
                case PieceType.Knight: return "나이트";
                case PieceType.Bishop: return "비숍";
                case PieceType.Queen: return "퀸";
                case PieceType.King: return "킹";
                case PieceType.Barricade: return "바리케이드";
                case PieceType.Trebuchet: return "트레뷰셋";
                default: return type.ToString();
            }
        }
    }
}

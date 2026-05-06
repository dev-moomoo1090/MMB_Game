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

        private void OnMouseEnter()
        {
            isHovered = true;
            visualFeedback.SetHovered(true);
            CommandActionTooltip.Show(actionName, transform);
        }

        private void OnMouseExit()
        {
            isHovered = false;
            visualFeedback.SetHovered(false);
        }

        private void OnMouseDown()
        {
            SetPressed(true);
            CommandActionTooltip.Hide();
            HandleClick();
        }

        private void OnMouseUp()
        {
            SetPressed(false);
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
            return ExecuteWithValue(ResolveInputValue(ResolveSelectedPiece()));
        }

        private void HandleClick()
        {
            if (CommandActionInputRequirements.RequiresValue(actionName))
            {
                CommandActionInputPrompt.Show(actionName, value => ExecuteWithValue(value));
                return;
            }

            Execute();
        }

        private bool ExecuteWithValue(int value)
        {
            if (string.IsNullOrEmpty(actionName))
            {
                return false;
            }

            ChessPiece target = ResolveSelectedPiece();
            PieceColor color = ResolveActorColor();
            bool result;

            if (category == CommandActionCategory.Fiscal)
            {
                result = politicsManager != null && politicsManager.ExecuteFiscalAction(actionName, target, value);
                RefreshProfileIfNeeded(result);
                return result;
            }

            if (category == CommandActionCategory.Military)
            {
                result = militaryManager != null && militaryManager.ExecuteMilitaryAction(actionName, target, targetFile, targetRank, color);
                RefreshProfileIfNeeded(result);
                return result;
            }

            result = politicalManager != null && politicalManager.ExecutePoliticalAction(actionName, target, color, value);
            RefreshProfileIfNeeded(result);
            return result;
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
    }
}

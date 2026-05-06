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
            politicsManager = FindObjectOfType<PoliticsManager>();
            militaryManager = FindObjectOfType<MilitaryManager>();
            politicalManager = FindObjectOfType<PoliticalManager>();
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
        }

        private void OnMouseExit()
        {
            isHovered = false;
            visualFeedback.SetHovered(false);
        }

        private void OnMouseDown()
        {
            SetPressed(true);
            Execute();
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

            SetPressed(false);
        }

        public bool Execute()
        {
            if (string.IsNullOrEmpty(actionName))
            {
                return false;
            }

            if (category == CommandActionCategory.Fiscal)
            {
                return politicsManager != null && politicsManager.ExecuteFiscalAction(actionName, selectedPiece, inputValue);
            }

            if (category == CommandActionCategory.Military)
            {
                return militaryManager != null && militaryManager.ExecuteMilitaryAction(actionName, selectedPiece, targetFile, targetRank, actorColor);
            }

            return politicalManager != null && politicalManager.ExecutePoliticalAction(actionName, selectedPiece, actorColor, inputValue);
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
            }

            if (containsPointer && mouse.leftButton.wasPressedThisFrame)
            {
                SetPressed(true);
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
    }
}

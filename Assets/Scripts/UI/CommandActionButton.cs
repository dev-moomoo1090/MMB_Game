using UnityEngine;

namespace MMBGame
{
    public enum CommandActionCategory
    {
        Fiscal,
        Military,
        Political
    }

    [RequireComponent(typeof(BoxCollider2D))]
    public partial class CommandActionButton : MonoBehaviour
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
            politicsManager = SceneComponentResolver.Resolve<PoliticsManager>();
            militaryManager = SceneComponentResolver.Resolve<MilitaryManager>();
            politicalManager = SceneComponentResolver.Resolve<PoliticalManager>();
            turnManager = SceneComponentResolver.Resolve<TurnManager>();
            boardInteraction = SceneComponentResolver.Resolve<BoardInteraction>();
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
    }
}

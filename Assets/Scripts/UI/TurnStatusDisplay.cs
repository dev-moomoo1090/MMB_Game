using UnityEngine;
using UnityEngine.InputSystem;

namespace MMBGame
{
    public partial class TurnStatusDisplay : MonoBehaviour
    {
        private const int BACKGROUND_SORTING_ORDER = 260;
        private const int TEXT_SORTING_ORDER = 270;
        private const int TOOLTIP_BACKGROUND_SORTING_ORDER = 280;
        private const int TOOLTIP_TEXT_SORTING_ORDER = 290;

        [SerializeField] private Vector2 viewportPosition = new Vector2(0.03f, 0.06f);
        [SerializeField] private Vector2 padding = new Vector2(0.18f, 0.1f);
        [SerializeField] private float depthFromCamera = 10f;
        [SerializeField] private float characterSize = 0.13f;
        [SerializeField] private Vector2 backgroundSize = new Vector2(1.65f, 0.42f);
        [SerializeField] private Vector2 turnNumberOffset = new Vector2(1.36f, 0.08f);
        [SerializeField] private Vector2 iconSize = new Vector2(0.42f, 0.42f);
        [SerializeField] private Vector2 iconOffset = new Vector2(0f, 0.56f);
        [SerializeField] private Vector2 tooltipSize = new Vector2(4.25f, 1.55f);
        [SerializeField] private Vector2 tooltipOffset = new Vector2(2.35f, 0.58f);

        private TextMesh label;
        private TextMesh turnNumberLabel;
        private TextMesh iconLabel;
        private TextMesh tooltipLabel;
        private SpriteRenderer background;
        private SpriteRenderer iconBackground;
        private SpriteRenderer tooltipBackground;
        private BoxCollider2D iconCollider;
        private GameObject tooltipRoot;
        private BoardManager boardManager;
        private PoliticsManager politicsManager;
        private TurnManager turnManager;
        private PieceColor currentColor = PieceColor.White;
        private GamePhase currentPhase = GamePhase.PoliticsPhase;
        private KingState currentState = KingState.Neutral;
        private int currentTurnNumber = 1;

        private void Awake()
        {
            EnsureVisuals();
            SyncTurnManagerState();
            FindInitialTurnState();
            UpdateLabel();
        }

        private void OnEnable()
        {
            EventBus.Instance.OnTurnChanged += HandleTurnChanged;
            EventBus.Instance.OnPhaseChanged += HandlePhaseChanged;
            UpdateLabel();
        }

        private void OnDisable()
        {
            EventBus.Instance.OnTurnChanged -= HandleTurnChanged;
            EventBus.Instance.OnPhaseChanged -= HandlePhaseChanged;
            HideTooltip();
        }

        private void LateUpdate()
        {
            AnchorToCamera();
            UpdateHoverState();
        }

        private void HandleTurnChanged(PieceColor color)
        {
            currentColor = color;
            UpdateLabel();
        }

        private void HandlePhaseChanged(GamePhase phase)
        {
            currentPhase = phase;
            UpdateLabel();
        }

        private void FindInitialTurnState()
        {
            turnManager = SceneComponentResolver.Resolve<TurnManager>();
            if (turnManager == null)
            {
                return;
            }

            if (turnManager.CurrentColor != PieceColor.None)
            {
                currentColor = turnManager.CurrentColor;
            }

            currentPhase = turnManager.CurrentPhase;
            currentTurnNumber = turnManager.TurnNumber;
        }

        private void UpdateLabel()
        {
            EnsureVisuals();
            SyncTurnManagerState();
            currentState = ResolveCurrentState();
            label.characterSize = characterSize;
            turnNumberLabel.characterSize = characterSize;
            label.text = GetColorText(currentColor) + " " + GetPhaseText(currentPhase) + " 턴";
            turnNumberLabel.text = currentTurnNumber.ToString();
            iconLabel.text = GetKingStateIconText(currentState);
            iconBackground.color = GetKingStateColor(currentState);
            if (tooltipRoot != null && tooltipRoot.activeSelf)
            {
                tooltipLabel.text = GetKingStateTooltipText(currentState);
            }
        }

        private void AnchorToCamera()
        {
            Camera mainCamera = Camera.main;
            if (mainCamera == null)
            {
                return;
            }

            Vector3 position = mainCamera.ViewportToWorldPoint(new Vector3(viewportPosition.x, viewportPosition.y, depthFromCamera));
            transform.position = position;
            transform.rotation = Quaternion.identity;
        }

        private void EnsureVisuals()
        {
            if (background == null)
            {
                CreateBackground();
            }

            if (label == null)
            {
                CreateLabel();
            }

            if (turnNumberLabel == null)
            {
                CreateTurnNumberLabel();
            }

            if (iconBackground == null || iconLabel == null || iconCollider == null)
            {
                CreateIcon();
            }

            if (tooltipRoot == null)
            {
                CreateTooltip();
            }
        }

        private void UpdateHoverState()
        {
            if (iconCollider == null || tooltipRoot == null || Camera.main == null)
            {
                return;
            }

            Mouse mouse = Mouse.current;
            if (mouse == null)
            {
                HideTooltip();
                return;
            }

            Vector2 screenPosition = mouse.position.ReadValue();
            Vector3 worldPosition = Camera.main.ScreenToWorldPoint(new Vector3(screenPosition.x, screenPosition.y, -Camera.main.transform.position.z));
            if (iconCollider.OverlapPoint(worldPosition))
            {
                ShowTooltip();
            }
            else
            {
                HideTooltip();
            }
        }

        private void ShowTooltip()
        {
            currentState = ResolveCurrentState();
            label.characterSize = characterSize;
            turnNumberLabel.characterSize = characterSize;
            tooltipLabel.text = GetKingStateTooltipText(currentState);
            tooltipRoot.SetActive(true);
        }

        private void HideTooltip()
        {
            if (tooltipRoot != null)
            {
                tooltipRoot.SetActive(false);
            }
        }

        private KingState ResolveCurrentState()
        {
            EnsureManagers();
            PlayerState player = politicsManager != null ? politicsManager.GetCurrentPlayer(currentColor) : null;
            BoardState board = boardManager != null ? boardManager.BoardState : null;
            if (player == null || board == null)
            {
                return KingStateEvaluator.GetCurrentState(currentColor);
            }

            KingState state = KingStateEvaluator.Evaluate(player, board);
            KingStateEvaluator.SetCurrentState(currentColor, state);
            return state;
        }

        private void EnsureManagers()
        {
            boardManager = SceneComponentResolver.Resolve(boardManager);
            politicsManager = SceneComponentResolver.Resolve(politicsManager);
            turnManager = SceneComponentResolver.Resolve(turnManager);
        }

        private void SyncTurnManagerState()
        {
            EnsureManagers();
            if (turnManager == null)
            {
                return;
            }

            if (turnManager.CurrentColor != PieceColor.None)
            {
                currentColor = turnManager.CurrentColor;
            }

            currentPhase = turnManager.CurrentPhase;
            currentTurnNumber = turnManager.TurnNumber;
        }

    }
}

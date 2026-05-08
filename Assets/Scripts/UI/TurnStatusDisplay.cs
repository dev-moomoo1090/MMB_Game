using UnityEngine;
using UnityEngine.InputSystem;

namespace MMBGame
{
    public class TurnStatusDisplay : MonoBehaviour
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
            turnManager = FindFirstObjectByType<TurnManager>();
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
            label.text = GetColorText(currentColor) + " " + GetPhaseText(currentPhase) + " 턴";
            turnNumberLabel.text = currentTurnNumber.ToString();
            iconLabel.text = GetKingStateIconText(currentState);
            iconBackground.color = GetKingStateColor(currentState);
            if (tooltipRoot != null && tooltipRoot.activeSelf)
            {
                tooltipLabel.text = GetKingStateTooltipText(currentState);
            }
        }

        private string GetColorText(PieceColor color)
        {
            return color == PieceColor.Black ? "흑" : "백";
        }

        private string GetPhaseText(GamePhase phase)
        {
            if (phase == GamePhase.ChessPhase)
            {
                return "체스";
            }

            if (phase == GamePhase.PrisonerPhase)
            {
                return "포로";
            }

            return "정치";
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

        private void CreateBackground()
        {
            GameObject backgroundObject = new GameObject("Background");
            backgroundObject.transform.SetParent(transform, false);
            backgroundObject.transform.localPosition = new Vector3(backgroundSize.x * 0.5f - padding.x, 0f, 0.02f);
            background = backgroundObject.AddComponent<SpriteRenderer>();
            background.sprite = CreateBackgroundSprite();
            background.color = new Color(0.08f, 0.08f, 0.08f, 0.72f);
            background.sortingOrder = BACKGROUND_SORTING_ORDER;
            backgroundObject.transform.localScale = new Vector3(backgroundSize.x, backgroundSize.y, 1f);
        }

        private void CreateLabel()
        {
            GameObject labelObject = new GameObject("Label");
            labelObject.transform.SetParent(transform, false);
            labelObject.transform.localPosition = new Vector3(0f, 0.08f, 0f);
            label = labelObject.AddComponent<TextMesh>();
            label.anchor = TextAnchor.MiddleLeft;
            label.alignment = TextAlignment.Left;
            label.fontSize = 48;
            label.characterSize = characterSize;
            label.color = Color.white;
            TextMeshFontApplier.Apply(label);
            MeshRenderer labelRenderer = label.GetComponent<MeshRenderer>();
            if (labelRenderer != null)
            {
                labelRenderer.sortingOrder = TEXT_SORTING_ORDER;
            }
        }

        private void CreateTurnNumberLabel()
        {
            GameObject labelObject = new GameObject("TurnNumberLabel");
            labelObject.transform.SetParent(transform, false);
            labelObject.transform.localPosition = new Vector3(turnNumberOffset.x, turnNumberOffset.y, -0.01f);
            turnNumberLabel = labelObject.AddComponent<TextMesh>();
            turnNumberLabel.anchor = TextAnchor.MiddleCenter;
            turnNumberLabel.alignment = TextAlignment.Center;
            turnNumberLabel.fontSize = 48;
            turnNumberLabel.characterSize = characterSize;
            turnNumberLabel.color = Color.white;
            TextMeshFontApplier.Apply(turnNumberLabel);
            MeshRenderer labelRenderer = turnNumberLabel.GetComponent<MeshRenderer>();
            if (labelRenderer != null)
            {
                labelRenderer.sortingOrder = TEXT_SORTING_ORDER;
            }
        }

        private void CreateIcon()
        {
            GameObject iconObject = new GameObject("KingStateIcon");
            iconObject.transform.SetParent(transform, false);
            iconObject.transform.localPosition = new Vector3(iconOffset.x, iconOffset.y, 0f);
            iconBackground = iconObject.AddComponent<SpriteRenderer>();
            iconBackground.sprite = CreateBackgroundSprite();
            iconBackground.color = GetKingStateColor(currentState);
            iconBackground.sortingOrder = BACKGROUND_SORTING_ORDER;
            iconObject.transform.localScale = new Vector3(iconSize.x, iconSize.y, 1f);
            iconCollider = iconObject.AddComponent<BoxCollider2D>();
            iconCollider.size = Vector2.one;
            iconCollider.isTrigger = true;

            GameObject iconLabelObject = new GameObject("IconLabel");
            iconLabelObject.transform.SetParent(iconObject.transform, false);
            iconLabelObject.transform.localPosition = new Vector3(0f, 0.1f, -0.01f);
            iconLabel = iconLabelObject.AddComponent<TextMesh>();
            iconLabel.anchor = TextAnchor.MiddleCenter;
            iconLabel.alignment = TextAlignment.Center;
            iconLabel.fontSize = 42;
            iconLabel.characterSize = 0.12f;
            iconLabel.color = Color.white;
            TextMeshFontApplier.Apply(iconLabel);
            MeshRenderer iconRenderer = iconLabel.GetComponent<MeshRenderer>();
            if (iconRenderer != null)
            {
                iconRenderer.sortingOrder = TEXT_SORTING_ORDER;
            }
        }

        private void CreateTooltip()
        {
            tooltipRoot = new GameObject("KingStateTooltip");
            tooltipRoot.transform.SetParent(transform, false);
            tooltipRoot.transform.localPosition = new Vector3(tooltipOffset.x, tooltipOffset.y, -0.1f);

            GameObject tooltipBackgroundObject = new GameObject("Background");
            tooltipBackgroundObject.transform.SetParent(tooltipRoot.transform, false);
            tooltipBackground = tooltipBackgroundObject.AddComponent<SpriteRenderer>();
            tooltipBackground.sprite = CreateBackgroundSprite();
            tooltipBackground.color = new Color(0.12f, 0.12f, 0.12f, 0.82f);
            tooltipBackground.sortingOrder = TOOLTIP_BACKGROUND_SORTING_ORDER;
            tooltipBackgroundObject.transform.localScale = new Vector3(tooltipSize.x, tooltipSize.y, 1f);

            GameObject tooltipLabelObject = new GameObject("Label");
            tooltipLabelObject.transform.SetParent(tooltipRoot.transform, false);
            tooltipLabelObject.transform.localPosition = new Vector3(-tooltipSize.x * 0.5f + 0.22f, tooltipSize.y * 0.5f - 0.22f, -0.01f);
            tooltipLabel = tooltipLabelObject.AddComponent<TextMesh>();
            tooltipLabel.anchor = TextAnchor.UpperLeft;
            tooltipLabel.alignment = TextAlignment.Left;
            tooltipLabel.fontSize = 42;
            tooltipLabel.characterSize = 0.073f;
            tooltipLabel.lineSpacing = 1f;
            tooltipLabel.color = Color.white;
            TextMeshFontApplier.Apply(tooltipLabel);
            MeshRenderer tooltipRenderer = tooltipLabel.GetComponent<MeshRenderer>();
            if (tooltipRenderer != null)
            {
                tooltipRenderer.sortingOrder = TOOLTIP_TEXT_SORTING_ORDER;
            }

            tooltipRoot.SetActive(false);
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
            if (boardManager == null)
            {
                boardManager = FindFirstObjectByType<BoardManager>();
            }

            if (politicsManager == null)
            {
                politicsManager = FindFirstObjectByType<PoliticsManager>();
            }

            if (turnManager == null)
            {
                turnManager = FindFirstObjectByType<TurnManager>();
            }
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

        private string GetKingStateIconText(KingState state)
        {
            switch (state)
            {
                case KingState.Sage: return "성";
                case KingState.Autocrat: return "독";
                case KingState.DarkKing: return "암";
                case KingState.Tyrant: return "폭";
                default: return "중";
            }
        }

        private Color GetKingStateColor(KingState state)
        {
            switch (state)
            {
                case KingState.Sage: return new Color(0.15f, 0.48f, 0.82f, 0.86f);
                case KingState.Autocrat: return new Color(0.38f, 0.28f, 0.58f, 0.86f);
                case KingState.DarkKing: return new Color(0.5f, 0.5f, 0.18f, 0.86f);
                case KingState.Tyrant: return new Color(0.66f, 0.12f, 0.1f, 0.86f);
                default: return new Color(0.24f, 0.24f, 0.24f, 0.86f);
            }
        }

        private string GetKingStateTooltipText(KingState state)
        {
            if (state == KingState.DarkKing)
            {
                return GetDarkKingTooltipText();
            }

            switch (state)
            {
                case KingState.Sage:
                    return "성군\n정치 행동 횟수 +1\n매 턴 모든 기물 지지도 +2";
                case KingState.Autocrat:
                    return "독재\n명령 거부 확률 0%\n군사 행동 비용 -30%";
                case KingState.DarkKing:
                    return "암군\n최초 진입 시 세금/수락/지지도 -50%\n20턴 생존 시 성군 고정";
                case KingState.Tyrant:
                    return "폭군\n처형 행동 턴 소모 0\n처형 시 특수 폰 소환\n세금 수입 +100%";
                default:
                    return "중립\n적용 중인 특수 효과 없음";
            }
        }

        private string GetDarkKingTooltipText()
        {
            KingStateEffectApplier applier = KingStateEffectApplier.Instance;
            int elapsed = applier != null ? applier.GetIncompetentTurnsElapsed(currentColor) : 0;
            int remaining = applier != null ? applier.GetIncompetentTurnsRemaining(currentColor) : 20;
            int total = applier != null ? applier.GetIncompetentSurvivalTurns() : 20;
            return "암군\n현재 " + elapsed + "/" + total + "턴 진행\n성군 고정까지 " + remaining + "턴 남음";
        }

        private Sprite CreateBackgroundSprite()
        {
            Texture2D texture = new Texture2D(1, 1);
            texture.SetPixel(0, 0, Color.white);
            texture.Apply();
            return Sprite.Create(texture, new Rect(0f, 0f, 1f, 1f), new Vector2(0.5f, 0.5f), 1f);
        }
    }
}

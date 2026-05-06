using UnityEngine;

namespace MMBGame
{
    public class TurnStatusDisplay : MonoBehaviour
    {
        private const int BACKGROUND_SORTING_ORDER = 260;
        private const int TEXT_SORTING_ORDER = 270;

        [SerializeField] private Vector2 viewportPosition = new Vector2(0.03f, 0.06f);
        [SerializeField] private Vector2 padding = new Vector2(0.18f, 0.1f);
        [SerializeField] private float depthFromCamera = 10f;
        [SerializeField] private float characterSize = 0.13f;
        [SerializeField] private Vector2 backgroundSize = new Vector2(1.65f, 0.42f);

        private TextMesh label;
        private SpriteRenderer background;
        private PieceColor currentColor = PieceColor.White;
        private GamePhase currentPhase = GamePhase.PoliticsPhase;

        private void Awake()
        {
            EnsureVisuals();
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
        }

        private void LateUpdate()
        {
            AnchorToCamera();
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
            TurnManager turnManager = FindFirstObjectByType<TurnManager>();
            if (turnManager == null)
            {
                return;
            }

            if (turnManager.CurrentColor != PieceColor.None)
            {
                currentColor = turnManager.CurrentColor;
            }

            currentPhase = turnManager.CurrentPhase;
        }

        private void UpdateLabel()
        {
            EnsureVisuals();
            label.text = GetColorText(currentColor) + " " + GetPhaseText(currentPhase) + " 턴";
        }

        private string GetColorText(PieceColor color)
        {
            if (color == PieceColor.Black)
            {
                return "흑";
            }

            return "백";
        }

        private string GetPhaseText(GamePhase phase)
        {
            if (phase == GamePhase.ChessPhase)
            {
                return "체스";
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
            MeshRenderer labelRenderer = label.GetComponent<MeshRenderer>();
            if (labelRenderer != null)
            {
                labelRenderer.sortingOrder = TEXT_SORTING_ORDER;
            }
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

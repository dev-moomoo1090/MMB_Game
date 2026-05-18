using UnityEngine;
using UnityEngine.InputSystem;

namespace MMBGame
{
    public class RegimeActionButtons : MonoBehaviour
    {
        private const int BACKGROUND_SORTING_ORDER = 300;
        private const int TEXT_SORTING_ORDER = 310;

        [SerializeField] private Vector2 viewportPosition = new Vector2(0.72f, 0.19f);
        [SerializeField] private float depthFromCamera = 10f;
        [SerializeField] private Vector2 buttonSize = new Vector2(1.05f, 0.34f);
        [SerializeField] private float characterSize = 0.055f;

        private TurnManager turnManager;
        private PoliticalManager politicalManager;
        private GameObject root;
        private RegimeButton skipButton;
        private RegimeButton isolationButton;

        public void Initialize()
        {
            turnManager = SceneComponentResolver.Resolve<TurnManager>();
            politicalManager = SceneComponentResolver.Resolve<PoliticalManager>();
            EnsureVisuals();
        }

        private void Awake()
        {
            EnsureVisuals();
        }

        private void Update()
        {
            EnsureManagers();
            AnchorToCamera();
            UpdateAvailability();
            HandleInput();
        }

        private void EnsureManagers()
        {
            if (turnManager == null)
            {
                turnManager = SceneComponentResolver.Resolve<TurnManager>();
            }

            if (politicalManager == null)
            {
                politicalManager = SceneComponentResolver.Resolve<PoliticalManager>();
            }
        }

        private void HandleInput()
        {
            if (ActionResultPanel.IsOpen || Mouse.current == null || Camera.main == null)
            {
                return;
            }

            Vector2 screenPosition = Mouse.current.position.ReadValue();
            Vector3 worldPosition = Camera.main.ScreenToWorldPoint(new Vector3(screenPosition.x, screenPosition.y, -Camera.main.transform.position.z));
            skipButton.SetHovered(skipButton.Contains(worldPosition));
            isolationButton.SetHovered(isolationButton.Contains(worldPosition));

            if (!Mouse.current.leftButton.wasPressedThisFrame)
            {
                return;
            }

            if (skipButton.IsHovered)
            {
                TrySkipPoliticsAction();
            }
            else if (isolationButton.IsHovered)
            {
                TryIsolation();
            }
        }

        private void TrySkipPoliticsAction()
        {
            PieceColor color = ResolveCurrentColor();
            if (turnManager == null || turnManager.CurrentPhase != GamePhase.PoliticsPhase)
            {
                ShowNotice("정치 턴에서만 사용할 수 있습니다.");
                return;
            }

            if (!KingStateEvaluator.IsBenevolent(color))
            {
                ShowNotice("성군 상태에서만 정치행동 스킵 보너스를 받을 수 있습니다.");
                return;
            }

            if (!turnManager.TryQueuePoliticsSkipBonus(color))
            {
                ShowNotice("이번 턴에는 정치행동 스킵을 사용할 수 없습니다.");
                return;
            }

            EventBus.Instance.PublishActionExecuted(color, "정치행동 스킵");
            ActionResultPanel.Show("행동 결과", "정치행동을 스킵했습니다.\n다음 정치 턴의 정치행동 횟수가 1회 증가합니다.", AdvanceTurn);
        }

        private void TryIsolation()
        {
            PieceColor color = ResolveCurrentColor();
            if (turnManager == null || turnManager.CurrentPhase != GamePhase.PoliticsPhase)
            {
                ShowNotice("정치 턴에서만 사용할 수 있습니다.");
                return;
            }

            if (politicalManager == null)
            {
                ShowNotice("정치 행동 관리자를 찾을 수 없습니다.");
                return;
            }

            if (!KingStateEvaluator.IsDictatorship(color))
            {
                ShowNotice("독재 상태에서만 쇄국을 사용할 수 있습니다.");
                return;
            }

            if (!politicalManager.ExecuteIsolation(color))
            {
                ShowNotice("쇄국은 게임당 한 번만 사용할 수 있으며, 이미 지속 중이면 다시 사용할 수 없습니다.");
                return;
            }

            ActionResultPanel.Show("행동 결과", "쇄국을 선포했습니다.\n3턴 동안 양 플레이어는 상대에게 영향을 주는 정치행동을 사용할 수 없습니다.", AdvanceTurn);
        }

        private void ShowNotice(string text)
        {
            ActionResultPanel.Show("행동 결과", text, null);
        }

        private void AdvanceTurn()
        {
            turnManager?.EndPhase();
        }

        private PieceColor ResolveCurrentColor()
        {
            return turnManager != null ? turnManager.CurrentColor : PieceColor.None;
        }

        private void UpdateAvailability()
        {
            PieceColor color = ResolveCurrentColor();
            bool isPoliticsPhase = turnManager == null || turnManager.CurrentPhase == GamePhase.PoliticsPhase;
            bool canSkip = isPoliticsPhase && turnManager != null && turnManager.CanQueuePoliticsSkipBonus(color);
            bool canIsolation = isPoliticsPhase && politicalManager != null && politicalManager.CanExecuteIsolation(color);
            skipButton.SetEnabled(canSkip);
            isolationButton.SetEnabled(canIsolation);
        }

        private void AnchorToCamera()
        {
            if (root == null || Camera.main == null)
            {
                return;
            }

            root.transform.position = Camera.main.ViewportToWorldPoint(new Vector3(viewportPosition.x, viewportPosition.y, depthFromCamera));
            root.transform.rotation = Quaternion.identity;
        }

        private void EnsureVisuals()
        {
            if (root != null)
            {
                return;
            }

            root = new GameObject("RegimeActionButtonsRoot");
            root.transform.SetParent(transform, false);
            skipButton = CreateButton("SkipPoliticsButton", "스킵", new Vector3(-0.58f, 0f, 0f));
            isolationButton = CreateButton("IsolationButton", "쇄국", new Vector3(0.58f, 0f, 0f));
        }

        private RegimeButton CreateButton(string objectName, string labelText, Vector3 localPosition)
        {
            GameObject buttonObject = new GameObject(objectName);
            buttonObject.transform.SetParent(root.transform, false);
            buttonObject.transform.localPosition = localPosition;

            GameObject backgroundObject = new GameObject("Background");
            backgroundObject.transform.SetParent(buttonObject.transform, false);
            SpriteRenderer background = backgroundObject.AddComponent<SpriteRenderer>();
            background.sprite = RuntimeUiFactory.GetPixelSprite();
            background.sortingOrder = BACKGROUND_SORTING_ORDER;
            backgroundObject.transform.localScale = new Vector3(buttonSize.x, buttonSize.y, 1f);

            BoxCollider2D collider = buttonObject.AddComponent<BoxCollider2D>();
            collider.size = buttonSize;
            buttonObject.AddComponent<RegimeActionButtonHitbox>();

            GameObject labelObject = new GameObject("Label");
            labelObject.transform.SetParent(buttonObject.transform, false);
            labelObject.transform.localPosition = new Vector3(0f, 0.02f, -0.01f);
            TextMesh label = labelObject.AddComponent<TextMesh>();
            label.text = labelText;
            label.anchor = TextAnchor.MiddleCenter;
            label.alignment = TextAlignment.Center;
            label.fontSize = 44;
            label.characterSize = characterSize;
            label.color = Color.white;
            TextMeshFontApplier.Apply(label);

            MeshRenderer labelRenderer = label.GetComponent<MeshRenderer>();
            if (labelRenderer != null)
            {
                labelRenderer.sortingOrder = TEXT_SORTING_ORDER;
            }

            return new RegimeButton(background, collider);
        }

        private class RegimeButton
        {
            private readonly SpriteRenderer background;
            private readonly BoxCollider2D collider;
            private bool isEnabled;
            private bool isHovered;

            public bool IsHovered => isHovered;

            public RegimeButton(SpriteRenderer background, BoxCollider2D collider)
            {
                this.background = background;
                this.collider = collider;
            }

            public void SetEnabled(bool enabled)
            {
                isEnabled = enabled;
                ApplyColor();
            }

            public void SetHovered(bool hovered)
            {
                if (isHovered == hovered)
                {
                    return;
                }

                isHovered = hovered;
                ApplyColor();
            }

            public bool Contains(Vector3 worldPosition)
            {
                return collider != null && collider.OverlapPoint(worldPosition);
            }

            private void ApplyColor()
            {
                if (background == null)
                {
                    return;
                }

                if (!isEnabled)
                {
                    background.color = new Color(0.12f, 0.12f, 0.12f, 0.46f);
                }
                else if (isHovered)
                {
                    background.color = new Color(0.35f, 0.42f, 0.48f, 0.9f);
                }
                else
                {
                    background.color = new Color(0.18f, 0.22f, 0.25f, 0.82f);
                }
            }
        }
    }

    public class RegimeActionButtonHitbox : MonoBehaviour
    {
    }
}

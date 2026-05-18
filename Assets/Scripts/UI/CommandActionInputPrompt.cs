using System;
using UnityEngine;
using UnityEngine.InputSystem;

namespace MMBGame
{
    public class CommandActionInputPrompt : MonoBehaviour
    {
        private const int BACKGROUND_SORTING_ORDER = 280;
        private const int TEXT_SORTING_ORDER = 290;

        private static CommandActionInputPrompt instance;

        [SerializeField] private Vector2 viewportPosition = new Vector2(0.72f, 0.12f);
        [SerializeField] private float depthFromCamera = 10f;
        [SerializeField] private Vector2 backgroundSize = new Vector2(2.8f, 0.68f);
        [SerializeField] private float characterSize = 0.105f;

        private GameObject root;
        private SpriteRenderer background;
        private TextMesh label;
        private Action<int> onSubmitted;
        private string inputText = string.Empty;
        private string currentActionName = string.Empty;

        public static void Show(string actionName, Action<int> submitCallback)
        {
            EnsureInstance();
            instance.ShowInternal(actionName, submitCallback);
        }

        public static void Hide()
        {
            if (instance != null)
            {
                instance.HideInternal();
            }
        }

        private static void EnsureInstance()
        {
            if (instance != null)
            {
                return;
            }

            CommandActionInputPrompt existing = SceneComponentResolver.Resolve<CommandActionInputPrompt>();
            if (existing != null)
            {
                instance = existing;
                return;
            }

            GameObject promptObject = new GameObject("CommandActionInputPrompt");
            instance = promptObject.AddComponent<CommandActionInputPrompt>();
        }

        private void Awake()
        {
            instance = this;
            EnsureVisuals();
            HideInternal();
        }

        private void Update()
        {
            if (root == null || !root.activeSelf)
            {
                return;
            }

            AnchorToCamera();
            HandleKeyboard();
        }

        private void ShowInternal(string actionName, Action<int> submitCallback)
        {
            EnsureVisuals();
            inputText = string.Empty;
            currentActionName = actionName ?? string.Empty;
            onSubmitted = submitCallback;
            UpdateLabel(actionName);
            root.SetActive(true);
            AnchorToCamera();
        }

        private void HideInternal()
        {
            if (root != null)
            {
                root.SetActive(false);
            }

            onSubmitted = null;
            inputText = string.Empty;
        }

        private void HandleKeyboard()
        {
            Keyboard keyboard = Keyboard.current;
            if (keyboard == null)
            {
                return;
            }

            for (int i = 0; i <= 9; i++)
            {
                Key key = (Key)((int)Key.Digit0 + i);
                Key numpadKey = (Key)((int)Key.Numpad0 + i);
                if (keyboard[key].wasPressedThisFrame || keyboard[numpadKey].wasPressedThisFrame)
                {
                    inputText += i.ToString();
                    UpdateLabel(null);
                }
            }

            if (keyboard.backspaceKey.wasPressedThisFrame && inputText.Length > 0)
            {
                inputText = inputText.Substring(0, inputText.Length - 1);
                UpdateLabel(null);
            }

            if (keyboard.escapeKey.wasPressedThisFrame)
            {
                HideInternal();
            }

            if (keyboard.enterKey.wasPressedThisFrame || keyboard.numpadEnterKey.wasPressedThisFrame)
            {
                Submit();
            }
        }

        private void Submit()
        {
            if (!int.TryParse(inputText, out int value) || value <= 0)
            {
                return;
            }

            Action<int> submit = onSubmitted;
            HideInternal();
            submit?.Invoke(value);
        }

        private void UpdateLabel(string actionName)
        {
            string valueText = string.IsNullOrEmpty(inputText) ? "_" : inputText;
            string costHint = GetCostHint(currentActionName, inputText);
            label.text = "입력값 : " + valueText + costHint + "\nEnter로 실행 / Esc 취소";
        }

        private static string GetCostHint(string actionName, string inputText)
        {
            int multiplier = GetCostMultiplier(actionName);
            if (multiplier <= 1)
            {
                return string.Empty;
            }

            if (int.TryParse(inputText, out int value) && value > 0)
            {
                return "  (비용: " + (value * multiplier) + " 골드)";
            }

            return "  (비용: 입력값 x" + multiplier + " 골드)";
        }

        private static int GetCostMultiplier(string actionName)
        {
            switch (actionName)
            {
                case "제후국":
                case "매수":
                    return 3;
                default:
                    return 1;
            }
        }

        private void AnchorToCamera()
        {
            Camera mainCamera = Camera.main;
            if (mainCamera == null)
            {
                return;
            }

            root.transform.position = mainCamera.ViewportToWorldPoint(new Vector3(viewportPosition.x, viewportPosition.y, depthFromCamera));
            root.transform.rotation = Quaternion.identity;
        }

        private void EnsureVisuals()
        {
            if (root != null)
            {
                return;
            }

            root = new GameObject("InputPromptRoot");
            root.transform.SetParent(transform, false);

            GameObject backgroundObject = new GameObject("Background");
            backgroundObject.transform.SetParent(root.transform, false);
            background = backgroundObject.AddComponent<SpriteRenderer>();
            background.sprite = RuntimeUiFactory.GetPixelSprite();
            background.color = new Color(0.08f, 0.08f, 0.08f, 0.82f);
            background.sortingOrder = BACKGROUND_SORTING_ORDER;
            backgroundObject.transform.localScale = new Vector3(backgroundSize.x, backgroundSize.y, 1f);

            GameObject labelObject = new GameObject("Label");
            labelObject.transform.SetParent(root.transform, false);
            labelObject.transform.localPosition = new Vector3(-backgroundSize.x * 0.45f, backgroundSize.y * 0.18f, -0.01f);
            label = labelObject.AddComponent<TextMesh>();
            label.anchor = TextAnchor.MiddleLeft;
            label.alignment = TextAlignment.Left;
            label.fontSize = 44;
            label.characterSize = characterSize;
            label.color = Color.white;
            TextMeshFontApplier.Apply(label);
            MeshRenderer labelRenderer = label.GetComponent<MeshRenderer>();
            if (labelRenderer != null)
            {
                labelRenderer.sortingOrder = TEXT_SORTING_ORDER;
            }
        }

    }
}

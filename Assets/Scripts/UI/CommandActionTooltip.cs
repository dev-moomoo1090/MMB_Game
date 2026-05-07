using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

namespace MMBGame
{
    public class CommandActionTooltip : MonoBehaviour
    {
        private const int BACKGROUND_SORTING_ORDER = 240;
        private const int TEXT_SORTING_ORDER = 250;

        private static CommandActionTooltip instance;
        [SerializeField] private Vector3 offset = new Vector3(2.3f, 0.45f, -0.15f);
        [SerializeField] private Vector2 padding = new Vector2(0.32f, 0.3f);
        [SerializeField] private float width = 8.4f;
        [SerializeField] private float height = 1.8f;
        [SerializeField] private float lineHeight = 0.25f;
        [SerializeField] private float characterSize = 0.087f;
        [SerializeField] private int maxCharactersPerLine = 36;

        private readonly List<string> contentLines = new List<string>();
        private GameObject root;
        private SpriteRenderer background;
        private TextMesh label;
        private BoxCollider2D hoverCollider;
        private BoxCollider2D anchorCollider;
        private int scrollIndex;
        private int visibleLineCount;

        public static void Show(string actionName, Transform anchor)
        {
            EnsureInstance();
            instance.ShowInternal(actionName, anchor);
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

            CommandActionTooltip existing = FindFirstObjectByType<CommandActionTooltip>();
            if (existing != null)
            {
                instance = existing;
                return;
            }

            GameObject tooltipObject = new GameObject("CommandActionTooltip");
            instance = tooltipObject.AddComponent<CommandActionTooltip>();
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

            UpdateScroll();
            if (!IsPointerInsideTooltip() && !IsPointerInsideAnchor())
            {
                HideInternal();
            }
        }

        public static bool IsPointerInside()
        {
            return instance != null && instance.IsPointerInsideTooltip();
        }

        private void ShowInternal(string actionName, Transform anchor)
        {
            if (anchor == null)
            {
                HideInternal();
                return;
            }

            EnsureVisuals();
            anchorCollider = anchor.GetComponent<BoxCollider2D>();
            contentLines.Clear();
            BuildWrappedLines(CommandActionTooltipDescriptions.Resolve(actionName));
            scrollIndex = 0;
            visibleLineCount = Mathf.Max(1, Mathf.FloorToInt((height - padding.y * 2f) / lineHeight));
            root.transform.position = anchor.position + offset;
            root.transform.localScale = Vector3.one;
            background.transform.localScale = new Vector3(width, height, 1f);
            hoverCollider.size = new Vector2(width, height);
            hoverCollider.offset = Vector2.zero;
            label.transform.localPosition = new Vector3(-width * 0.5f + padding.x, height * 0.5f - padding.y, -0.01f);
            UpdateVisibleText();
            root.SetActive(true);
        }

        private void HideInternal()
        {
            if (root != null)
            {
                root.SetActive(false);
            }

            anchorCollider = null;
        }

        private void EnsureVisuals()
        {
            if (root != null)
            {
                return;
            }

            root = new GameObject("TooltipRoot");
            root.transform.SetParent(transform, false);

            GameObject backgroundObject = new GameObject("Background");
            backgroundObject.transform.SetParent(root.transform, false);
            background = backgroundObject.AddComponent<SpriteRenderer>();
            background.sprite = CreateBackgroundSprite();
            background.color = new Color(0.16f, 0.16f, 0.16f, 0.78f);
            background.sortingOrder = BACKGROUND_SORTING_ORDER;
            hoverCollider = root.AddComponent<BoxCollider2D>();
            hoverCollider.isTrigger = true;

            GameObject labelObject = new GameObject("Label");
            labelObject.transform.SetParent(root.transform, false);
            label = labelObject.AddComponent<TextMesh>();
            label.anchor = TextAnchor.UpperLeft;
            label.alignment = TextAlignment.Left;
            label.fontSize = 44;
            label.characterSize = characterSize;
            label.lineSpacing = 1f;
            label.color = Color.white;
            TextMeshFontApplier.Apply(label);
            MeshRenderer labelRenderer = label.GetComponent<MeshRenderer>();
            if (labelRenderer != null)
            {
                labelRenderer.sortingOrder = TEXT_SORTING_ORDER;
            }
        }

        private void BuildWrappedLines(string text)
        {
            string[] rawLines = text.Split('\n');
            for (int i = 0; i < rawLines.Length; i++)
            {
                AddWrappedLine(rawLines[i]);
            }
        }

        private void AddWrappedLine(string line)
        {
            if (string.IsNullOrEmpty(line))
            {
                contentLines.Add(string.Empty);
                return;
            }

            int start = 0;
            while (start < line.Length)
            {
                int length = Mathf.Min(maxCharactersPerLine, line.Length - start);
                contentLines.Add(line.Substring(start, length));
                start += length;
            }
        }

        private void UpdateScroll()
        {
            if (!IsPointerInsideTooltip())
            {
                return;
            }

            Mouse mouse = Mouse.current;
            if (mouse == null)
            {
                return;
            }

            float scrollY = mouse.scroll.ReadValue().y;
            if (Mathf.Abs(scrollY) < 0.01f)
            {
                return;
            }

            int maxScroll = Mathf.Max(0, contentLines.Count - visibleLineCount);
            int direction = scrollY > 0f ? -1 : 1;
            int nextIndex = Mathf.Clamp(scrollIndex + direction, 0, maxScroll);
            if (nextIndex == scrollIndex)
            {
                return;
            }

            scrollIndex = nextIndex;
            UpdateVisibleText();
        }

        private void UpdateVisibleText()
        {
            int lineCount = Mathf.Min(visibleLineCount, contentLines.Count - scrollIndex);
            System.Text.StringBuilder builder = new System.Text.StringBuilder();
            for (int i = 0; i < lineCount; i++)
            {
                if (i > 0)
                {
                    builder.Append('\n');
                }

                builder.Append(contentLines[scrollIndex + i]);
            }

            if (contentLines.Count > visibleLineCount)
            {
                builder.Append('\n');
                builder.Append(scrollIndex < contentLines.Count - visibleLineCount ? "▼" : "▲");
            }

            label.text = builder.ToString();
        }

        private bool IsPointerInsideTooltip()
        {
            if (hoverCollider == null || !root.activeSelf || Camera.main == null)
            {
                return false;
            }

            Mouse mouse = Mouse.current;
            if (mouse == null)
            {
                return false;
            }

            Vector2 screenPosition = mouse.position.ReadValue();
            Vector3 worldPosition = Camera.main.ScreenToWorldPoint(new Vector3(screenPosition.x, screenPosition.y, -Camera.main.transform.position.z));
            return hoverCollider.OverlapPoint(worldPosition);
        }

        private bool IsPointerInsideAnchor()
        {
            if (anchorCollider == null || Camera.main == null)
            {
                return false;
            }

            Mouse mouse = Mouse.current;
            if (mouse == null)
            {
                return false;
            }

            Vector2 screenPosition = mouse.position.ReadValue();
            Vector3 worldPosition = Camera.main.ScreenToWorldPoint(new Vector3(screenPosition.x, screenPosition.y, -Camera.main.transform.position.z));
            return anchorCollider.OverlapPoint(worldPosition);
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

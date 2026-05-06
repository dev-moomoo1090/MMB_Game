using UnityEngine;

namespace MMBGame
{
    [RequireComponent(typeof(BoxCollider2D))]
    public class CommandActionButtonVisualFeedback : MonoBehaviour
    {
        private static readonly Color IMPERIAL_RED = new Color(0.929f, 0.161f, 0.224f, 1f);

        [SerializeField] private Vector2 textColliderPadding = new Vector2(0.08f, 0.04f);
        [SerializeField] private float textOutlineOffset = 0.012f;

        private BoxCollider2D boxCollider;
        private TextMesh label;
        private TextMesh[] outlineLabels;
        private bool isPressed;

        public BoxCollider2D Collider => boxCollider;

        private void Awake()
        {
            Initialize();
        }

        private void OnDisable()
        {
            SetHovered(false);
            SetPressed(false);
        }

        public void Initialize()
        {
            boxCollider = GetComponent<BoxCollider2D>();
            label = GetComponent<TextMesh>();
            if (label == null)
            {
                label = GetComponentInChildren<TextMesh>();
            }

            FitColliderToText();
            CreateTextOutline();
            SetHovered(false);
        }

        public void SetHovered(bool isHovered)
        {
            if (outlineLabels == null)
            {
                return;
            }

            for (int i = 0; i < outlineLabels.Length; i++)
            {
                if (outlineLabels[i] != null)
                {
                    outlineLabels[i].gameObject.SetActive(isHovered);
                }
            }
        }

        public void SetPressed(bool pressed)
        {
            if (isPressed == pressed)
            {
                return;
            }

            isPressed = pressed;
            SetLabelColor(pressed ? IMPERIAL_RED : Color.white);
        }

        private void CreateTextOutline()
        {
            if (label == null)
            {
                return;
            }

            Transform existing = transform.Find("TextHoverOutline");
            GameObject outlineRoot = existing == null ? new GameObject("TextHoverOutline") : existing.gameObject;
            outlineRoot.transform.SetParent(transform, false);
            Vector3[] offsets = CreateOutlineOffsets();
            outlineLabels = new TextMesh[offsets.Length];
            for (int i = 0; i < offsets.Length; i++)
            {
                outlineLabels[i] = CreateOutlineLabel(outlineRoot.transform, offsets[i], i);
            }
        }

        private Vector3[] CreateOutlineOffsets()
        {
            return new Vector3[]
            {
                new Vector3(-textOutlineOffset, 0f, 0f),
                new Vector3(textOutlineOffset, 0f, 0f),
                new Vector3(0f, -textOutlineOffset, 0f),
                new Vector3(0f, textOutlineOffset, 0f),
                new Vector3(-textOutlineOffset, -textOutlineOffset, 0f),
                new Vector3(-textOutlineOffset, textOutlineOffset, 0f),
                new Vector3(textOutlineOffset, -textOutlineOffset, 0f),
                new Vector3(textOutlineOffset, textOutlineOffset, 0f)
            };
        }

        private TextMesh CreateOutlineLabel(Transform outlineRoot, Vector3 offset, int index)
        {
            Transform child = outlineRoot.Find("Outline_" + index);
            GameObject outlineObject = child == null ? new GameObject("Outline_" + index) : child.gameObject;
            outlineObject.transform.SetParent(outlineRoot, false);
            outlineObject.transform.localPosition = offset;
            outlineObject.transform.localRotation = Quaternion.identity;
            outlineObject.transform.localScale = Vector3.one;
            TextMesh outline = outlineObject.GetComponent<TextMesh>();
            if (outline == null)
            {
                outline = outlineObject.AddComponent<TextMesh>();
            }

            CopyLabelStyle(outline);
            return outline;
        }

        private void FitColliderToText()
        {
            if (boxCollider == null || label == null)
            {
                return;
            }

            MeshRenderer textRenderer = label.GetComponent<MeshRenderer>();
            if (textRenderer == null)
            {
                return;
            }

            Bounds localBounds = TransformWorldBoundsToLocal(textRenderer.bounds);
            boxCollider.offset = new Vector2(localBounds.center.x, localBounds.center.y);
            boxCollider.size = new Vector2(
                Mathf.Max(0.01f, localBounds.size.x + textColliderPadding.x),
                Mathf.Max(0.01f, localBounds.size.y + textColliderPadding.y)
            );
        }

        private Bounds TransformWorldBoundsToLocal(Bounds worldBounds)
        {
            Vector3 min = transform.InverseTransformPoint(worldBounds.min);
            Vector3 max = transform.InverseTransformPoint(worldBounds.max);
            Bounds localBounds = new Bounds();
            localBounds.SetMinMax(Vector3.Min(min, max), Vector3.Max(min, max));
            return localBounds;
        }

        private void CopyLabelStyle(TextMesh outline)
        {
            outline.text = label.text;
            outline.characterSize = label.characterSize;
            outline.lineSpacing = label.lineSpacing;
            outline.anchor = label.anchor;
            outline.alignment = label.alignment;
            outline.tabSize = label.tabSize;
            outline.fontSize = label.fontSize;
            outline.fontStyle = label.fontStyle;
            outline.richText = label.richText;
            outline.font = label.font;
            outline.color = IMPERIAL_RED;
            CopyRendererStyle(outline);
        }

        private void CopyRendererStyle(TextMesh outline)
        {
            MeshRenderer sourceRenderer = label.GetComponent<MeshRenderer>();
            MeshRenderer outlineRenderer = outline.GetComponent<MeshRenderer>();
            if (sourceRenderer == null || outlineRenderer == null)
            {
                return;
            }

            outlineRenderer.sharedMaterial = sourceRenderer.sharedMaterial;
            outlineRenderer.sortingLayerID = sourceRenderer.sortingLayerID;
            outlineRenderer.sortingOrder = sourceRenderer.sortingOrder - 1;
        }

        private void SetLabelColor(Color color)
        {
            if (label == null)
            {
                return;
            }

            label.color = color;
            MeshRenderer labelRenderer = label.GetComponent<MeshRenderer>();
            if (labelRenderer != null)
            {
                labelRenderer.material.color = color;
            }
        }
    }
}

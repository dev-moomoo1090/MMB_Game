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
        private static readonly Color IMPERIAL_RED = new Color(0.929f, 0.161f, 0.224f, 1f);

        [SerializeField] private CommandActionCategory category;
        [SerializeField] private string actionName;
        [SerializeField] private int inputValue;
        [SerializeField] private PieceColor actorColor = PieceColor.White;
        [SerializeField] private ChessPiece selectedPiece;
        [SerializeField] private int targetFile = -1;
        [SerializeField] private int targetRank = -1;
        [SerializeField] private Vector2 textColliderPadding = new Vector2(0.08f, 0.04f);
        [SerializeField] private float textOutlineOffset = 0.012f;

        private PoliticsManager politicsManager;
        private MilitaryManager militaryManager;
        private PoliticalManager politicalManager;
        private BoxCollider2D boxCollider;
        private TextMesh label;
        private TextMesh[] outlineLabels;
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
            boxCollider = GetComponent<BoxCollider2D>();
            label = GetComponent<TextMesh>();
            if (label == null)
            {
                label = GetComponentInChildren<TextMesh>();
            }

            FitColliderToText();
            CreateTextOutline();
            SetTextOutlineVisible(false);
        }

        private void Update()
        {
            UpdatePointerState();
        }

        private void OnMouseEnter()
        {
            isHovered = true;
            SetTextOutlineVisible(true);
        }

        private void OnMouseExit()
        {
            isHovered = false;
            SetTextOutlineVisible(false);
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
            SetTextOutlineVisible(false);
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
            if (boxCollider == null || Camera.main == null)
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
            bool containsPointer = boxCollider.OverlapPoint(worldPosition);

            if (containsPointer != isHovered)
            {
                isHovered = containsPointer;
                SetTextOutlineVisible(isHovered);
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

        private void CreateTextOutline()
        {
            if (label == null)
            {
                return;
            }

            Transform existing = transform.Find("TextHoverOutline");
            GameObject outlineRoot;
            if (existing == null)
            {
                outlineRoot = new GameObject("TextHoverOutline");
                outlineRoot.transform.SetParent(transform, false);
            }
            else
            {
                outlineRoot = existing.gameObject;
            }

            Vector3[] offsets =
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

            outlineLabels = new TextMesh[offsets.Length];
            for (int i = 0; i < offsets.Length; i++)
            {
                Transform child = outlineRoot.transform.Find("Outline_" + i);
                GameObject outlineObject;
                if (child == null)
                {
                    outlineObject = new GameObject("Outline_" + i);
                    outlineObject.transform.SetParent(outlineRoot.transform, false);
                }
                else
                {
                    outlineObject = child.gameObject;
                }

                outlineObject.transform.localPosition = offsets[i];
                outlineObject.transform.localRotation = Quaternion.identity;
                outlineObject.transform.localScale = Vector3.one;
                TextMesh outline = outlineObject.GetComponent<TextMesh>();
                if (outline == null)
                {
                    outline = outlineObject.AddComponent<TextMesh>();
                }

                CopyLabelStyle(outline);
                outlineLabels[i] = outline;
            }
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

        private void SetPressed(bool pressed)
        {
            if (isPressed == pressed)
            {
                return;
            }

            isPressed = pressed;
            SetLabelColor(pressed ? IMPERIAL_RED : Color.white);
        }

        private void SetTextOutlineVisible(bool isVisible)
        {
            if (outlineLabels == null)
            {
                return;
            }

            for (int i = 0; i < outlineLabels.Length; i++)
            {
                if (outlineLabels[i] != null)
                {
                    outlineLabels[i].gameObject.SetActive(isVisible);
                }
            }
        }
    }
}

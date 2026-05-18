using UnityEngine;

namespace MMBGame
{
    public partial class BoardPieceProfileDisplay
    {
        private void EnsureBindings()
        {
            profileTarget = EnsureTransform("Profile_Position");
            profileImageRenderer = EnsureProfileImageRenderer(profileTarget);
            nameText = EnsureText("Name_Position", nameText);
            goldText = EnsureText("GoldperTurn_Position", goldText);
            supportText = EnsureText("Support_Position", supportText);
            moveText = EnsureText("PieceMove_Position", moveText);
        }

        private Transform EnsureTransform(string objectName)
        {
            GameObject target = GameObject.Find(objectName);
            if (target == null)
            {
                return null;
            }

            return target.transform;
        }

        private SpriteRenderer EnsureProfileImageRenderer(Transform target)
        {
            if (target == null)
            {
                return null;
            }

            Transform existing = target.Find("ProfileImage");
            GameObject imageObject = existing != null ? existing.gameObject : new GameObject("ProfileImage");
            imageObject.transform.SetParent(target, false);
            imageObject.transform.localPosition = new Vector3(0f, 0f, -0.1f);
            SpriteRenderer renderer = imageObject.GetComponent<SpriteRenderer>();
            if (renderer == null)
            {
                renderer = imageObject.AddComponent<SpriteRenderer>();
            }

            renderer.sortingOrder = SORTING_ORDER;
            return renderer;
        }

        private TextMesh EnsureText(string objectName, TextMesh currentText)
        {
            if (currentText != null)
            {
                FitTextScale(currentText.transform);
                ConfigureProfileText(currentText);
                return currentText;
            }

            GameObject target = GameObject.Find(objectName);
            if (target == null)
            {
                return null;
            }

            GameObject textObject = new GameObject("Text");
            textObject.transform.SetParent(target.transform, false);
            textObject.transform.localPosition = new Vector3(0f, 0f, -0.1f);
            FitTextScale(textObject.transform);
            TextMesh textMesh = textObject.AddComponent<TextMesh>();
            textMesh.anchor = TextAnchor.MiddleCenter;
            textMesh.alignment = TextAlignment.Center;
            textMesh.fontSize = 40;
            textMesh.characterSize = BASE_TEXT_CHARACTER_SIZE;
            textMesh.color = Color.white;
            TextMeshFontApplier.Apply(textMesh);
            MeshRenderer renderer = textObject.GetComponent<MeshRenderer>();
            if (renderer != null)
            {
                renderer.sortingOrder = SORTING_ORDER + 1;
            }

            return textMesh;
        }

        private void ConfigureProfileText(TextMesh textMesh)
        {
            if (textMesh == null)
            {
                return;
            }

            textMesh.color = Color.white;
            TextMeshFontApplier.Apply(textMesh);
        }

        private void FitTextScale(Transform textTransform)
        {
            if (textTransform == null || textTransform.parent == null)
            {
                return;
            }

            Vector3 parentScale = textTransform.parent.lossyScale;
            textTransform.localScale = new Vector3(GetInverseScale(parentScale.x), GetInverseScale(parentScale.y), 1f);
        }

        private float GetInverseScale(float scale)
        {
            return Mathf.Abs(scale) > 0.001f ? 1f / scale : 1f;
        }

        private void SetText(TextMesh textMesh, string value)
        {
            if (textMesh != null)
            {
                textMesh.text = value;
                FitTextToPosition(textMesh);
            }
        }

        private void FitTextToPosition(TextMesh textMesh)
        {
            if (textMesh == null || textMesh.transform.parent == null)
            {
                return;
            }

            FitTextScale(textMesh.transform);
            ConfigureProfileText(textMesh);
            textMesh.characterSize = BASE_TEXT_CHARACTER_SIZE;

            MeshRenderer renderer = textMesh.GetComponent<MeshRenderer>();
            if (renderer == null)
            {
                return;
            }

            Vector2 boxSize = ResolvePositionBoxSize(textMesh.transform.parent);
            if (boxSize.x <= 0f || boxSize.y <= 0f)
            {
                return;
            }

            Bounds bounds = renderer.bounds;
            if (bounds.size.x <= 0f || bounds.size.y <= 0f)
            {
                return;
            }

            float widthRatio = boxSize.x * TEXT_BOX_WIDTH_PADDING / bounds.size.x;
            float heightRatio = boxSize.y * TEXT_BOX_HEIGHT_PADDING / bounds.size.y;
            float ratio = Mathf.Min(1f, widthRatio, heightRatio);
            textMesh.characterSize = Mathf.Max(MIN_TEXT_CHARACTER_SIZE, BASE_TEXT_CHARACTER_SIZE * ratio);
        }

        private Vector2 ResolvePositionBoxSize(Transform target)
        {
            SpriteRenderer spriteRenderer = target.GetComponent<SpriteRenderer>();
            if (spriteRenderer != null)
            {
                Bounds bounds = spriteRenderer.bounds;
                return new Vector2(bounds.size.x, bounds.size.y);
            }

            BoxCollider2D collider = target.GetComponent<BoxCollider2D>();
            if (collider != null)
            {
                Vector2 scaledSize = Vector2.Scale(collider.size, target.lossyScale);
                return new Vector2(Mathf.Abs(scaledSize.x), Mathf.Abs(scaledSize.y));
            }

            Vector3 scale = target.lossyScale;
            return new Vector2(Mathf.Abs(scale.x), Mathf.Abs(scale.y));
        }
    }
}

using UnityEngine;

namespace MMBGame
{
    public partial class TurnStatusDisplay
    {
        private void CreateBackground()
        {
            GameObject backgroundObject = new GameObject("Background");
            backgroundObject.transform.SetParent(transform, false);
            backgroundObject.transform.localPosition = new Vector3(backgroundSize.x * 0.5f - padding.x, 0f, 0.02f);
            background = backgroundObject.AddComponent<SpriteRenderer>();
            background.sprite = RuntimeUiFactory.GetPixelSprite();
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
            iconBackground.sprite = RuntimeUiFactory.GetPixelSprite();
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
            tooltipBackground.sprite = RuntimeUiFactory.GetPixelSprite();
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

    }
}

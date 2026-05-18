using System;
using UnityEngine;

namespace MMBGame
{
    public partial class PrisonerPanel
    {
        private void CreateBackground(Vector3 center)
        {
            GameObject bg = new GameObject("PrisonerBG");
            bg.transform.position = new Vector3(center.x, center.y, center.z - 0.2f);
            bg.transform.localScale = new Vector3(PANEL_W, PANEL_H, 1f);

            SpriteRenderer sr = bg.AddComponent<SpriteRenderer>();
            sr.sprite = RuntimeUiFactory.GetPixelSprite();
            sr.color = new Color(0.12f, 0.08f, 0.08f, 0.95f);
            sr.sortingOrder = SORT_BG;

            backgroundCollider = bg.AddComponent<BoxCollider2D>();
            backgroundCollider.size = Vector2.one;
            objects.Add(bg);

            CreateLabel("포로 처리",
                new Vector3(center.x, center.y + PANEL_H * 0.5f - 0.7f, center.z - 0.3f),
                SORT_TEXT, 0.26f);
        }

        private void CreatePieceDisplay(ChessPiece piece, Vector3 center, BoardPieceSetupManager setupManager)
        {
            float iconY = center.y + 0.7f;
            Vector3 iconPos = new Vector3(center.x - 2.0f, iconY, center.z - 0.3f);

            GameObject iconObj = new GameObject("PieceIcon");
            iconObj.transform.position = iconPos;
            SpriteRenderer sr = iconObj.AddComponent<SpriteRenderer>();

            Sprite sprite = GetPieceSprite(piece, setupManager);
            if (sprite != null)
            {
                sr.sprite = sprite;
                float spriteSize = Mathf.Max(sprite.bounds.size.x, sprite.bounds.size.y);
                if (spriteSize > 0.001f)
                {
                    iconObj.transform.localScale = Vector3.one * (1.6f / spriteSize);
                }
            }
            else
            {
                sr.sprite = RuntimeUiFactory.GetPixelSprite();
                sr.color = piece.color == PieceColor.White
                    ? new Color(0.92f, 0.88f, 0.75f)
                    : new Color(0.28f, 0.22f, 0.18f);
                iconObj.transform.localScale = Vector3.one * 1.6f;
            }

            sr.sortingOrder = SORT_ICON;
            objects.Add(iconObj);

            CreateLabel(GetPieceName(piece),
                new Vector3(center.x + 0.6f, iconY, center.z - 0.3f),
                SORT_TEXT, 0.22f);

            CreateLabel("처우를 결정하십시오",
                new Vector3(center.x, center.y + 0.05f, center.z - 0.3f),
                SORT_TEXT, 0.15f);
        }

        private void CreateChoiceButtons(ChessPiece piece, PieceColor attackerColor, Vector3 center)
        {
            float buttonY = center.y - 1.2f;
            float[] xOffsets = { -2.6f, 0f, 2.6f };
            string[] labels = { "석방", "처형", "몸값" };
            int honorValue = BoardEvaluator.GetPieceValue(piece.type) * 5;
            int ransomValue = BoardEvaluator.GetPieceValue(piece.type) * 100;
            string[] subLabels = { "+" + honorValue + " 명예", "-" + honorValue + " 명예", "+" + ransomValue + " 금화" };
            Color[] colors =
            {
                new Color(0.20f, 0.48f, 0.25f, 1f),
                new Color(0.50f, 0.15f, 0.15f, 1f),
                new Color(0.50f, 0.44f, 0.10f, 1f),
            };

            Action[] actions =
            {
                () => ApplyChoice(attackerColor, 0, piece),
                () => ApplyChoice(attackerColor, 1, piece),
                () => ApplyChoice(attackerColor, 2, piece),
            };

            for (int i = 0; i < 3; i++)
            {
                Vector3 btnPos = new Vector3(center.x + xOffsets[i], buttonY, center.z - 0.3f);
                CreateButton(labels[i], subLabels[i], btnPos, colors[i], actions[i]);
            }
        }

        private void CreateButton(string label, string subLabel, Vector3 pos, Color color, Action callback)
        {
            GameObject btn = new GameObject("Btn_" + label);
            btn.transform.position = pos;
            btn.transform.localScale = new Vector3(BUTTON_W, BUTTON_H, 1f);

            SpriteRenderer sr = btn.AddComponent<SpriteRenderer>();
            sr.sprite = RuntimeUiFactory.GetPixelSprite();
            sr.color = color;
            sr.sortingOrder = SORT_BUTTON;

            BoxCollider2D col = btn.AddComponent<BoxCollider2D>();
            col.size = Vector2.one;
            objects.Add(btn);
            buttons.Add((col, callback));

            CreateLabel(label,
                new Vector3(pos.x, pos.y + 0.12f, pos.z - 0.1f),
                SORT_TEXT, 0.20f);

            CreateLabel(subLabel,
                new Vector3(pos.x, pos.y - 0.22f, pos.z - 0.1f),
                SORT_TEXT, 0.13f);
        }

        private Sprite GetPieceSprite(ChessPiece piece, BoardPieceSetupManager setupManager)
        {
            if (setupManager == null)
            {
                return null;
            }

            PieceSetupDefinition def = setupManager.GetPieceDefinition(piece.color, piece.side, piece.type, piece.lane);
            return def?.sprite;
        }

        private string GetPieceName(ChessPiece piece)
        {
            return PieceDisplayNames.GetSimpleName(piece);
        }

        private void CreateLabel(string text, Vector3 pos, int sortOrder, float charSize = 0.15f)
        {
            RuntimeUiFactory.CreateLabel("Label", text, pos, sortOrder, charSize, objects);
        }
    }
}

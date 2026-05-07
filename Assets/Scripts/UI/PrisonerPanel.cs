using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

namespace MMBGame
{
    public class PrisonerPanel : MonoBehaviour
    {
        private static PrisonerPanel instance;

        public static bool IsOpen { get; private set; }

        private const int SORT_BG = 70;
        private const int SORT_ICON = 71;
        private const int SORT_BUTTON = 72;
        private const int SORT_TEXT = 73;
        private const float PANEL_W = 8.0f;
        private const float PANEL_H = 4.8f;
        private const float BUTTON_W = 2.2f;
        private const float BUTTON_H = 0.80f;

        private readonly List<(BoxCollider2D col, Action callback)> buttons =
            new List<(BoxCollider2D, Action)>();
        private readonly List<GameObject> objects = new List<GameObject>();
        private BoxCollider2D backgroundCollider;

        public static void Show(PieceColor attackerColor, List<ChessPiece> capturedPieces,
            BoardPieceSetupManager setupManager)
        {
            if (instance == null)
            {
                instance = new GameObject("PrisonerPanel").AddComponent<PrisonerPanel>();
            }

            instance.Open(attackerColor, capturedPieces, setupManager);
        }

        public static void Hide()
        {
            instance?.Close();
        }

        private void Open(PieceColor attackerColor, List<ChessPiece> capturedPieces,
            BoardPieceSetupManager setupManager)
        {
            Close();

            if (capturedPieces == null || capturedPieces.Count == 0)
            {
                AdvanceTurn();
                return;
            }

            IsOpen = true;
            ChessPiece piece = capturedPieces[0];
            Vector3 center = ScreenCenter();

            CreateBackground(center);
            CreatePieceDisplay(piece, center, setupManager);
            CreateChoiceButtons(piece, attackerColor, center);
        }

        private void Update()
        {
            Mouse mouse = Mouse.current;
            if (mouse == null || !mouse.leftButton.wasPressedThisFrame || Camera.main == null)
            {
                return;
            }

            Vector2 sp = mouse.position.ReadValue();
            Vector3 wp = Camera.main.ScreenToWorldPoint(
                new Vector3(sp.x, sp.y, -Camera.main.transform.position.z));

            for (int i = 0; i < buttons.Count; i++)
            {
                if (buttons[i].col != null && buttons[i].col.OverlapPoint(wp))
                {
                    Action cb = buttons[i].callback;
                    Close();
                    cb?.Invoke();
                    return;
                }
            }
        }

        private void Close()
        {
            IsOpen = false;
            for (int i = objects.Count - 1; i >= 0; i--)
            {
                if (objects[i] != null)
                {
                    Destroy(objects[i]);
                }
            }

            objects.Clear();
            buttons.Clear();
            backgroundCollider = null;
        }

        private void CreateBackground(Vector3 center)
        {
            GameObject bg = new GameObject("PrisonerBG");
            bg.transform.position = new Vector3(center.x, center.y, center.z - 0.2f);
            bg.transform.localScale = new Vector3(PANEL_W, PANEL_H, 1f);

            SpriteRenderer sr = bg.AddComponent<SpriteRenderer>();
            sr.sprite = MakePixelSprite();
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
                sr.sprite = MakePixelSprite();
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
            string[] subLabels = { "+20 명예", "-20 명예", "+200 금화" };
            Color[] colors =
            {
                new Color(0.20f, 0.48f, 0.25f, 1f),
                new Color(0.50f, 0.15f, 0.15f, 1f),
                new Color(0.50f, 0.44f, 0.10f, 1f),
            };

            Action[] actions =
            {
                () => ApplyChoice(attackerColor, 0),
                () => ApplyChoice(attackerColor, 1, piece),
                () => ApplyChoice(attackerColor, 2),
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
            sr.sprite = MakePixelSprite();
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

        private void ApplyChoice(PieceColor attackerColor, int choice, ChessPiece capturedPiece = null)
        {
            PoliticsManager pm = FindFirstObjectByType<PoliticsManager>();
            if (pm != null)
            {
                PlayerState player = pm.GetCurrentPlayer(attackerColor);
                if (player != null)
                {
                    switch (choice)
                    {
                        case 0: player.AddHonor(20); break;
                        case 1:
                            KingStateEffectApplier.Instance?.SuppressNextHonorDecreasePenalty(attackerColor);
                            player.AddHonor(-20);
                            ApplyDictatorshipPrisonerExecutionBonus(attackerColor, capturedPiece);
                            break;
                        case 2: player.AddGold(200); break;
                    }
                }
            }

            AdvanceTurn();
        }

        private void ApplyDictatorshipPrisonerExecutionBonus(PieceColor attackerColor, ChessPiece capturedPiece)
        {
            if (!KingStateEvaluator.IsDictatorship(attackerColor) || capturedPiece == null)
            {
                return;
            }

            BoardManager boardManager = FindFirstObjectByType<BoardManager>();
            if (boardManager == null || boardManager.BoardState == null)
            {
                return;
            }

            int bonus = Mathf.CeilToInt(BoardEvaluator.GetPieceValue(capturedPiece.type) / 2f);
            if (bonus <= 0)
            {
                return;
            }

            List<ChessPiece> pieces = boardManager.BoardState.GetAllPieces();
            for (int i = 0; i < pieces.Count; i++)
            {
                ChessPiece piece = pieces[i];
                if (piece != null && piece.color == attackerColor)
                {
                    piece.support += bonus;
                }
            }
        }

        private void AdvanceTurn()
        {
            TurnManager tm = FindFirstObjectByType<TurnManager>();
            tm?.EndPhase();
        }

        private Vector3 ScreenCenter()
        {
            if (Camera.main == null)
            {
                return Vector3.zero;
            }

            return Camera.main.ScreenToWorldPoint(new Vector3(
                Screen.width * 0.5f, Screen.height * 0.5f,
                -Camera.main.transform.position.z));
        }

        private Sprite GetPieceSprite(ChessPiece piece, BoardPieceSetupManager setupManager)
        {
            if (setupManager == null)
            {
                return null;
            }

            PieceSetupDefinition def = setupManager.GetPieceDefinition(piece.color, piece.side, piece.type);
            return def?.sprite;
        }

        private string GetPieceName(ChessPiece piece)
        {
            return (piece.color == PieceColor.White ? "백" : "흑") + " " + GetTypeName(piece.type);
        }

        private string GetTypeName(PieceType type)
        {
            switch (type)
            {
                case PieceType.Pawn: return "폰";
                case PieceType.Rook: return "룩";
                case PieceType.Knight: return "나이트";
                case PieceType.Bishop: return "비숍";
                case PieceType.Queen: return "퀸";
                case PieceType.King: return "킹";
                default: return type.ToString();
            }
        }

        private void CreateLabel(string text, Vector3 pos, int sortOrder, float charSize = 0.15f)
        {
            GameObject obj = new GameObject("Label");
            obj.transform.position = pos;

            TextMesh tm = obj.AddComponent<TextMesh>();
            tm.text = text;
            tm.anchor = TextAnchor.MiddleCenter;
            tm.alignment = TextAlignment.Center;
            tm.fontSize = 36;
            tm.characterSize = charSize;
            tm.color = Color.white;
            TextMeshFontApplier.Apply(tm);

            MeshRenderer mr = obj.GetComponent<MeshRenderer>();
            if (mr != null)
            {
                mr.sortingOrder = sortOrder;
            }

            objects.Add(obj);
        }

        private Sprite MakePixelSprite()
        {
            Texture2D tex = new Texture2D(1, 1);
            tex.SetPixel(0, 0, Color.white);
            tex.Apply();
            return Sprite.Create(tex, new Rect(0, 0, 1, 1), new Vector2(0.5f, 0.5f), 1f);
        }
    }
}

using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

namespace MMBGame
{
    public class FrontDeployPanel : MonoBehaviour
    {
        private static FrontDeployPanel instance;

        public static bool IsOpen { get; private set; }

        private const int SORT_BG = 60;
        private const int SORT_BUTTON = 61;
        private const int SORT_TEXT = 62;
        private const float CELL_SIZE = 2.2f;
        private const int MAX_PER_ROW = 4;

        private readonly List<(BoxCollider2D col, ChessPiece piece)> buttons =
            new List<(BoxCollider2D, ChessPiece)>();
        private readonly List<GameObject> objects = new List<GameObject>();
        private BoxCollider2D backgroundCollider;
        private Action<ChessPiece> onSelected;

        public static void Show(PieceColor color, BoardState boardState,
            BoardPieceSetupManager setupManager, Action<ChessPiece> onPieceSelected)
        {
            if (instance == null)
            {
                instance = new GameObject("FrontDeployPanel").AddComponent<FrontDeployPanel>();
            }

            instance.Open(color, boardState, setupManager, onPieceSelected);
            IsOpen = true;
        }

        public static void Hide()
        {
            instance?.Close();
        }

        private void Open(PieceColor color, BoardState boardState,
            BoardPieceSetupManager setupManager, Action<ChessPiece> onPieceSelected)
        {
            Close();
            onSelected = onPieceSelected;

            List<ChessPiece> pieces = CollectOffBoard(color, boardState);
            Vector3 center = ScreenCenter();

            int cols = Mathf.Max(1, Mathf.Min(pieces.Count, MAX_PER_ROW));
            int rows = Mathf.Max(1, Mathf.CeilToInt(pieces.Count / (float)MAX_PER_ROW));
            float panelW = cols * CELL_SIZE + 0.8f;
            float panelH = rows * CELL_SIZE + 1.4f;

            CreateBackground(center, panelW, panelH);
            PlacePieces(pieces, center, rows, cols, setupManager);
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
                    ChessPiece selected = buttons[i].piece;
                    Action<ChessPiece> cb = onSelected;
                    Close();
                    cb?.Invoke(selected);
                    return;
                }
            }

            if (backgroundCollider != null && !backgroundCollider.OverlapPoint(wp))
            {
                Close();
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

        private List<ChessPiece> CollectOffBoard(PieceColor color, BoardState boardState)
        {
            List<ChessPiece> result = new List<ChessPiece>();
            if (boardState?.offBoardPieces == null)
            {
                return result;
            }

            for (int i = 0; i < boardState.offBoardPieces.Count; i++)
            {
                ChessPiece p = boardState.offBoardPieces[i];
                if (p != null && p.color == color)
                {
                    result.Add(p);
                }
            }

            return result;
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

        private void CreateBackground(Vector3 center, float w, float h)
        {
            GameObject bg = new GameObject("FrontDeployBG");
            bg.transform.position = new Vector3(center.x, center.y, center.z - 0.2f);
            bg.transform.localScale = new Vector3(w, h, 1f);

            SpriteRenderer sr = bg.AddComponent<SpriteRenderer>();
            sr.sprite = MakePixelSprite();
            sr.color = new Color(0.18f, 0.18f, 0.18f, 0.92f);
            sr.sortingOrder = SORT_BG;

            backgroundCollider = bg.AddComponent<BoxCollider2D>();
            backgroundCollider.size = Vector2.one;
            objects.Add(bg);

            // Title
            CreateLabel("전방배치 선택", new Vector3(center.x, center.y + h * 0.5f - 0.7f, center.z - 0.3f), SORT_TEXT, 0.22f);
        }

        private void PlacePieces(List<ChessPiece> pieces, Vector3 center, int rows, int cols,
            BoardPieceSetupManager setupManager)
        {
            if (pieces.Count == 0)
            {
                CreateLabel("후방배치 기물 없음", center, SORT_TEXT, 0.2f);
                return;
            }

            float startY = center.y + (rows - 1) * CELL_SIZE * 0.5f - 0.3f;

            for (int i = 0; i < pieces.Count; i++)
            {
                int row = i / MAX_PER_ROW;
                int col = i % MAX_PER_ROW;
                int countInRow = Mathf.Min(MAX_PER_ROW, pieces.Count - row * MAX_PER_ROW);
                float rowW = (countInRow - 1) * CELL_SIZE;
                float x = center.x - rowW * 0.5f + col * CELL_SIZE;
                float y = startY - row * CELL_SIZE;
                Vector3 pos = new Vector3(x, y, center.z - 0.3f);
                CreatePieceButton(pieces[i], pos, setupManager);
            }
        }

        private void CreatePieceButton(ChessPiece piece, Vector3 pos, BoardPieceSetupManager setupManager)
        {
            GameObject btn = new GameObject(piece.pieceName ?? "Piece");
            btn.transform.position = pos;

            SpriteRenderer sr = btn.AddComponent<SpriteRenderer>();
            Sprite sprite = GetPieceSprite(piece, setupManager);

            if (sprite != null)
            {
                sr.sprite = sprite;
                float spriteSize = Mathf.Max(sprite.bounds.size.x, sprite.bounds.size.y);
                if (spriteSize > 0.001f)
                {
                    float scale = (CELL_SIZE * 0.75f) / spriteSize;
                    btn.transform.localScale = Vector3.one * scale;
                }
            }
            else
            {
                sr.sprite = MakePixelSprite();
                sr.color = piece.color == PieceColor.White
                    ? new Color(0.92f, 0.88f, 0.75f)
                    : new Color(0.28f, 0.22f, 0.18f);
                btn.transform.localScale = Vector3.one * (CELL_SIZE * 0.65f);
            }

            sr.sortingOrder = SORT_BUTTON;

            BoxCollider2D col = btn.AddComponent<BoxCollider2D>();
            col.size = Vector2.one * (CELL_SIZE * 0.9f) / btn.transform.localScale.x;

            CreateLabel(GetPieceName(piece),
                new Vector3(pos.x, pos.y - CELL_SIZE * 0.48f, pos.z),
                SORT_TEXT, 0.14f);

            objects.Add(btn);
            buttons.Add((col, piece));
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
            // pixelsPerUnit=1 so sprite is 1 world unit — scale handles actual size
            return Sprite.Create(tex, new Rect(0, 0, 1, 1), new Vector2(0.5f, 0.5f), 1f);
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
    }
}

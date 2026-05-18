using System;
using System.Collections.Generic;
using UnityEngine;

namespace MMBGame
{
    public partial class PrisonerPanel : MonoBehaviour
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
            Vector3 center = RuntimeUiFactory.GetScreenCenter();

            CreateBackground(center);
            CreatePieceDisplay(piece, center, setupManager);
            CreateChoiceButtons(piece, attackerColor, center);
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
    }
}

using UnityEngine;

namespace MMBGame
{
    public partial class BoardPieceProfileDisplay : MonoBehaviour
    {
        private const string PROFILE_PATH = "Assets/Prefabs/Profile/";
        private const int SORTING_ORDER = 40;
        private const float BASE_TEXT_CHARACTER_SIZE = 0.18f;
        private const float MIN_TEXT_CHARACTER_SIZE = 0.055f;
        private const float TEXT_BOX_WIDTH_PADDING = 0.88f;
        private const float TEXT_BOX_HEIGHT_PADDING = 0.78f;

        private Transform profileTarget;
        private SpriteRenderer profileImageRenderer;
        private TextMesh nameText;
        private TextMesh goldText;
        private TextMesh supportText;
        private TextMesh moveText;
        private PoliticsManager politicsManager;

        public void Show(BoardManager boardManager, BoardPieceVisual visual)
        {
            if (boardManager == null || boardManager.BoardState == null || visual == null)
            {
                Clear();
                return;
            }

            ChessPiece piece = boardManager.BoardState.GetPiece(visual.File, visual.Rank);
            if (piece == null)
            {
                Clear();
                return;
            }

            EnsureBindings();
            SetProfile(piece);
            SetText(nameText, PieceDisplayNames.GetDisplayName(piece));
            if (piece.type == PieceType.King)
            {
                PlayerState player = GetPlayerState(piece.color);
                int totalSupport = boardManager.BoardState != null
                    ? KingStateEvaluator.ComputeTotalSupport(piece.color, boardManager.BoardState)
                    : 0;
                KingState state = player != null && boardManager.BoardState != null
                    ? KingStateEvaluator.Evaluate(player, boardManager.BoardState)
                    : KingState.Neutral;
                SetText(goldText, player != null ? player.gold.ToString() : "0");
                SetText(supportText, totalSupport.ToString());
                SetText(moveText, GetKingStateText(state, totalSupport, player != null ? player.honor : 0));
            }
            else
            {
                SetText(goldText, piece.GetEffectiveTax().ToString());
                SetText(supportText, piece.support.ToString());
                SetText(moveText, GetMoveText(piece));
            }
        }

        public void Clear()
        {
            EnsureBindings();
            if (profileImageRenderer != null)
            {
                profileImageRenderer.sprite = null;
            }

            SetText(nameText, string.Empty);
            SetText(goldText, string.Empty);
            SetText(supportText, string.Empty);
            SetText(moveText, string.Empty);
        }
    }
}

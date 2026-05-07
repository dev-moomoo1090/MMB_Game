using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace MMBGame
{
    public class BoardPieceProfileDisplay : MonoBehaviour
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
            SetText(nameText, GetDisplayName(piece));
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

        private void SetProfile(ChessPiece piece)
        {
            if (profileImageRenderer == null)
            {
                return;
            }

            Sprite sprite = LoadProfileSprite(GetProfileAssetPath(piece));
            if (sprite == null)
            {
                sprite = LoadProfileSprite(GetSideProfileAssetPath(piece));
            }

            if (sprite == null)
            {
                sprite = LoadProfileSprite(GetFallbackProfileAssetPath(piece.side));
            }

            profileImageRenderer.sprite = sprite;
            FitProfileImage();
        }

        private void FitProfileImage()
        {
            if (profileTarget == null || profileImageRenderer == null || profileImageRenderer.sprite == null)
            {
                return;
            }

            Bounds bounds = profileImageRenderer.sprite.bounds;
            if (bounds.size.x <= 0f || bounds.size.y <= 0f)
            {
                return;
            }

            Vector3 parentScale = profileTarget.lossyScale;
            float targetWidth = Mathf.Max(0.1f, parentScale.x);
            float targetHeight = Mathf.Max(0.1f, parentScale.y);
            float scale = Mathf.Min(targetWidth / bounds.size.x / parentScale.x, targetHeight / bounds.size.y / parentScale.y);
            profileImageRenderer.transform.localScale = new Vector3(scale, scale, 1f);
        }

        private Sprite LoadProfileSprite(string path)
        {
#if UNITY_EDITOR
            return AssetDatabase.LoadAssetAtPath<Sprite>(path);
#else
            return null;
#endif
        }

        private string GetProfileAssetPath(ChessPiece piece)
        {
            string laneName = piece.lane == PieceLane.None ? string.Empty : "_" + piece.lane;
            return PROFILE_PATH + piece.color + "_" + piece.type + laneName + "_Profile.png";
        }

        private string GetSideProfileAssetPath(ChessPiece piece)
        {
            string sideName = piece.side == PieceSide.None ? string.Empty : "_" + piece.side;
            return PROFILE_PATH + piece.color + "_" + piece.type + sideName + "_Profile.png";
        }

        private string GetFallbackProfileAssetPath(PieceSide side)
        {
            if (side == PieceSide.Kingside)
            {
                return PROFILE_PATH + "Black_Pawn_Kingside_Profile.png";
            }

            return PROFILE_PATH + "Black_Pawn_Queenside_Profile.png";
        }

        private string GetDisplayName(ChessPiece piece)
        {
            string laneName = GetLaneName(piece.lane);
            if (!string.IsNullOrEmpty(laneName))
            {
                return GetColorName(piece.color) + " " + laneName + " " + GetTypeName(piece.type);
            }

            return GetColorName(piece.color) + " " + GetSideName(piece.side) + " " + GetTypeName(piece.type);
        }

        private string GetColorName(PieceColor color)
        {
            if (color == PieceColor.White)
            {
                return "백";
            }

            return color == PieceColor.Black ? "흑" : "무색";
        }

        private string GetSideName(PieceSide side)
        {
            if (side == PieceSide.Kingside)
            {
                return "킹사이드";
            }

            return side == PieceSide.Queenside ? "퀸사이드" : string.Empty;
        }

        private string GetLaneName(PieceLane lane)
        {
            return lane == PieceLane.None ? string.Empty : lane.ToString() + "열";
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
                case PieceType.Barricade: return "바리케이드";
                case PieceType.Trebuchet: return "트레뷰셋";
                default: return type.ToString();
            }
        }

        private string GetMoveText(ChessPiece piece)
        {
            return GetTypeName(piece.type);
        }

        private PlayerState GetPlayerState(PieceColor color)
        {
            if (politicsManager == null)
            {
                politicsManager = Object.FindFirstObjectByType<PoliticsManager>();
            }

            return politicsManager?.GetCurrentPlayer(color);
        }

        private string GetKingStateText(KingState state, int totalSupport, int honor)
        {
            string stateName = GetKingStateName(state);
            string supportStr = totalSupport >= 0 ? "+" + totalSupport : totalSupport.ToString();
            string honorStr = honor >= 0 ? "+" + honor : honor.ToString();
            return stateName + " : " + supportStr + " / " + honorStr;
        }

        private string GetKingStateName(KingState state)
        {
            switch (state)
            {
                case KingState.Sage: return "성군";
                case KingState.Autocrat: return "독재";
                case KingState.DarkKing: return "암군";
                case KingState.Tyrant: return "폭군";
                default: return "중립";
            }
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
